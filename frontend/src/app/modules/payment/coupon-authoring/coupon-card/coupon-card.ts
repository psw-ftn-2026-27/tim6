import { Component, input } from '@angular/core';
import { CouponDto } from '../../api/payment-api-types';

@Component({
  selector: 'app-coupon-card',
  styleUrl: './coupon-card.scss',
  templateUrl: './coupon-card.html',
})
export class CouponCard {
  readonly coupon = input.required<CouponDto>();

  protected date(value: string): string {
    return new Date(value).toLocaleDateString('en-GB', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
      timeZone: 'UTC',
    });
  }
}
