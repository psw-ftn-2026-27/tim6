import { httpResource } from '@angular/common/http';
import { Component, DestroyRef, computed, effect, inject, input, signal, untracked } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Auth } from '../../../../core/auth/auth';
import { ConversationChangedEvent } from '../../../../shared/api/conversation-changed-event';
import {
  ConversationChat,
  ConversationChatMessage,
  ConversationChatParticipant,
  MuteParticipantRequest,
} from '../../../../shared/conversation-chat/conversation-chat/conversation-chat';
import { serverMessage } from '../../../../shared/util/server-message';
import {
  ChatMessageDto,
  ConversationDto,
  MessageCursorDto,
  MessagePageDto,
} from '../../api/exploration-api-types';
import { TourConversationMessaging } from '../tour-conversation-messaging';

type MessageRequestKind = 'latest' | 'older' | 'catchup';

interface MessageRequest {
  cursor: MessageCursorDto | null;
  kind: MessageRequestKind;
  nonce: number;
}

@Component({
  selector: 'app-tour-conversation',
  imports: [ConversationChat],
  templateUrl: './tour-conversation.html',
  styleUrl: './tour-conversation.scss',
})
export class TourConversation {
  readonly id = input.required<string>();

  private readonly auth = inject(Auth);
  private readonly messaging = inject(TourConversationMessaging);
  private readonly destroyRef = inject(DestroyRef);

  private readonly conversationId = signal<string | null>(null);
  protected readonly conversation = httpResource<ConversationDto>(() => {
    const conversationId = this.conversationId();
    return conversationId === null ? undefined : `/api/exploration/conversations/${conversationId}`;
  });

  private readonly messageRequest = signal<MessageRequest>({ cursor: null, kind: 'latest', nonce: 0 });
  protected readonly messagePage = httpResource<MessagePageDto>(() => {
    const conversationId = this.conversationId();
    if (conversationId === null) return undefined;
    const request = this.messageRequest();
    const parameters = new URLSearchParams({ pageSize: '20', request: request.nonce.toString() });
    if (request.cursor !== null) {
      parameters.set('cursorCreatedAt', request.cursor.createdAt);
      parameters.set('cursorId', request.cursor.id);
      parameters.set('afterCursor', (request.kind === 'catchup').toString());
    }
    return `/api/exploration/conversations/${conversationId}/messages?${parameters.toString()}`;
  });

  private readonly messages = signal<ChatMessageDto[]>([]);
  protected readonly hasMore = signal(false);
  protected readonly pending = signal(false);
  protected readonly opening = signal(true);
  protected readonly error = signal<string | null>(null);
  private readonly liveError = signal<string | null>(null);
  protected readonly composerResetKey = signal(0);
  private readonly muteClock = signal(Date.now());

  private processedPage: MessagePageDto | undefined;
  private openedTourId: string | null = null;
  private connection: HubConnection | null = null;
  private muteExpiryTimer: number | null = null;
  private destroyed = false;

  protected readonly currentUserId = computed(() => this.auth.user()?.id ?? '');
  protected readonly conversationTitle = computed(
    () => `Tour chat: ${this.conversation.value()?.tourName ?? 'loading...'}`,
  );
  private readonly participantLabels = computed(
    () => new Map(
      (this.conversation.value()?.participants ?? []).map(
        (participant) => [participant.userId, participant.userEmail ?? 'User'],
      ),
    ),
  );
  private readonly currentParticipant = computed(() =>
    this.conversation.value()?.participants.find(
      (participant) => participant.userId === this.currentUserId(),
    ),
  );
  private readonly canModerate = computed(() => this.currentParticipant()?.role === 'Privileged');
  protected readonly isMuted = computed(() => {
    const participant = this.currentParticipant();
    const now = this.muteClock();
    return participant?.isPermanentlyMuted === true ||
      (participant?.mutedUntil !== null &&
        participant?.mutedUntil !== undefined &&
        new Date(participant.mutedUntil).getTime() > now);
  });
  protected readonly muteDescription = computed(() => {
    const participant = this.currentParticipant();
    if (participant?.isPermanentlyMuted) return 'permanently';
    return !this.isMuted() || participant?.mutedUntil === null || participant?.mutedUntil === undefined
      ? null
      : `until ${new Date(participant.mutedUntil).toLocaleString()}`;
  });
  protected readonly chatParticipants = computed<ConversationChatParticipant[]>(() => {
    const now = this.muteClock();
    return (this.conversation.value()?.participants ?? []).map((participant) => ({
        userId: participant.userId,
        label:
          participant.userId === this.currentUserId()
            ? 'You'
            : (participant.userEmail ?? 'User'),
        isPrivileged: participant.role === 'Privileged',
        mutedUntil:
          participant.mutedUntil !== null && new Date(participant.mutedUntil).getTime() > now
            ? participant.mutedUntil
            : null,
        isPermanentlyMuted: participant.isPermanentlyMuted,
        canModerate:
          this.canModerate() &&
          participant.role === 'Participant' &&
          participant.userId !== this.currentUserId(),
      }));
  });
  protected readonly chatMessages = computed<ConversationChatMessage[]>(() => {
    const messages = this.messages();
    const participants = new Map(
      this.chatParticipants().map((participant) => [participant.userId, participant]),
    );
    const latestMessageByAuthor = new Map<string, string>();
    messages.forEach((message) => latestMessageByAuthor.set(message.authorId, message.id));
    return messages.map((message) => {
      const participant = participants.get(message.authorId);
      return {
        id: message.id,
        authorId: message.authorId,
        authorLabel:
          message.authorId === this.currentUserId()
            ? 'You'
            : (this.participantLabels().get(message.authorId) ?? 'User'),
        content: message.content,
        createdAt: message.createdAt,
        isDeleted: message.isDeleted,
        canDelete:
          !message.isDeleted &&
          (message.authorId === this.currentUserId() || this.canModerate()),
        moderationTarget:
          participant?.canModerate === true && latestMessageByAuthor.get(message.authorId) === message.id
            ? participant
            : null,
      };
    });
  });
  protected readonly displayError = computed(() => {
    if (this.error() !== null) return this.error();
    if (this.liveError() !== null) return this.liveError();
    if (this.conversation.error()) return serverMessage(this.conversation.error(), 'Could not load the conversation.');
    if (this.messagePage.error()) return serverMessage(this.messagePage.error(), 'Could not load messages.');
    return null;
  });

  constructor() {
    effect(() => {
      const tourId = this.id();
      if (tourId === this.openedTourId) return;
      this.openedTourId = tourId;
      untracked(() => void this.open(tourId));
    });

    effect(() => {
      const page = this.messagePage.value();
      if (page === undefined || page === this.processedPage) return;
      this.processedPage = page;
      const request = this.messageRequest();
      untracked(() => this.applyPage(page, request));
    });

    effect(() => {
      const now = Date.now();
      const nextMuteExpiry = (this.conversation.value()?.participants ?? [])
        .map((participant) => participant.mutedUntil)
        .filter((mutedUntil): mutedUntil is string => mutedUntil !== null)
        .map((mutedUntil) => new Date(mutedUntil).getTime())
        .filter((mutedUntil) => mutedUntil > now)
        .sort((left, right) => left - right)[0];
      if (this.muteExpiryTimer !== null) {
        window.clearTimeout(this.muteExpiryTimer);
        this.muteExpiryTimer = null;
      }
      this.muteClock.set(now);
      if (nextMuteExpiry === undefined) return;
      this.muteExpiryTimer = window.setTimeout(() => {
        this.muteExpiryTimer = null;
        this.muteClock.set(Date.now());
      }, nextMuteExpiry - now + 1);
    });

    this.destroyRef.onDestroy(() => {
      this.destroyed = true;
      if (this.muteExpiryTimer !== null) window.clearTimeout(this.muteExpiryTimer);
      void this.connection?.stop();
    });
  }

  protected loadOlder(): void {
    const oldest = this.messages()[0];
    if (oldest === undefined) return;
    this.messageRequest.set({
      cursor: { createdAt: oldest.createdAt, id: oldest.id },
      kind: 'older',
      nonce: this.messageRequest().nonce + 1,
    });
  }

  protected async send(content: string): Promise<void> {
    const conversationId = this.conversationId();
    if (conversationId === null) return;
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.messaging.send(conversationId, { content });
      this.composerResetKey.update((value) => value + 1);
      this.catchUp();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not send the message.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async delete(messageId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.messaging.delete(messageId);
      this.reloadLatest(true);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the message.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async mute(request: MuteParticipantRequest): Promise<void> {
    const conversationId = this.conversationId();
    if (conversationId === null) return;
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.messaging.mute(conversationId, request);
      this.conversation.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not mute the participant.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async unmute(participantId: string): Promise<void> {
    const conversationId = this.conversationId();
    if (conversationId === null) return;
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.messaging.unmute(conversationId, participantId);
      this.conversation.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not unmute the participant.'));
    } finally {
      this.pending.set(false);
    }
  }

  private async open(tourId: string): Promise<void> {
    this.opening.set(true);
    this.error.set(null);
    this.liveError.set(null);
    await this.connection?.stop();
    this.connection = null;
    this.conversationId.set(null);
    this.messages.set([]);
    this.hasMore.set(false);
    this.processedPage = undefined;
    this.messageRequest.set({ cursor: null, kind: 'latest', nonce: this.messageRequest().nonce + 1 });
    try {
      const conversationId = await this.messaging.open(tourId);
      if (this.openedTourId !== tourId || this.destroyed) return;
      this.conversationId.set(conversationId);
      await this.connect(conversationId);
    } catch (failure) {
      if (this.openedTourId === tourId) {
        this.error.set(serverMessage(failure, 'You do not have access to this tour conversation.'));
      }
    } finally {
      if (this.openedTourId === tourId) this.opening.set(false);
    }
  }

  private applyPage(page: MessagePageDto, request: MessageRequest): void {
    const merged = new Map(this.messages().map((message) => [message.id, message]));
    page.items.forEach((message) => merged.set(message.id, message));
    this.messages.set(
      [...merged.values()].sort(
        (left, right) => left.createdAt.localeCompare(right.createdAt) || left.id.localeCompare(right.id),
      ),
    );
    if (request.kind === 'catchup' && page.hasMore && page.nextCursor !== null) {
      this.messageRequest.set({ cursor: page.nextCursor, kind: 'catchup', nonce: request.nonce + 1 });
      return;
    }
    if (request.kind !== 'catchup') this.hasMore.set(page.hasMore);
  }

  private catchUp(): void {
    const newest = this.messages().at(-1);
    if (newest === undefined) {
      this.reloadLatest(false);
      return;
    }
    this.messageRequest.set({
      cursor: { createdAt: newest.createdAt, id: newest.id },
      kind: 'catchup',
      nonce: this.messageRequest().nonce + 1,
    });
  }

  private reloadLatest(clear: boolean): void {
    if (clear) this.messages.set([]);
    this.messageRequest.set({ cursor: null, kind: 'latest', nonce: this.messageRequest().nonce + 1 });
    this.conversation.reload();
  }

  private async connect(conversationId: string): Promise<void> {
    await this.connection?.stop();
    const connection = new HubConnectionBuilder()
      .withUrl('/hubs/exploration/conversations', {
        accessTokenFactory: () => this.auth.accessToken() ?? '',
      })
      .withAutomaticReconnect()
      .build();
    this.connection = connection;
    connection.on('ConversationChanged', (event: ConversationChangedEvent) => {
      if (event.conversationId !== conversationId) return;
      if (event.changeType === 'MessageDeleted') this.reloadLatest(true);
      else if (event.changeType === 'MessageSent') this.catchUp();
      else this.conversation.reload();
    });
    connection.onreconnected(async () => {
      try {
        await connection.invoke('JoinConversation', conversationId);
        this.liveError.set(null);
        this.catchUp();
        this.conversation.reload();
      } catch {
        this.liveError.set('Could not restore live conversation updates.');
      }
    });
    connection.onclose(() => {
      if (!this.destroyed) this.liveError.set('Live updates are temporarily unavailable.');
    });
    try {
      await connection.start();
      await connection.invoke('JoinConversation', conversationId);
      this.liveError.set(null);
    } catch {
      if (!this.destroyed) this.liveError.set('Could not connect to live conversation updates.');
    }
  }
}
