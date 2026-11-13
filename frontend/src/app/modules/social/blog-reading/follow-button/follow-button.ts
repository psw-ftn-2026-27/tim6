import { Component, input, output } from '@angular/core';
import { FollowStatus } from '../../api/social-api-types';

@Component({
  selector: 'app-follow-button',
  styleUrl: './follow-button.scss',
  templateUrl: './follow-button.html',
})
export class FollowButton {
  readonly status = input.required<FollowStatus>();
  readonly pending = input(false);

  readonly follow = output<void>();
}
