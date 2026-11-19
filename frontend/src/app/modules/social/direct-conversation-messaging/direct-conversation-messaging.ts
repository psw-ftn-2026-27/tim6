import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { SendMessageDto } from '../api/social-api-types';

const BASE_URL = '/api/social/conversations';

@Injectable({ providedIn: 'root' })
export class DirectConversationMessaging {
  private readonly http = inject(HttpClient);

  open(otherUserId: string): Promise<string> {
    return firstValueFrom(this.http.post<string>(`${BASE_URL}/direct/${otherUserId}`, {}));
  }

  send(conversationId: string, dto: SendMessageDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(`${BASE_URL}/${conversationId}/messages`, dto));
  }

  async delete(messageId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/messages/${messageId}`));
  }
}
