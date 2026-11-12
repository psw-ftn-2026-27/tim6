import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

export type FollowTab = 'requests' | 'followers' | 'following';

@Component({
  imports: [RouterLink],
  selector: 'app-follow-tabs',
  styleUrl: './follow-tabs.scss',
  templateUrl: './follow-tabs.html',
})
export class FollowTabs {
  readonly active = input.required<FollowTab>();
}
