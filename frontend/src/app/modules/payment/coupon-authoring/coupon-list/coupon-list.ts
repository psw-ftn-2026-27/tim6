import { httpResource } from '@angular/common/http';
import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { CouponDto } from '../../api/payment-api-types';
import { CouponCard } from '../coupon-card/coupon-card';

const PAGE_SIZE = 20;

@Component({
  imports: [CouponCard, RouterLink],
  selector: 'app-coupon-list',
  styleUrl: './coupon-list.scss',
  templateUrl: './coupon-list.html',
})
export class CouponList {
  protected readonly page = signal(1);
  protected readonly coupons = httpResource<PageResult<CouponDto>>(
    () => `/api/payment/coupons?page=${this.page()}&pageSize=${PAGE_SIZE}`,
  );

  protected readonly pageCount = computed(() => {
    const totalCount = this.coupons.value()?.totalCount ?? 0;
    return Math.max(1, Math.ceil(totalCount / PAGE_SIZE));
  });

  protected previousPage(): void {
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.page.update((page) => page + 1);
  }
}
