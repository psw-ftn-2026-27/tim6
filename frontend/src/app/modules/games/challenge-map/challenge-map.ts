import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { LocationDto } from '../api/games-api-types';

const BASE_URL = '/api/games/social-challenges';

// The commands of the challenge map page.
@Injectable({ providedIn: 'root' })
export class ChallengeMapCommands {
  private readonly http = inject(HttpClient);

  // Joins the explorer to the social challenge's active run, or starts a new one.
  async activateSocialChallenge(id: string, position: LocationDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/activate`, position));
  }
}
