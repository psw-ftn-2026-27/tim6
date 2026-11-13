import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, maxLength, min, required } from '@angular/forms/signals';
import { Auth } from '../../../../core/auth/auth';
import {
  GeographicMap,
  MapCoordinate,
  MapPin,
} from '../../../../shared/geographic-map/geographic-map';
import { serverMessage } from '../../../../shared/util/server-message';
import { CreateQuizDto, QuizDto } from '../../api/games-api-types';
import { QuizAction, QuizCard } from '../quiz-card/quiz-card';
import { QuizAuthoring } from '../quiz-authoring';

// The limits only disable the add and remove buttons; the server checks the rules.
const MAX_QUESTIONS = 20;
const MIN_QUESTIONS = 1;
const MAX_OPTIONS = 6;
const MIN_OPTIONS = 2;

@Component({
  imports: [FormField, GeographicMap, QuizCard],
  selector: 'app-quiz-list',
  styleUrl: './quiz-list.scss',
  templateUrl: './quiz-list.html',
})
export class QuizList {
  private readonly quizAuthoring = inject(QuizAuthoring);
  private readonly auth = inject(Auth);

  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );

  protected readonly quizzes = httpResource<QuizDto[]>(() => '/api/games/quizzes');

  // The form section is open for a new quiz (editingQuizId is null) or for a draft.
  protected readonly formOpen = signal(false);
  protected readonly editingQuizId = signal<string | null>(null);
  protected readonly confirmingQuizId = signal<string | null>(null);
  protected readonly confirmingAction = signal<QuizAction | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly formError = signal<string | null>(null);
  // The map shows the saved location as a pin; a click places the chosen one beside it.
  protected readonly savedPins = signal<MapPin[]>([]);

  // The rules on questions and answer options (P4 to P8) are checked by the server;
  // its message is shown above the form. The location is chosen on the map, not typed.
  protected readonly quizForm = form(
    signal<CreateQuizDto>({ name: '', instructions: '', xp: 1, questions: [], location: null }),
    (path) => {
      required(path.name, { message: 'Naziv je obavezan.' });
      maxLength(path.name, 200, { message: 'Naziv može imati najviše 200 znakova.' });
      required(path.instructions, { message: 'Uputstva su obavezna.' });
      required(path.xp, { message: 'XP je obavezan.' });
      min(path.xp, 1, { message: 'XP mora biti veći od 0.' });
    },
  );

  protected readonly canAddQuestion = computed(
    () => this.quizForm.questions().value().length < MAX_QUESTIONS,
  );
  protected readonly canRemoveQuestion = computed(
    () => this.quizForm.questions().value().length > MIN_QUESTIONS,
  );

  protected canAddOption(questionIndex: number): boolean {
    return this.quizForm.questions().value()[questionIndex].options.length < MAX_OPTIONS;
  }

  protected canRemoveOption(questionIndex: number): boolean {
    return this.quizForm.questions().value()[questionIndex].options.length > MIN_OPTIONS;
  }

  protected startCreate(): void {
    this.cancelAction();
    this.formError.set(null);
    this.editingQuizId.set(null);
    this.savedPins.set([]);
    this.quizForm().reset({ name: '', instructions: '', xp: 1, questions: [], location: null });
    this.formOpen.set(true);
  }

  // The list returns whole quizzes, so the form is filled from the clicked card's data.
  protected startEdit(quiz: QuizDto): void {
    this.cancelAction();
    this.formError.set(null);
    this.editingQuizId.set(quiz.id);
    this.savedPins.set(quiz.location === null ? [] : [quiz.location]);
    this.quizForm().reset({
      name: quiz.name,
      instructions: quiz.instructions,
      xp: quiz.xp,
      questions: quiz.questions.map((question) => ({
        text: question.text,
        options: question.options.map((option) => ({
          text: option.text,
          isCorrect: option.isCorrect,
        })),
      })),
      location: quiz.location,
    });
    this.formOpen.set(true);
  }

  protected cancelForm(): void {
    this.formOpen.set(false);
    this.editingQuizId.set(null);
    this.formError.set(null);
  }

  // A new question starts with two empty answer options, the first one marked correct,
  // so every question always has exactly one correct option.
  protected addQuestion(): void {
    this.quizForm.questions().value.update((questions) => [
      ...questions,
      {
        text: '',
        options: [
          { text: '', isCorrect: true },
          { text: '', isCorrect: false },
        ],
      },
    ]);
  }

  protected removeQuestion(questionIndex: number): void {
    this.quizForm
      .questions()
      .value.update((questions) => questions.filter((_, index) => index !== questionIndex));
  }

  protected addOption(questionIndex: number): void {
    this.quizForm.questions[questionIndex]
      .options()
      .value.update((options) => [...options, { text: '', isCorrect: false }]);
  }

  // Removing the correct option marks the first remaining one, so one stays marked.
  protected removeOption(questionIndex: number, optionIndex: number): void {
    this.quizForm.questions[questionIndex].options().value.update((options) => {
      const remaining = options.filter((_, index) => index !== optionIndex);
      if (remaining.some((option) => option.isCorrect)) {
        return remaining;
      }
      return remaining.map((option, index) => ({ text: option.text, isCorrect: index === 0 }));
    });
  }

  // A radio button is not typed text, so the click sets isCorrect on the clicked option
  // and clears it on the other options of the same question.
  protected markCorrect(questionIndex: number, optionIndex: number): void {
    this.quizForm.questions[questionIndex]
      .options()
      .value.update((options) =>
        options.map((option, index) => ({ text: option.text, isCorrect: index === optionIndex })),
      );
  }

  // A map click is not typed text, so the page stores the chosen location in the form.
  protected selectLocation(coordinate: MapCoordinate): void {
    this.quizForm
      .location()
      .value.set({ latitude: coordinate.latitude, longitude: coordinate.longitude });
  }

  // The draft is saved with an empty location, which the server removes.
  protected removeLocation(): void {
    this.quizForm.location().value.set(null);
    this.savedPins.set([]);
  }

  protected async save(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.formError.set(null);
    try {
      const quizId = this.editingQuizId();
      if (quizId === null) {
        await this.quizAuthoring.create(this.quizForm().value());
      } else {
        await this.quizAuthoring.update(quizId, this.quizForm().value());
      }
      this.cancelForm();
      this.quizzes.reload();
    } catch (failure) {
      this.formError.set(serverMessage(failure, 'Kviz nije sačuvan.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected startAction(quizId: string, action: QuizAction): void {
    this.cancelForm();
    this.confirmingQuizId.set(quizId);
    this.confirmingAction.set(action);
  }

  protected cancelAction(): void {
    this.confirmingQuizId.set(null);
    this.confirmingAction.set(null);
  }

  protected async runAction(quiz: QuizDto, action: QuizAction): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      switch (action) {
        case 'publish':
        case 'republish':
          await this.quizAuthoring.publish(quiz.id);
          break;
        case 'retire':
          await this.quizAuthoring.retire(quiz.id);
          break;
        case 'archive':
          await this.quizAuthoring.archive(quiz.id);
          break;
        case 'delete':
          await this.quizAuthoring.delete(quiz.id);
          break;
      }
      this.cancelAction();
      this.quizzes.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Radnja nije izvršena.'));
    } finally {
      this.pending.set(false);
    }
  }
}
