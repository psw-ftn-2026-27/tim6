import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { LocationDto } from '../api/games-api-types';

const BASE_URL = '/api/games/social-challenges';

@Injectable({ providedIn: 'root' })
export class SocialChallengeSolving {
  private readonly http = inject(HttpClient);

  // Joins the explorer to the active run again after leaving it.
  async activate(id: string, position: LocationDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/activate`, position));
  }

  // Reports where the explorer is; the page reads the run state again afterwards.
  async checkPosition(id: string, position: LocationDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/check-position`, position));
  }
}
