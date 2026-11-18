import { httpResource } from '@angular/common/http';
import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { ProductDto } from '../../api/payment-api-types';

const PAGE_SIZE = 20;

@Component({
  imports: [RouterLink],
  selector: 'app-product-list',
  styleUrl: './product-list.scss',
  templateUrl: './product-list.html',
})
export class ProductList {
  protected readonly page = signal(1);

  // The resource reads the page signal, so changing the page loads that page again.
  protected readonly products = httpResource<PageResult<ProductDto>>(
    () => `/api/payment/products/offer?page=${this.page()}&pageSize=${PAGE_SIZE}`,
  );

  protected readonly pageCount = computed(() => {
    const totalCount = this.products.value()?.totalCount ?? 0;
    return Math.max(1, Math.ceil(totalCount / PAGE_SIZE));
  });

  protected previousPage(): void {
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.page.update((page) => page + 1);
  }
}
