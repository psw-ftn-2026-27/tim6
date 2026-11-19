import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CoordinatesDto } from '../../../../shared/api/coordinates-dto';
import { serverMessage } from '../../../../shared/util/server-message';
import {
  QuizAttemptResultDto,
  QuizForSolvingDto,
  SelectedAnswerDto,
} from '../../api/games-api-types';
import { AttemptResult } from '../attempt-result/attempt-result';
import { QuizQuestion } from '../quiz-question/quiz-question';
import { QuizSolving } from '../quiz-solving';

@Component({
  imports: [AttemptResult, QuizQuestion, RouterLink],
  selector: 'app-solve-quiz',
  styleUrl: './solve-quiz.scss',
  templateUrl: './solve-quiz.html',
})
export class SolveQuiz {
  private readonly quizSolving = inject(QuizSolving);

  // The quiz id from the route /games/quizzes/:id/solve.
  readonly id = input.required<string>();

  // The page only reads the position; the explorer sets it on the challenge list's map.
  protected readonly position = httpResource<CoordinatesDto | null>(
    () => '/api/shared/positions/mine',
  );
  protected readonly explorerPosition = computed(() =>
    this.position.hasValue() ? this.position.value() : null,
  );
  protected readonly positionError = computed(() =>
    this.position.error()
      ? serverMessage(this.position.error(), 'Your position could not be loaded.')
      : null,
  );

  // Opening the page is the activation: the server refuses a quiz that is too far,
  // already solved or not published.
  protected readonly quiz = httpResource<QuizForSolvingDto>(() => {
    const position = this.explorerPosition();
    return position === null
      ? undefined
      : `/api/games/quizzes/${this.id()}/solving?latitude=${position.latitude}&longitude=${position.longitude}`;
  });
  protected readonly quizError = computed(() =>
    this.quiz.error() ? serverMessage(this.quiz.error(), 'The quiz could not be loaded.') : null,
  );

  // The answers stay in the page until the last question; leaving the page drops the attempt.
  protected readonly answers = signal<SelectedAnswerDto[]>([]);
  protected readonly questionIndex = computed(() => this.answers().length);
  protected readonly question = computed(() =>
    this.quiz.hasValue() ? (this.quiz.value().questions[this.questionIndex()] ?? null) : null,
  );

  protected readonly submitting = signal(false);
  protected readonly submitError = signal<string | null>(null);

  // After a successful submit the page reads the result by the attempt id.
  protected readonly attemptId = signal<string | null>(null);
  protected readonly result = httpResource<QuizAttemptResultDto>(() => {
    const attemptId = this.attemptId();
    return attemptId === null ? undefined : `/api/games/quizzes/attempts/${attemptId}`;
  });
  protected readonly resultError = computed(() =>
    this.result.error()
      ? serverMessage(this.result.error(), 'The result could not be loaded.')
      : null,
  );

  protected async next(optionIndex: number): Promise<void> {
    const quiz = this.quiz.hasValue() ? this.quiz.value() : null;
    if (quiz === null || this.submitting()) {
      return;
    }

    const answers = [...this.answers(), { questionIndex: this.questionIndex(), optionIndex }];

    if (answers.length < quiz.questions.length) {
      this.answers.set(answers);
      return;
    }

    await this.submit(quiz.id, answers);
  }

  // The last answer sends one command with all answers and the current position.
  private async submit(quizId: string, answers: SelectedAnswerDto[]): Promise<void> {
    const position = this.explorerPosition();
    if (position === null) {
      return;
    }

    this.submitting.set(true);
    this.submitError.set(null);
    try {
      const attemptId = await this.quizSolving.submitAttempt(quizId, {
        position: { latitude: position.latitude, longitude: position.longitude },
        answers,
      });
      this.answers.set(answers);
      this.attemptId.set(attemptId);
    } catch (failure) {
      this.submitError.set(serverMessage(failure, 'The answers could not be sent.'));
    } finally {
      this.submitting.set(false);
    }
  }

  // A new attempt reloads the quiz and starts from the first question.
  protected tryAgain(): void {
    this.attemptId.set(null);
    this.answers.set([]);
    this.quiz.reload();
  }
}
