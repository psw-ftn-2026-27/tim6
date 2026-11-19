import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, max, min, required, validate } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ProductDto } from '../../api/payment-api-types';
import { CouponAuthoring } from '../coupon-authoring';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-create-coupon',
  styleUrl: './create-coupon.scss',
  templateUrl: './create-coupon.html',
})
export class CreateCoupon {
  private readonly couponAuthoring = inject(CouponAuthoring);
  private readonly router = inject(Router);

  protected readonly products = httpResource<ProductDto[]>(
    () => '/api/payment/coupons/products',
  );

  protected readonly form = form(
    signal({
      productId: '',
      discountPercentage: null as number | null,
      validUntil: '',
    }),
    (path) => {
      required(path.discountPercentage, { message: 'Discount percentage is required.' });
      min(path.discountPercentage, 1, { message: 'Discount must be greater than 0%.' });
      max(path.discountPercentage, 100, { message: 'Discount cannot exceed 100%.' });
      validate(path.discountPercentage, ({ value }) => {
        const discount = value();
        if (discount === null || Number.isInteger(discount)) {
          return null;
        }
        return { kind: 'wholeNumber', message: 'Discount must be a whole number.' };
      });
      required(path.validUntil, { message: 'Valid until date is required.' });
      validate(path.validUntil, ({ value }) => {
        const validUntil = value();
        if (validUntil === '' || validUntil > localDate(new Date())) {
          return null;
        }
        return { kind: 'futureDate', message: 'Valid until date must be in the future.' };
      });
    },
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    if (!this.form().valid()) {
      this.form().markAsTouched();
      return;
    }

    const discountPercentage = this.form.discountPercentage().value();
    if (discountPercentage === null) {
      return;
    }

    this.pending.set(true);
    this.error.set(null);
    try {
      await this.couponAuthoring.create({
        productId: this.form.productId().value() || null,
        discountPercentage,
        validUntil: endOfDayUtc(this.form.validUntil().value()),
      });
      await this.router.navigate(['/payment/coupons']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the coupon.'));
    } finally {
      this.pending.set(false);
    }
  }
}

function localDate(value: Date): string {
  const year = value.getFullYear();
  const month = String(value.getMonth() + 1).padStart(2, '0');
  const day = String(value.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function endOfDayUtc(value: string): string {
  return new Date(`${value}T23:59:59.999`).toISOString();
}
