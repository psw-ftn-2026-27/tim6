import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form, min, required, validate } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { WalletDetailsDto } from '../../api/payment-api-types';
import { WalletManagement } from '../wallet-management';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-wallet-details',
  styleUrl: './wallet-details.scss',
  templateUrl: './wallet-details.html',
})
export class WalletDetails {
  private readonly walletManagement = inject(WalletManagement);

  readonly id = input.required<string>();

  protected readonly wallet = httpResource<WalletDetailsDto>(
    () => `/api/payment/wallets/${this.id()}`,
  );

  protected readonly notFound = computed(() => {
    const error = this.wallet.error();
    return error instanceof HttpErrorResponse && error.status === 404;
  });

  protected readonly pending = signal(false);

  // The amount is a positive whole number (P3), so the form never sends zero, a negative
  // or a decimal amount.
  protected readonly depositForm = form(signal({ amount: null as number | null }), (path) => {
    required(path.amount, { message: 'Iznos je obavezan.' });
    min(path.amount, 1, { message: 'Iznos mora biti najmanje 1.' });
    validate(path.amount, ({ value }) => {
      const amount = value();
      if (amount === null || Number.isInteger(amount)) {
        return null;
      }
      return { kind: 'wholeNumber', message: 'Iznos mora biti ceo broj.' };
    });
  });

  protected readonly depositError = signal<string | null>(null);

  // The payment whose cancellation section is open below it, or null when none is open.
  protected readonly cancellingTransactionId = signal<string | null>(null);

  protected readonly cancelForm = form(signal({ reason: '' }), (path) => {
    required(path.reason, { message: 'Razlog storniranja je obavezan.' });
  });

  protected readonly cancelError = signal<string | null>(null);

  protected async deposit(event: Event): Promise<void> {
    event.preventDefault();
    const amount = this.depositForm.amount().value();
    if (amount === null) {
      return;
    }
    this.pending.set(true);
    this.depositError.set(null);
    try {
      await this.walletManagement.deposit(this.id(), { amount });
      this.depositForm().reset({ amount: null });
      this.wallet.reload();
    } catch (failure) {
      this.depositError.set(serverMessage(failure, 'Uplatu nije moguće izvršiti.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected startCancelling(transactionId: string): void {
    this.cancellingTransactionId.set(transactionId);
    this.cancelError.set(null);
    this.cancelForm().reset({ reason: '' });
  }

  protected stopCancelling(): void {
    this.cancellingTransactionId.set(null);
    this.cancelError.set(null);
  }

  protected async cancelTransaction(event: Event, transactionId: string): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.cancelError.set(null);
    try {
      await this.walletManagement.cancelTransaction(this.id(), transactionId, {
        reason: this.cancelForm.reason().value(),
      });
      this.cancellingTransactionId.set(null);
      this.wallet.reload();
    } catch (failure) {
      // The section stays open, so the server's reason (P5, P6, P8, P9) is shown below the payment.
      this.cancelError.set(serverMessage(failure, 'Uplatu nije moguće stornirati.'));
    } finally {
      this.pending.set(false);
    }
  }

  // The server sends UTC time as "2026-11-12T09:30:00..."; the page shows "2026-11-12 09:30".
  protected time(value: string): string {
    return value.slice(0, 16).replace('T', ' ');
  }
}
