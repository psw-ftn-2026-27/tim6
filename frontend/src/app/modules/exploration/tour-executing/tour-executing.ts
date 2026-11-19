import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { LocationDto } from '../api/exploration-api-types';

const BASE_URL = '/api/exploration/tour-executions';

@Injectable({ providedIn: 'root' })
export class TourExecuting {
  private readonly http = inject(HttpClient);

  async start(tourId: string, position: LocationDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(BASE_URL, { tourId, position }));
  }

  async abandon(tourId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/active/${tourId}/abandon`, {}));
  }
}
