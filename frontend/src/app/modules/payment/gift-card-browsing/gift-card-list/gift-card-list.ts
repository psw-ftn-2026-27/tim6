import { httpResource } from '@angular/common/http';
import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { GiftCardDto } from '../../api/payment-api-types';
import { GiftCardCard } from '../gift-card-card/gift-card-card';

const PAGE_SIZE = 10;

@Component({
  imports: [GiftCardCard, RouterLink],
  selector: 'app-gift-card-list',
  styleUrl: './gift-card-list.scss',
  templateUrl: './gift-card-list.html',
})
export class GiftCardList {
  protected readonly sentPage = signal(1);
  protected readonly receivedPage = signal(1);

  protected readonly sent = httpResource<PageResult<GiftCardDto>>(
    () => `/api/payment/gift-cards/sent?page=${this.sentPage()}&pageSize=${PAGE_SIZE}`,
  );
  protected readonly received = httpResource<PageResult<GiftCardDto>>(
    () => `/api/payment/gift-cards/received?page=${this.receivedPage()}&pageSize=${PAGE_SIZE}`,
  );

  protected readonly hasMoreSent = computed(
    () => this.sentPage() * PAGE_SIZE < (this.sent.value()?.totalCount ?? 0),
  );
  protected readonly hasMoreReceived = computed(
    () => this.receivedPage() * PAGE_SIZE < (this.received.value()?.totalCount ?? 0),
  );

  protected previousSent(): void {
    this.sentPage.update((page) => Math.max(page - 1, 1));
  }

  protected nextSent(): void {
    this.sentPage.update((page) => page + 1);
  }

  protected previousReceived(): void {
    this.receivedPage.update((page) => Math.max(page - 1, 1));
  }

  protected nextReceived(): void {
    this.receivedPage.update((page) => page + 1);
  }
}
