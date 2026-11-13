import { Component, input, output } from '@angular/core';
import { QuizDto } from '../../api/games-api-types';

@Component({
  selector: 'app-quiz-card',
  styleUrl: './quiz-card.scss',
  templateUrl: './quiz-card.html',
})
export class QuizCard {
  readonly quiz = input.required<QuizDto>();
  readonly confirmingDelete = input(false);
  readonly pending = input(false);

  readonly edit = output<void>();
  readonly startDelete = output<void>();
  readonly confirmDelete = output<void>();
  readonly cancelDelete = output<void>();
}
