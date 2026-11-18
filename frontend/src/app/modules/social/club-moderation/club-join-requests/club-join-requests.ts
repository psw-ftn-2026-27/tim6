import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubJoinRequestDto } from '../../api/social-api-types';
import { ClubModeration } from '../club-moderation';

@Component({
  imports: [RouterLink],
  selector: 'app-club-join-requests',
  styleUrl: './club-join-requests.scss',
  templateUrl: './club-join-requests.html',
})
export class ClubJoinRequests {
  private readonly clubModeration = inject(ClubModeration);

  readonly id = input.required<string>();

  protected readonly requests = httpResource<ClubJoinRequestDto[]>(
    () => `/api/social/clubs/${this.id()}/join-requests`,
  );

  protected readonly loadError = computed(() =>
    serverMessage(this.requests.error(), 'Could not load the join requests.'),
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected formatDate(value: string): string {
    return new Date(value).toLocaleString('sr-Latn-RS', {
      day: 'numeric',
      month: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  protected async accept(applicantId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubModeration.acceptJoinRequest(this.id(), applicantId);
      this.requests.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not accept the join request.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async reject(applicantId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubModeration.rejectJoinRequest(this.id(), applicantId);
      this.requests.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not reject the join request.'));
    } finally {
      this.pending.set(false);
    }
  }
}
