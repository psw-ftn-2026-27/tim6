import { Component, computed, input, output } from '@angular/core';
import { GeographicMap, MapPin } from '../../../../shared/geographic-map/geographic-map';
import { LocationChallengeDto } from '../../api/games-api-types';

// An action on the card that asks for confirmation first. "republish" and "publish" call
// the same server command; they differ only in the button and the question.
export type LocationChallengeAction = 'publish' | 'republish' | 'retire' | 'archive' | 'delete';

@Component({
  imports: [GeographicMap],
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

  // The start location, when it is set, and every hidden point.
  protected readonly pins = computed<MapPin[]>(() => {
    const challenge = this.challenge();
    const hiddenPoints = challenge.hiddenPoints.map((point) => point.location);
    return challenge.location === null ? hiddenPoints : [challenge.location, ...hiddenPoints];
  });

  protected confirm(): void {
    const action = this.confirming();
    if (action !== null) {
      this.confirmAction.emit(action);
    }
  }
}
