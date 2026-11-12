import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/payment/wallets/mine';

@Injectable({ providedIn: 'root' })
export class MyWallet {
  private readonly http = inject(HttpClient);

  // Creates the wallet when the user has none; otherwise changes nothing.
  async open(): Promise<void> {
    await firstValueFrom(this.http.post<void>(BASE_URL, null));
  }
}
