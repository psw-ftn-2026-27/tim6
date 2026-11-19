import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubDto, ClubMemberDto, ClubMembershipDto, ClubRole } from '../../api/social-api-types';
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

  // The club itself is read only to know whether it is closed: a closed club's table has
  // no buttons.
  protected readonly detail = httpResource<ClubDto>(() => `/api/social/clubs/${this.id()}`);

  protected readonly loadError = computed(() =>
    serverMessage(this.members.error(), 'Could not load the club members.'),
  );

  // The signed-in user's role in this club decides which buttons a row shows.
  private readonly myRole = computed<ClubRole | null>(() =>
    this.membership.hasValue() ? this.membership.value().role : null,
  );

  private readonly active = computed(
    () => this.detail.hasValue() && this.detail.value().status === 'Active',
  );

  protected readonly confirmingMemberId = signal<string | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected canPromote(member: ClubMemberDto): boolean {
    return this.active() && this.myRole() === 'Owner' && member.role === 'Member';
  }

  protected canTransferOwnership(member: ClubMemberDto): boolean {
    return this.active() && this.myRole() === 'Owner' && member.role !== 'Owner';
  }

  protected canRemove(member: ClubMemberDto): boolean {
    if (!this.active()) {
      return false;
    }
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

  protected async transferOwnership(memberId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubModeration.transferOwnership(this.id(), memberId);
      this.confirmingMemberId.set(null);
      // The signed-in user is now a moderator, so the buttons change along with the roles.
      this.members.reload();
      this.membership.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not transfer the ownership.'));
    } finally {
      this.pending.set(false);
    }
  }
}
