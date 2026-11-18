import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/social/clubs';

@Injectable({ providedIn: 'root' })
export class ClubModeration {
  private readonly http = inject(HttpClient);

  async acceptJoinRequest(clubId: string, applicantId: string): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/${clubId}/join-requests/${applicantId}/accept`, {}),
    );
  }

  async rejectJoinRequest(clubId: string, applicantId: string): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/${clubId}/join-requests/${applicantId}/reject`, {}),
    );
  }
}
