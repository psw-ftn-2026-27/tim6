import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CreateCouponDto } from '../api/payment-api-types';

const BASE_URL = '/api/payment/coupons';

@Injectable({ providedIn: 'root' })
export class CouponAuthoring {
  private readonly http = inject(HttpClient);

  async create(dto: CreateCouponDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(BASE_URL, dto));
  }
}
