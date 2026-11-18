import { Component, input, output } from '@angular/core';
import { ClubMembershipStatus } from '../../api/social-api-types';

@Component({
  selector: 'app-club-membership',
  styleUrl: './club-membership.scss',
  templateUrl: './club-membership.html',
})
export class ClubMembership {
  readonly status = input.required<ClubMembershipStatus>();
  readonly pending = input(false);

  readonly requestToJoin = output<void>();
  readonly withdraw = output<void>();
}
