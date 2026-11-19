import { Component, input, output } from '@angular/core';
import { LocationChallengeDto } from '../../api/games-api-types';

// An action on the card that asks for confirmation first. "republish" and "publish" call
// the same server command; they differ only in the button and the question.
export type LocationChallengeAction = 'publish' | 'republish' | 'retire' | 'archive' | 'delete';

@Component({
  selector: 'app-location-challenge-card',
  styleUrl: './location-challenge-card.scss',
  templateUrl: './location-challenge-card.html',
})
export class LocationChallengeCard {
  readonly challenge = input.required<LocationChallengeDto>();
  readonly confirming = input<LocationChallengeAction | null>(null);
  readonly pending = input(false);

  readonly edit = output<void>();
  readonly startAction = output<LocationChallengeAction>();
  readonly confirmAction = output<LocationChallengeAction>();
  readonly cancelAction = output<void>();

  protected confirm(): void {
    const action = this.confirming();
    if (action !== null) {
      this.confirmAction.emit(action);
    }
  }
}
