import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/social';

@Injectable({ providedIn: 'root' })
export class Following {
  private readonly http = inject(HttpClient);

  async cancelRequest(requestId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/follow-requests/${requestId}`));
  }

  async unfollow(userId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/following/${userId}`));
  }
}
