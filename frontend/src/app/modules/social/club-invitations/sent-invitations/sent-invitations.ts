import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubInvitationDto } from '../../api/social-api-types';
import { ClubInvitations } from '../club-invitations';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-sent-invitations',
  styleUrl: './sent-invitations.scss',
  templateUrl: './sent-invitations.html',
})
export class SentInvitations {
  private readonly clubInvitations = inject(ClubInvitations);

  readonly id = input.required<string>();

  protected readonly invitations = httpResource<ClubInvitationDto[]>(
    () => `/api/social/clubs/${this.id()}/invitations`,
  );

  protected readonly loadError = computed(() =>
    serverMessage(this.invitations.error(), 'Could not load the invitations.'),
  );

  protected readonly form = form(signal({ email: '' }), (path) => {
    required(path.email, { message: 'Email is required.' });
  });

  protected readonly pending = signal(false);
  // The server's message for a failed invite stands under the form, the one for a failed
  // withdraw above the table.
  protected readonly inviteError = signal<string | null>(null);
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

  protected async invite(event: Event): Promise<void> {
    event.preventDefault();
    if (!this.form().valid()) {
      this.form().markAsTouched();
      return;
    }
    this.pending.set(true);
    this.inviteError.set(null);
    try {
      await this.clubInvitations.invite(this.id(), this.form().value());
      // The page stays on screen, so the form starts over for the next invitation.
      this.form().reset({ email: '' });
      this.invitations.reload();
    } catch (failure) {
      this.inviteError.set(serverMessage(failure, 'Could not send the invitation.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async withdraw(inviteeId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubInvitations.withdrawInvitation(this.id(), inviteeId);
      this.invitations.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not withdraw the invitation.'));
    } finally {
      this.pending.set(false);
    }
  }
}
