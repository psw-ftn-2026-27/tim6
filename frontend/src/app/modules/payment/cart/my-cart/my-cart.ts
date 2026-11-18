import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { CartDto } from '../../api/payment-api-types';
import { Cart } from '../cart';

@Component({
  imports: [RouterLink],
  selector: 'app-my-cart',
  styleUrl: './my-cart.scss',
  templateUrl: './my-cart.html',
})
export class MyCart {
  private readonly cartService = inject(Cart);

  // An explorer without a cart gets an empty cart with 0 EC, so there is no 404 to handle.
  protected readonly cart = httpResource<CartDto>(() => '/api/payment/carts/mine');

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // The page reloads the cart after removing, so the list and the total always come from the server.
  protected async removeItem(entityId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.cartService.removeItem(entityId);
      this.cart.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Stavku nije moguće ukloniti.'));
    } finally {
      this.pending.set(false);
    }
  }
}
