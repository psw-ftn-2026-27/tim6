import { httpResource } from '@angular/common/http';
import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { WalletSummaryDto } from '../../api/payment-api-types';

const PAGE_SIZE = 20;

@Component({
  imports: [RouterLink],
  selector: 'app-wallet-list',
  styleUrl: './wallet-list.scss',
  templateUrl: './wallet-list.html',
})
export class WalletList {
  protected readonly page = signal(1);

  // The resource reads the page signal, so changing the page loads that page again.
  protected readonly wallets = httpResource<PageResult<WalletSummaryDto>>(
    () => `/api/payment/wallets?page=${this.page()}&pageSize=${PAGE_SIZE}`,
  );

  protected readonly pageCount = computed(() => {
    const totalCount = this.wallets.value()?.totalCount ?? 0;
    return Math.max(1, Math.ceil(totalCount / PAGE_SIZE));
  });

  protected previousPage(): void {
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.page.update((page) => page + 1);
  }
}
