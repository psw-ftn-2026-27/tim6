import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { Analytics } from '../../../../shared/analytics/analytics';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { ProductDto } from '../../api/payment-api-types';
import { Catalog } from '../catalog';

const PAGE_SIZE = 20;

@Component({
  imports: [RouterLink],
  selector: 'app-product-list',
  styleUrl: './product-list.scss',
  templateUrl: './product-list.html',
})
export class ProductList {
  private readonly catalog = inject(Catalog);
  private readonly auth = inject(Auth);
  private readonly analytics = inject(Analytics);

  // Only an explorer buys, so only an explorer sees "Dodaj u korpu" and "Korpa".
  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );

  protected readonly page = signal(1);

  // The resource reads the page signal, so changing the page loads that page again.
  protected readonly products = httpResource<PageResult<ProductDto>>(
    () => `/api/payment/products/offer?page=${this.page()}&pageSize=${PAGE_SIZE}`,
  );

  protected readonly pageCount = computed(() => {
    const totalCount = this.products.value()?.totalCount ?? 0;
    return Math.max(1, Math.ceil(totalCount / PAGE_SIZE));
  });

  protected readonly pending = signal(false);

  // The outcome of the latest "Dodaj u korpu", shown next to the row of that product.
  protected readonly addOutcome = signal<{
    productId: string;
    added: boolean;
    message: string;
  } | null>(null);

  protected previousPage(): void {
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.page.update((page) => page + 1);
  }

  protected async addToCart(product: ProductDto): Promise<void> {
    this.pending.set(true);
    this.addOutcome.set(null);
    try {
      await this.catalog.addToCart(product.entityId);
      this.analytics.productAddedToCart(product.id);
      this.addOutcome.set({ productId: product.id, added: true, message: 'Dodato u korpu.' });
    } catch (failure) {
      // The server says why it refused: already in the cart, already purchased, not in the offer,
      // or not found.
      this.addOutcome.set({
        productId: product.id,
        added: false,
        message: serverMessage(failure, 'Proizvod nije moguće dodati u korpu.'),
      });
    } finally {
      this.pending.set(false);
    }
  }
}
