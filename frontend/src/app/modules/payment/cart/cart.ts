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

  // Buys the whole cart; the server applies the selected benefits and empties the cart.
  async purchase(couponCode: string | null, giftCardId: string | null): Promise<void> {
    const params = new URLSearchParams();
    if (couponCode) {
      params.set('couponCode', couponCode);
    }
    if (giftCardId) {
      params.set('giftCardId', giftCardId);
    }
    const query = params.toString();
    const url = `/api/payment/purchases/mine${query ? `?${query}` : ''}`;
    await firstValueFrom(this.http.post<void>(url, null));
  }
}
