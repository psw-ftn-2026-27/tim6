import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CancellationDto, DepositDto } from '../api/payment-api-types';

const BASE_URL = '/api/payment/wallets';

@Injectable({ providedIn: 'root' })
export class WalletManagement {
  private readonly http = inject(HttpClient);

  async deposit(walletId: string, dto: DepositDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${walletId}/deposits`, dto));
  }

  async cancelTransaction(
    walletId: string,
    transactionId: string,
    dto: CancellationDto,
  ): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/${walletId}/transactions/${transactionId}/cancel`, dto),
    );
  }
}
