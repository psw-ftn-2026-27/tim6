import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubDto, ClubMembershipDto } from '../../api/social-api-types';
import { ClubMembership } from '../club-membership/club-membership';
import { ClubReading } from '../club-reading';

@Component({
  imports: [ClubMembership, RouterLink],
  selector: 'app-club-detail',
  styleUrl: './club-detail.scss',
  templateUrl: './club-detail.html',
})
export class ClubDetail {
  private readonly clubReading = inject(ClubReading);

  readonly id = input.required<string>();

  protected readonly detail = httpResource<ClubDto>(() => `/api/social/clubs/${this.id()}`);

  protected readonly membership = httpResource<ClubMembershipDto>(
    () => `/api/social/clubs/${this.id()}/membership`,
  );

  protected readonly membershipPending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async requestToJoin(): Promise<void> {
    this.membershipPending.set(true);
    this.error.set(null);
    try {
      await this.clubReading.requestToJoin(this.id());
      this.membership.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not send the join request.'));
    } finally {
      this.membershipPending.set(false);
    }
  }

  protected async withdrawJoinRequest(): Promise<void> {
    this.membershipPending.set(true);
    this.error.set(null);
    try {
      await this.clubReading.withdrawJoinRequest(this.id());
      this.membership.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not withdraw the join request.'));
    } finally {
      this.membershipPending.set(false);
    }
  }
}
