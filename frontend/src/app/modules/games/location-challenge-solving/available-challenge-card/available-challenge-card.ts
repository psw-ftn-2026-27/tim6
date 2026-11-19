import { Component, input, output } from '@angular/core';
import { AvailableLocationChallengeDto } from '../../api/games-api-types';

@Component({
  selector: 'app-available-challenge-card',
  styleUrl: './available-challenge-card.scss',
  templateUrl: './available-challenge-card.html',
})
export class AvailableChallengeCard {
  readonly challenge = input.required<AvailableLocationChallengeDto>();
  // The explorer has a saved position, so the page can send it to the server.
  readonly canActivate = input(false);
  readonly pending = input(false);
  // The server's message when it rejected the activation of this challenge.
  readonly error = input<string | null>(null);

  readonly activate = output<void>();
}
