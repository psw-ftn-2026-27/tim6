import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/payment/carts/mine';

@Injectable({ providedIn: 'root' })
export class Cart {
  private readonly http = inject(HttpClient);

  async removeItem(entityId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/items/${entityId}`));
  }
}
