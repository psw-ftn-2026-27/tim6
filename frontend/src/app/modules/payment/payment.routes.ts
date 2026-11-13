import { Routes } from '@angular/router';
import { Wallet } from './my-wallet/wallet/wallet';
import { PaymentHome } from './payment-home/payment-home';
import { WalletDetails } from './wallet-management/wallet-details/wallet-details';
import { WalletList } from './wallet-management/wallet-list/wallet-list';

export const paymentRoutes: Routes = [
  { path: '', component: PaymentHome },
  { path: 'wallet', component: Wallet },
  { path: 'wallets', component: WalletList },
  { path: 'wallets/:id', component: WalletDetails },
];
