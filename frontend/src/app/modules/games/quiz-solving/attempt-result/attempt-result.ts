import { Component, input, output } from '@angular/core';
import { QuizAttemptResultDto } from '../../api/games-api-types';

// The result of a finished attempt. A quiz that is not solved offers a new attempt.
@Component({
  selector: 'app-attempt-result',
  styleUrl: './attempt-result.scss',
  templateUrl: './attempt-result.html',
})
export class AttemptResult {
  readonly result = input.required<QuizAttemptResultDto>();

  readonly tryAgain = output<void>();
}
