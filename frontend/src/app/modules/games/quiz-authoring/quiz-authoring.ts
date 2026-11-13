import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CreateQuizDto, UpdateQuizDto } from '../api/games-api-types';

const BASE_URL = '/api/games/quizzes';

@Injectable({ providedIn: 'root' })
export class QuizAuthoring {
  private readonly http = inject(HttpClient);

  // Returns the id of the new draft.
  async create(dto: CreateQuizDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(BASE_URL, dto));
  }

  async update(id: string, dto: UpdateQuizDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}`, dto));
  }

  async delete(id: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}`));
  }

  // Publishes a draft or republishes a retired quiz; the server decides which.
  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, null));
  }

  async retire(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/retire`, null));
  }

  async archive(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/archive`, null));
  }
}
