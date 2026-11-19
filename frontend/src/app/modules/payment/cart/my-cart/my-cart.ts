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

  // An explorer without a cart gets an empty cart with 0 EC, and without a wallet a balance of
  // 0 EC, so there is no 404 to handle.
  protected readonly cart = httpResource<CartDto>(() => '/api/payment/carts/mine');

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // The confirmation of the latest purchase; the empty cart and the new balance come from the reload.
  protected readonly purchased = signal(false);

  // The page reloads the cart after removing, so the list and the total always come from the server.
  protected async removeItem(entityId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.purchased.set(false);
    try {
      await this.cartService.removeItem(entityId);
      this.cart.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Stavku nije moguće ukloniti.'));
    } finally {
      this.pending.set(false);
    }
  }

  // The server says why it refused (not enough EC, an unavailable item, an empty cart, no cart),
  // and on refusal nothing changes, so the page only shows that message.
  protected async purchase(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.purchased.set(false);
    try {
      await this.cartService.purchase();
      this.purchased.set(true);
      this.cart.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Kupovina nije uspela.'));
    } finally {
      this.pending.set(false);
    }
  }
}
