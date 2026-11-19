import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TourDto } from '../../api/exploration-api-types';

@Component({
  imports: [RouterLink],
  selector: 'app-tour-card',
  styleUrl: './tour-card.scss',
  templateUrl: './tour-card.html',
})
export class TourCard {
  readonly tour = input.required<TourDto>();
  readonly canStart = input(false);
}
