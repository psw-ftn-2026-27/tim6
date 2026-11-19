import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { LocationDto } from '../api/games-api-types';

const BASE_URL = '/api/games/location-challenges';

@Injectable({ providedIn: 'root' })
export class LocationChallengeSolving {
  private readonly http = inject(HttpClient);

  // Opens a new attempt or continues the open one; the server decides which. Returns the
  // attempt id.
  async activate(challengeId: string, position: LocationDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(`${BASE_URL}/${challengeId}/activate`, position));
  }

  // Reveals the hidden point the explorer stands at; the server rejects a check with no point
  // nearby.
  async checkLocation(challengeId: string, position: LocationDto): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/${challengeId}/check-location`, position),
    );
  }
}
