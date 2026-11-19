import { Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ChallengeOnMapDto } from '../../api/games-api-types';

// The challenge selected on the map, shown below it. A quiz is activated by opening its
// solving page; a social challenge is activated by the page, which shows a refusal here.
@Component({
  imports: [RouterLink],
  selector: 'app-challenge-details',
  styleUrl: './challenge-details.scss',
  templateUrl: './challenge-details.html',
})
export class ChallengeDetails {
  readonly challenge = input.required<ChallengeOnMapDto>();
  readonly activating = input(false);
  readonly activateError = input<string | null>(null);

  // The id of the social challenge to activate.
  readonly activate = output<string>();
}
