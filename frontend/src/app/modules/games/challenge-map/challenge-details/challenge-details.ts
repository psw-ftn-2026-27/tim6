import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ChallengeOnMapDto } from '../../api/games-api-types';

// The challenge selected on the map, shown below it. A quiz is activated by opening its
// solving page; a social challenge cannot be activated here yet.
@Component({
  imports: [RouterLink],
  selector: 'app-challenge-details',
  styleUrl: './challenge-details.scss',
  templateUrl: './challenge-details.html',
})
export class ChallengeDetails {
  readonly challenge = input.required<ChallengeOnMapDto>();
}
