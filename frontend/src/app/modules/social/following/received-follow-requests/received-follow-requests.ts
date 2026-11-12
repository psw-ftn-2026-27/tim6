import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ReceivedFollowRequestDto } from '../../api/social-api-types';
import { Following } from '../following';

@Component({
  imports: [RouterLink],
  selector: 'app-received-follow-requests',
  styleUrl: './received-follow-requests.scss',
  templateUrl: './received-follow-requests.html',
})
export class ReceivedFollowRequests {
  private readonly following = inject(Following);

  protected readonly requests = httpResource<ReceivedFollowRequestDto[]>(
    () => '/api/social/follow-requests/received',
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async accept(requestId: string): Promise<void> {
    await this.run(() => this.following.acceptRequest(requestId), 'Could not accept the follow request.');
  }

  protected async reject(requestId: string): Promise<void> {
    await this.run(() => this.following.rejectRequest(requestId), 'Could not reject the follow request.');
  }

  protected async acceptAndFollowBack(requestId: string): Promise<void> {
    await this.run(
      () => this.following.acceptAndFollowBack(requestId),
      'Could not accept the request and follow back.',
    );
  }

  private async run(action: () => Promise<void>, errorMessage: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await action();
      this.requests.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, errorMessage));
    } finally {
      this.pending.set(false);
    }
  }
}
