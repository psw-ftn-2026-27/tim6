import { DatePipe } from '@angular/common';
import { Component, effect, input, output, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';

export interface ConversationChatMessage {
  id: string;
  authorId: string;
  authorLabel: string;
  content: string | null;
  createdAt: string;
  isDeleted: boolean;
  canDelete: boolean;
  moderationTarget: ConversationChatParticipant | null;
}

export interface ConversationChatParticipant {
  userId: string;
  label: string;
  isPrivileged: boolean;
  mutedUntil: string | null;
  isPermanentlyMuted: boolean;
  canModerate: boolean;
}

export type ConversationMuteDuration = 'FifteenMinutes' | 'OneHour' | 'TwentyFourHours' | 'Permanent';

export interface MuteParticipantRequest {
  participantId: string;
  duration: ConversationMuteDuration;
}

@Component({
  selector: 'app-conversation-chat',
  imports: [DatePipe, FormField],
  templateUrl: './conversation-chat.html',
  styleUrl: './conversation-chat.scss',
})
export class ConversationChat {
  readonly messages = input.required<ConversationChatMessage[]>();
  readonly currentUserId = input.required<string>();
  readonly isMuted = input(false);
  readonly muteDescription = input<string | null>(null);
  readonly hasMore = input(false);
  readonly loading = input(false);
  readonly pending = input(false);
  readonly error = input<string | null>(null);
  readonly composerResetKey = input(0);

  readonly sendMessage = output<string>();
  readonly deleteMessage = output<string>();
  readonly muteParticipant = output<MuteParticipantRequest>();
  readonly unmuteParticipant = output<string>();
  readonly loadOlderMessages = output<void>();

  private readonly model = signal({ content: '' });
  protected readonly confirmingMessageId = signal<string | null>(null);
  protected readonly messageForm = form(this.model, (path) => {
    required(path.content, { message: 'Message cannot be empty.' });
  });

  private lastResetKey = 0;

  constructor() {
    effect(() => {
      const resetKey = this.composerResetKey();
      if (resetKey !== this.lastResetKey) {
        this.lastResetKey = resetKey;
        this.messageForm().reset({ content: '' });
      }
    });
  }

  protected submit(event: Event): void {
    event.preventDefault();
    if (!this.messageForm().valid() || this.pending() || this.isMuted()) {
      return;
    }
    this.sendMessage.emit(this.messageForm.content().value());
  }

  protected requestDelete(messageId: string): void {
    this.confirmingMessageId.set(messageId);
  }

  protected confirmDelete(): void {
    const messageId = this.confirmingMessageId();
    if (messageId === null) return;
    this.confirmingMessageId.set(null);
    this.deleteMessage.emit(messageId);
  }
}
