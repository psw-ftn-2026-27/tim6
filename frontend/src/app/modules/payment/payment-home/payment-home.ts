import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../core/auth/auth';
import { serverMessage } from '../../../shared/util/server-message';
import { MyWallet } from '../my-wallet/my-wallet';

@Component({
  imports: [RouterLink],
  selector: 'app-payment-home',
  styleUrl: './payment-home.scss',
  templateUrl: './payment-home.html',
})
export class PaymentHome {
  private readonly myWallet = inject(MyWallet);
  private readonly router = inject(Router);
  private readonly auth = inject(Auth);

  // An administrator manages every wallet, so the page shows "Novčanici" instead of "Moj novčanik".
  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // The wallet page only reads, so the wallet is opened (created on first use) before going there.
  protected async openWallet(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.myWallet.open();
      await this.router.navigate(['/payment/wallet']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Novčanik nije moguće otvoriti.'));
    } finally {
      this.pending.set(false);
    }
  }
}
