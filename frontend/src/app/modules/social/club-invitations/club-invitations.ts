import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { InviteDto } from '../api/social-api-types';

const BASE_URL = '/api/social/clubs';

@Injectable({ providedIn: 'root' })
export class ClubInvitations {
  private readonly http = inject(HttpClient);

  async invite(clubId: string, dto: InviteDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${clubId}/invitations`, dto));
  }

  async withdrawInvitation(clubId: string, inviteeId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${clubId}/invitations/${inviteeId}`));
  }
}
