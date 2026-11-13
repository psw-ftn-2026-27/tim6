import { Component, computed, input, output } from '@angular/core';
import { GeographicMap, MapPin } from '../../../../shared/geographic-map/geographic-map';
import { QuizDto } from '../../api/games-api-types';

// An action on the card that asks for confirmation first. "republish" and "publish" call
// the same server command; they differ only in the button and the question.
export type QuizAction = 'publish' | 'republish' | 'retire' | 'archive' | 'delete';

@Component({
  imports: [GeographicMap],
  selector: 'app-quiz-card',
  styleUrl: './quiz-card.scss',
  templateUrl: './quiz-card.html',
})
export class QuizCard {
  readonly quiz = input.required<QuizDto>();
  readonly confirming = input<QuizAction | null>(null);
  readonly pending = input(false);

  readonly edit = output<void>();
  readonly startAction = output<QuizAction>();
  readonly confirmAction = output<QuizAction>();
  readonly cancelAction = output<void>();

  protected readonly pins = computed<MapPin[]>(() => {
    const location = this.quiz().location;
    return location === null ? [] : [location];
  });

  protected confirm(): void {
    const action = this.confirming();
    if (action !== null) {
      this.confirmAction.emit(action);
    }
  }
}
