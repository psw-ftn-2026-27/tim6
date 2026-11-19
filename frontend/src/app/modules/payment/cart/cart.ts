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

  // Buys the whole cart; the server empties the cart and takes the EC from the wallet.
  async purchase(): Promise<void> {
    await firstValueFrom(this.http.post<void>('/api/payment/purchases/mine', null));
  }
}
