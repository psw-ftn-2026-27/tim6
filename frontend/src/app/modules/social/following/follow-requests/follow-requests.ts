import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { serverMessage } from '../../../../shared/util/server-message';
import { ReceivedFollowRequestDto, SentFollowRequestDto } from '../../api/social-api-types';
import { FollowTabs } from '../follow-tabs/follow-tabs';
import { Following } from '../following';

interface FollowRequestRow {
  id: string;
  userId: string;
  createdAt: string;
  direction: 'sent' | 'received';
}

@Component({
  imports: [FollowTabs],
  selector: 'app-follow-requests',
  styleUrl: './follow-requests.scss',
  templateUrl: './follow-requests.html',
})
export class FollowRequests {
  private readonly following = inject(Following);

  protected readonly sent = httpResource<SentFollowRequestDto[]>(
    () => '/api/social/follow-requests/sent',
  );
  protected readonly received = httpResource<ReceivedFollowRequestDto[]>(
    () => '/api/social/follow-requests/received',
  );

  protected readonly rows = computed<FollowRequestRow[]>(() => {
    const sentRows = (this.sent.value() ?? [])
      .filter((request) => request.status === 'Pending')
      .map((request) => ({
        id: request.id,
        userId: request.recipientId,
        createdAt: request.createdAt,
        direction: 'sent' as const,
      }));
    const receivedRows = (this.received.value() ?? []).map((request) => ({
      id: request.id,
      userId: request.senderId,
      createdAt: request.createdAt,
      direction: 'received' as const,
    }));
    return [...sentRows, ...receivedRows].sort((a, b) => b.createdAt.localeCompare(a.createdAt));
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async cancel(requestId: string): Promise<void> {
    await this.run(() => this.following.cancelRequest(requestId), 'Could not cancel the follow request.');
  }

  protected async accept(requestId: string): Promise<void> {
    await this.run(() => this.following.acceptRequest(requestId), 'Could not accept the follow request.');
  }

  protected async reject(requestId: string): Promise<void> {
    await this.run(() => this.following.rejectRequest(requestId), 'Could not reject the follow request.');
  }

  private async run(action: () => Promise<void>, errorMessage: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await action();
      this.sent.reload();
      this.received.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, errorMessage));
    } finally {
      this.pending.set(false);
    }
  }
}
