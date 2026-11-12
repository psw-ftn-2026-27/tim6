import { Component, input, output } from '@angular/core';
import { SocialChallengeDto } from '../../api/games-api-types';

@Component({
  selector: 'app-challenge-card',
  styleUrl: './challenge-card.scss',
  templateUrl: './challenge-card.html',
})
export class ChallengeCard {
  readonly challenge = input.required<SocialChallengeDto>();
  readonly confirming = input(false);
  readonly pending = input(false);

  readonly edit = output<void>();
  readonly startDelete = output<void>();
  readonly confirmDelete = output<void>();
  readonly cancelDelete = output<void>();

  // The server sends "HH:mm:ss"; the card shows hours and minutes only.
  protected shortTime(time: string): string {
    return time.slice(0, 5);
  }
}
