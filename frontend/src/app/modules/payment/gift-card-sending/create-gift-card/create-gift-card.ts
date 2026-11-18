import { Component, inject, signal } from '@angular/core';
import {
  FormField,
  form,
  max,
  maxLength,
  min,
  pattern,
  required,
  validate,
} from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { GiftCardSending } from '../gift-card-sending';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-create-gift-card',
  styleUrl: './create-gift-card.scss',
  templateUrl: './create-gift-card.html',
})
export class CreateGiftCard {
  private readonly giftCardSending = inject(GiftCardSending);
  private readonly router = inject(Router);

  protected readonly form = form(
    signal({ recipientEmail: '', amount: null as number | null, message: '' }),
    (path) => {
      required(path.recipientEmail, { message: 'Recipient email is required.' });
      pattern(path.recipientEmail, /^[^\s@]+@[^\s@]+\.[^\s@]+$/, {
        message: 'Enter a valid email address.',
      });
      required(path.amount, { message: 'Amount is required.' });
      min(path.amount, 10, { message: 'Amount must be at least 10 EC.' });
      max(path.amount, 1000, { message: 'Amount cannot exceed 1000 EC.' });
      validate(path.amount, ({ value }) => {
        const amount = value();
        if (amount === null || Number.isInteger(amount)) {
          return null;
        }
        return { kind: 'wholeNumber', message: 'Amount must be a whole number.' };
      });
      required(path.message, { message: 'Message is required.' });
      maxLength(path.message, 200, { message: 'Message cannot exceed 200 characters.' });
    },
  );

  protected readonly reviewing = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected review(event: Event): void {
    event.preventDefault();
    if (!this.form().valid()) {
      this.form().markAsTouched();
      return;
    }
    this.error.set(null);
    this.reviewing.set(true);
  }

  protected edit(): void {
    this.error.set(null);
    this.reviewing.set(false);
  }

  protected async confirm(): Promise<void> {
    const amount = this.form.amount().value();
    if (amount === null) {
      return;
    }

    this.pending.set(true);
    this.error.set(null);
    try {
      await this.giftCardSending.create({
        recipientEmail: this.form.recipientEmail().value(),
        amount,
        message: this.form.message().value(),
      });
      await this.router.navigate(['/payment/gift-cards']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the gift card.'));
    } finally {
      this.pending.set(false);
    }
  }
}
