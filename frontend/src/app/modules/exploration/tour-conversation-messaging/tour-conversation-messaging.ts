import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { MuteParticipantDto, SendMessageDto } from '../api/exploration-api-types';

const BASE_URL = '/api/exploration/conversations';

@Injectable({ providedIn: 'root' })
export class TourConversationMessaging {
  private readonly http = inject(HttpClient);

  open(tourId: string): Promise<string> {
    return firstValueFrom(this.http.post<string>(`${BASE_URL}/tour/${tourId}`, {}));
  }

  send(conversationId: string, dto: SendMessageDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(`${BASE_URL}/${conversationId}/messages`, dto));
  }

  async delete(messageId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/messages/${messageId}`));
  }

  async mute(conversationId: string, dto: MuteParticipantDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${conversationId}/mute`, dto));
  }

  async unmute(conversationId: string, participantId: string): Promise<void> {
    await firstValueFrom(
      this.http.delete<void>(`${BASE_URL}/${conversationId}/mute/${participantId}`),
    );
  }
}
