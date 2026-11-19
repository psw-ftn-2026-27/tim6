import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { SubmitAttemptDto } from '../api/games-api-types';

const BASE_URL = '/api/games/quizzes';

@Injectable({ providedIn: 'root' })
export class QuizSolving {
  private readonly http = inject(HttpClient);

  // Sends all answers of a finished attempt at once; returns the id of the saved attempt.
  async submitAttempt(quizId: string, dto: SubmitAttemptDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(`${BASE_URL}/${quizId}/attempts`, dto));
  }
}
