import { Component, computed, input, output } from '@angular/core';
import { GeographicMap, MapPin } from '../../../../shared/geographic-map/geographic-map';
import { SocialChallengeDto } from '../../api/games-api-types';

// An action on the card that asks for confirmation first. "republish" and "publish" call
// the same server command; they differ only in the button and the question.
export type ChallengeAction = 'publish' | 'republish' | 'retire' | 'archive' | 'delete';

@Component({
  imports: [GeographicMap],
  selector: 'app-challenge-card',
  styleUrl: './challenge-card.scss',
  templateUrl: './challenge-card.html',
})
export class ChallengeCard {
  readonly challenge = input.required<SocialChallengeDto>();
  readonly confirming = input<ChallengeAction | null>(null);
  readonly pending = input(false);

  readonly edit = output<void>();
  readonly startAction = output<ChallengeAction>();
  readonly confirmAction = output<ChallengeAction>();
  readonly cancelAction = output<void>();

  protected readonly pins = computed<MapPin[]>(() => {
    const location = this.challenge().location;
    return location === null ? [] : [location];
  });

  protected confirm(): void {
    const action = this.confirming();
    if (action !== null) {
      this.confirmAction.emit(action);
    }
  }

  // The server sends "HH:mm:ss"; the card shows hours and minutes only.
  protected shortTime(time: string): string {
    return time.slice(0, 5);
  }
}
