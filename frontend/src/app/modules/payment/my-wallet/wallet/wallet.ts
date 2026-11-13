import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { WalletDto } from '../../api/payment-api-types';

@Component({
  imports: [RouterLink],
  selector: 'app-wallet',
  styleUrl: './wallet.scss',
  templateUrl: './wallet.html',
})
export class Wallet {
  protected readonly wallet = httpResource<WalletDto>(() => '/api/payment/wallets/mine');

  // The server answers 404 when the user opens this page before opening the wallet on /payment.
  protected readonly notFound = computed(() => {
    const error = this.wallet.error();
    return error instanceof HttpErrorResponse && error.status === 404;
  });

  // The server sends UTC time as "2026-11-12T09:30:00..."; the page shows "2026-11-12 09:30".
  protected time(value: string): string {
    return value.slice(0, 16).replace('T', ' ');
  }
}
