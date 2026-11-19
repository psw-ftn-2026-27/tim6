import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class Catalog {
  private readonly http = inject(HttpClient);

  // The cart is created on the first product the explorer adds.
  async addToCart(entityId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`/api/payment/carts/mine/items/${entityId}`, null));
  }
}
