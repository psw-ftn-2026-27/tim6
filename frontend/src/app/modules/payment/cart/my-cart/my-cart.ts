import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Analytics } from '../../../../shared/analytics/analytics';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { CartDto, GiftCardDto } from '../../api/payment-api-types';
import { Cart } from '../cart';
import { CartReceipt } from '../cart-receipt/cart-receipt';
import { GiftCardPicker } from '../gift-card-picker/gift-card-picker';

const GIFT_CARD_PAGE_SIZE = 5;

@Component({
  imports: [CartReceipt, FormField, GiftCardPicker, RouterLink],
  selector: 'app-my-cart',
  styleUrl: './my-cart.scss',
  templateUrl: './my-cart.html',
})
export class MyCart {
  private readonly cartService = inject(Cart);
  private readonly analytics = inject(Analytics);

  protected readonly benefitForm = form(signal({ couponCode: '' }));
  protected readonly appliedCouponCode = signal<string | null>(null);
  protected readonly selectedGiftCardId = signal<string | null>(null);
  protected readonly benefitErrorTarget = signal<'coupon' | 'gift-card' | null>(null);
  private readonly cartRequestVersion = signal(0);

  // An explorer without a cart gets an empty cart with 0 EC, and without a wallet a balance of
  // 0 EC, so there is no 404 to handle.
  protected readonly cart = httpResource<CartDto>(() => {
    this.cartRequestVersion();
    const params = new URLSearchParams();
    const couponCode = this.appliedCouponCode();
    const giftCardId = this.selectedGiftCardId();
    if (couponCode) {
      params.set('couponCode', couponCode);
    }
    if (giftCardId) {
      params.set('giftCardId', giftCardId);
    }
    const query = params.toString();
    return `/api/payment/carts/mine${query ? `?${query}` : ''}`;
  });
  private readonly previousCart = signal<CartDto | null>(null);
  protected readonly displayedCart = computed(() =>
    this.cart.hasValue() ? this.cart.value() : this.previousCart(),
  );

  protected readonly giftCardPage = signal(1);
  protected readonly receivedGiftCards = httpResource<PageResult<GiftCardDto>>(
    () =>
      `/api/payment/gift-cards/received?page=${this.giftCardPage()}&pageSize=${GIFT_CARD_PAGE_SIZE}`,
  );
  protected readonly validGiftCards = computed(
    () => this.receivedGiftCards.value()?.items.filter((giftCard) => giftCard.status === 'Valid') ?? [],
  );
  protected readonly hasMoreGiftCards = computed(
    () =>
      this.giftCardPage() * GIFT_CARD_PAGE_SIZE <
      (this.receivedGiftCards.value()?.totalCount ?? 0),
  );
  protected readonly benefitError = computed(() => {
    const failure = this.cart.error();
    return failure ? serverMessage(failure, 'Could not apply the selected card or coupon.') : null;
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // The confirmation of the latest purchase; the empty cart and the new balance come from the reload.
  protected readonly purchased = signal(false);

  protected applyCoupon(): void {
    this.rememberCurrentCart();
    this.error.set(null);
    this.purchased.set(false);
    this.benefitErrorTarget.set('coupon');
    this.appliedCouponCode.set(this.benefitForm.couponCode().value().trim() || null);
    this.cartRequestVersion.update((version) => version + 1);
  }

  protected selectGiftCard(giftCardId: string): void {
    this.rememberCurrentCart();
    this.error.set(null);
    this.purchased.set(false);
    if (!this.cart.error()) {
      this.benefitErrorTarget.set('gift-card');
    }
    this.selectedGiftCardId.set(giftCardId);
    this.cartRequestVersion.update((version) => version + 1);
  }

  protected removeCoupon(): void {
    this.rememberCurrentCart();
    this.error.set(null);
    this.purchased.set(false);
    if (this.benefitErrorTarget() === 'coupon') {
      this.benefitErrorTarget.set(this.selectedGiftCardId() ? 'gift-card' : null);
    }
    this.benefitForm.couponCode().value.set('');
    this.appliedCouponCode.set(null);
  }

  protected changeGiftCardPage(page: number): void {
    this.clearGiftCard();
    this.giftCardPage.set(page);
  }

  protected removeGiftCard(): void {
    this.clearGiftCard();
  }

  // The page reloads only its cart resource, so the list and the total always come from the server.
  protected async removeItem(entityId: string): Promise<void> {
    const removesLastItem = this.displayedCart()?.items.length === 1;
    this.pending.set(true);
    this.error.set(null);
    this.purchased.set(false);
    try {
      await this.cartService.removeItem(entityId);
      if (removesLastItem) {
        this.resetBenefitsForEmptyCart();
      } else {
        this.cart.reload();
      }
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not remove the item.'));
    } finally {
      this.pending.set(false);
    }
  }

  // The server says why it refused (not enough EC, an unavailable item, an empty cart, no cart),
  // and on refusal nothing changes, so the page only shows that message.
  protected async purchase(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.purchased.set(false);
    try {
      await this.cartService.purchase(this.appliedCouponCode(), this.selectedGiftCardId());
      this.analytics.cartPurchased();
      this.purchased.set(true);
      this.benefitForm().reset({ couponCode: '' });
      this.previousCart.set(null);
      this.appliedCouponCode.set(null);
      this.selectedGiftCardId.set(null);
      this.benefitErrorTarget.set(null);
      this.cart.reload();
      this.receivedGiftCards.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Purchase failed.'));
    } finally {
      this.pending.set(false);
    }
  }

  private clearGiftCard(): void {
    this.rememberCurrentCart();
    this.selectedGiftCardId.set(null);
    if (this.benefitErrorTarget() === 'gift-card') {
      this.benefitErrorTarget.set(this.appliedCouponCode() ? 'coupon' : null);
    }
  }

  private rememberCurrentCart(): void {
    if (this.cart.hasValue()) {
      this.previousCart.set(this.cart.value());
    }
  }

  private resetBenefitsForEmptyCart(): void {
    this.benefitForm().reset({ couponCode: '' });
    this.previousCart.set(null);
    this.appliedCouponCode.set(null);
    this.selectedGiftCardId.set(null);
    this.benefitErrorTarget.set(null);
    this.cartRequestVersion.update((version) => version + 1);
  }
}
