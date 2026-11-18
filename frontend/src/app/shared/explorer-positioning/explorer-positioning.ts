import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CoordinatesDto } from '../api/coordinates-dto';

const BASE_URL = '/api/shared/positions';

// Saves where the explorer is. The page reads the position itself, with an httpResource on
// /api/shared/positions/mine, and reloads it after move succeeds.
@Injectable({ providedIn: 'root' })
export class ExplorerPositioning {
  private readonly http = inject(HttpClient);

  async move(coordinates: CoordinatesDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/mine`, coordinates));
  }
}
