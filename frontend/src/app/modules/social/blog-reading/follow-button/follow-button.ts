import { Component, input, output } from '@angular/core';
import { BlogFollowState } from '../../api/social-api-types';

@Component({
  selector: 'app-follow-button',
  styleUrl: './follow-button.scss',
  templateUrl: './follow-button.html',
})
export class FollowButton {
  readonly state = input.required<BlogFollowState>();
  readonly pending = input(false);

  readonly follow = output<void>();
}
