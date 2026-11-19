import { Component, input, output, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { QuestionForSolvingDto } from '../../api/games-api-types';

// One question of an attempt. It reports the chosen option index and clears its choice for
// the next question; the page decides what happens with the answer.
@Component({
  imports: [FormField],
  selector: 'app-quiz-question',
  styleUrl: './quiz-question.scss',
  templateUrl: './quiz-question.html',
})
export class QuizQuestion {
  readonly question = input.required<QuestionForSolvingDto>();
  readonly number = input.required<number>();
  readonly total = input.required<number>();
  readonly pending = input(false);

  readonly answered = output<number>();

  // The chosen option index, as the radio button's value.
  protected readonly form = form(signal({ option: '' }), (path) => {
    required(path.option, { message: 'Choose an answer.' });
  });

  protected submit(event: Event): void {
    event.preventDefault();
    this.answered.emit(Number(this.form.option().value()));
    this.form().reset({ option: '' });
  }
}
