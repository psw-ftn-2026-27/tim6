import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/social';

@Injectable({ providedIn: 'root' })
export class Following {
  private readonly http = inject(HttpClient);

  async sendFollowRequest(recipientId: string): Promise<string> {
    return firstValueFrom(this.http.post<string>(`${BASE_URL}/users/${recipientId}/follow-requests`, {}));
  }

  async cancelRequest(requestId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/follow-requests/${requestId}`));
  }

  async unfollow(userId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/following/${userId}`));
  }

  async acceptRequest(requestId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/follow-requests/${requestId}/accept`, {}));
  }

  async rejectRequest(requestId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/follow-requests/${requestId}/reject`, {}));
  }

  async acceptAndFollowBack(requestId: string): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/follow-requests/${requestId}/accept-and-follow-back`, {}),
    );
  }

  async removeFollower(followerId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/followers/${followerId}`));
  }
}
