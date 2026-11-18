import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubMemberDto, ClubMembershipDto, ClubRole } from '../../api/social-api-types';
import { ClubModeration } from '../club-moderation';

@Component({
  imports: [RouterLink],
  selector: 'app-club-members',
  styleUrl: './club-members.scss',
  templateUrl: './club-members.html',
})
export class ClubMembers {
  private readonly clubModeration = inject(ClubModeration);

  readonly id = input.required<string>();

  protected readonly members = httpResource<ClubMemberDto[]>(
    () => `/api/social/clubs/${this.id()}/members`,
  );

  protected readonly membership = httpResource<ClubMembershipDto>(
    () => `/api/social/clubs/${this.id()}/membership`,
  );

  protected readonly loadError = computed(() =>
    serverMessage(this.members.error(), 'Could not load the club members.'),
  );

  // The signed-in user's role in this club decides which buttons a row shows.
  private readonly myRole = computed<ClubRole | null>(() =>
    this.membership.hasValue() ? this.membership.value().role : null,
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected canPromote(member: ClubMemberDto): boolean {
    return this.myRole() === 'Owner' && member.role === 'Member';
  }

  protected canRemove(member: ClubMemberDto): boolean {
    if (this.myRole() === 'Owner') {
      return member.role !== 'Owner';
    }
    if (this.myRole() === 'Moderator') {
      return member.role === 'Member';
    }
    return false;
  }

  protected async promote(memberId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubModeration.promoteToModerator(this.id(), memberId);
      this.members.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not promote the member.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async remove(memberId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubModeration.removeMember(this.id(), memberId);
      this.members.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not remove the member.'));
    } finally {
      this.pending.set(false);
    }
  }
}
