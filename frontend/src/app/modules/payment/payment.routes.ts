import { Routes } from '@angular/router';
import { Wallet } from './my-wallet/wallet/wallet';
import { PaymentHome } from './payment-home/payment-home';

export const paymentRoutes: Routes = [
  { path: '', component: PaymentHome },
  { path: 'wallet', component: Wallet },
];
