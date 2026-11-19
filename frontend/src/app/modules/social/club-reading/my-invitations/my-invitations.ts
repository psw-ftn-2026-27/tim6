import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ReceivedClubInvitationDto } from '../../api/social-api-types';
import { ClubReading } from '../club-reading';

@Component({
  imports: [RouterLink],
  selector: 'app-my-invitations',
  styleUrl: './my-invitations.scss',
  templateUrl: './my-invitations.html',
})
export class MyInvitations {
  private readonly clubReading = inject(ClubReading);

  protected readonly invitations = httpResource<ReceivedClubInvitationDto[]>(
    () => '/api/social/clubs/invitations',
  );

  protected readonly loadError = computed(() =>
    serverMessage(this.invitations.error(), 'Could not load your invitations.'),
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

  protected async accept(clubId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubReading.acceptInvitation(clubId);
      this.invitations.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not accept the invitation.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async reject(clubId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubReading.rejectInvitation(clubId);
      this.invitations.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not reject the invitation.'));
    } finally {
      this.pending.set(false);
    }
  }
}
