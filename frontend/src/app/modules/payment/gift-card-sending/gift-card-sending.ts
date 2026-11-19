import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CreateGiftCardDto } from '../api/payment-api-types';

const BASE_URL = '/api/payment/gift-cards';

@Injectable({ providedIn: 'root' })
export class GiftCardSending {
  private readonly http = inject(HttpClient);

  async create(dto: CreateGiftCardDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(BASE_URL, dto));
  }
}
