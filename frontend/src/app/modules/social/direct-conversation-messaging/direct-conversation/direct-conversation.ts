import { httpResource } from '@angular/common/http';
import { Component, DestroyRef, computed, effect, inject, input, signal, untracked } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Auth } from '../../../../core/auth/auth';
import { ConversationChangedEvent } from '../../../../shared/api/conversation-changed-event';
import {
  ConversationChat,
  ConversationChatMessage,
} from '../../../../shared/conversation-chat/conversation-chat/conversation-chat';
import { serverMessage } from '../../../../shared/util/server-message';
import {
  ChatMessageDto,
  ConversationDto,
  MessageCursorDto,
  MessagePageDto,
} from '../../api/social-api-types';
import { DirectConversationMessaging } from '../direct-conversation-messaging';

type MessageRequestKind = 'latest' | 'older' | 'catchup';

interface MessageRequest {
  cursor: MessageCursorDto | null;
  kind: MessageRequestKind;
  nonce: number;
}

@Component({
  selector: 'app-direct-conversation',
  imports: [ConversationChat],
  templateUrl: './direct-conversation.html',
  styleUrl: './direct-conversation.scss',
})
export class DirectConversation {
  readonly conversationId = input.required<string>();

  private readonly auth = inject(Auth);
  private readonly messaging = inject(DirectConversationMessaging);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly conversation = httpResource<ConversationDto>(
    () => `/api/social/conversations/${this.conversationId()}`,
  );

  private readonly messageRequest = signal<MessageRequest>({ cursor: null, kind: 'latest', nonce: 0 });
  protected readonly messagePage = httpResource<MessagePageDto>(() => {
    const request = this.messageRequest();
    const parameters = new URLSearchParams({ pageSize: '20', request: request.nonce.toString() });
    if (request.cursor !== null) {
      parameters.set('cursorCreatedAt', request.cursor.createdAt);
      parameters.set('cursorId', request.cursor.id);
      parameters.set('afterCursor', (request.kind === 'catchup').toString());
    }
    return `/api/social/conversations/${this.conversationId()}/messages?${parameters.toString()}`;
  });

  private readonly messages = signal<ChatMessageDto[]>([]);
  protected readonly hasMore = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  private readonly liveError = signal<string | null>(null);
  protected readonly composerResetKey = signal(0);

  private processedPage: MessagePageDto | undefined;
  private connection: HubConnection | null = null;
  private destroyed = false;

  protected readonly currentUserId = computed(() => this.auth.user()?.id ?? '');
  private readonly participantLabels = computed(
    () => new Map(
      (this.conversation.value()?.participants ?? []).map(
        (participant) => [participant.userId, participant.userEmail ?? 'User'],
      ),
    ),
  );
  protected readonly conversationTitle = computed(() => {
    const otherParticipant = this.conversation.value()?.participants.find(
      (participant) => participant.userId !== this.currentUserId(),
    );
    return `Chat with ${otherParticipant?.userEmail ?? 'user'}`;
  });
  protected readonly chatMessages = computed<ConversationChatMessage[]>(() =>
    this.messages().map((message) => ({
      id: message.id,
      authorId: message.authorId,
      authorLabel:
        message.authorId === this.currentUserId()
          ? 'You'
          : (this.participantLabels().get(message.authorId) ?? 'User'),
      content: message.content,
      createdAt: message.createdAt,
      isDeleted: message.isDeleted,
      canDelete: !message.isDeleted && message.authorId === this.currentUserId(),
      moderationTarget: null,
    })),
  );
  protected readonly displayError = computed(() => {
    if (this.error() !== null) return this.error();
    if (this.liveError() !== null) return this.liveError();
    if (this.conversation.error()) return serverMessage(this.conversation.error(), 'Could not load the conversation.');
    if (this.messagePage.error()) return serverMessage(this.messagePage.error(), 'Could not load messages.');
    return null;
  });

  constructor() {
    effect(() => {
      const page = this.messagePage.value();
      if (page === undefined || page === this.processedPage) return;
      this.processedPage = page;
      const request = this.messageRequest();
      untracked(() => this.applyPage(page, request));
    });

    effect(() => {
      const conversationId = this.conversationId();
      untracked(() => {
        this.messages.set([]);
        this.hasMore.set(false);
        this.processedPage = undefined;
        this.messageRequest.set({ cursor: null, kind: 'latest', nonce: this.messageRequest().nonce + 1 });
        void this.connect(conversationId);
      });
    });

    this.destroyRef.onDestroy(() => {
      this.destroyed = true;
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
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.messaging.send(this.conversationId(), { content });
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

  private applyPage(page: MessagePageDto, request: MessageRequest): void {
    const merged = new Map(this.messages().map((message) => [message.id, message]));
    page.items.forEach((message) => merged.set(message.id, message));
    this.messages.set(
      [...merged.values()].sort(
        (left, right) => left.createdAt.localeCompare(right.createdAt) || left.id.localeCompare(right.id),
      ),
    );

    if (request.kind === 'catchup' && page.hasMore && page.nextCursor !== null) {
      this.messageRequest.set({
        cursor: page.nextCursor,
        kind: 'catchup',
        nonce: request.nonce + 1,
      });
      return;
    }
    if (request.kind !== 'catchup') {
      this.hasMore.set(page.hasMore);
    }
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
      .withUrl('/hubs/social/conversations', {
        accessTokenFactory: () => this.auth.accessToken() ?? '',
      })
      .withAutomaticReconnect()
      .build();
    this.connection = connection;

    connection.on('ConversationChanged', (event: ConversationChangedEvent) => {
      if (event.conversationId !== conversationId) return;
      if (event.changeType === 'MessageDeleted') this.reloadLatest(true);
      else this.catchUp();
    });
    connection.onreconnected(async () => {
      try {
        await connection.invoke('JoinConversation', conversationId);
        this.liveError.set(null);
        this.catchUp();
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
