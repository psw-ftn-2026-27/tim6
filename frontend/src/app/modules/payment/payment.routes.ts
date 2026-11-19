import { Routes } from '@angular/router';
import { ProductList } from './catalog/product-list/product-list';
import { CreateCoupon } from './coupon-authoring/create-coupon/create-coupon';
import { CouponList } from './coupon-authoring/coupon-list/coupon-list';
import { Wallet } from './my-wallet/wallet/wallet';
import { PaymentHome } from './payment-home/payment-home';
import { WalletDetails } from './wallet-management/wallet-details/wallet-details';
import { WalletList } from './wallet-management/wallet-list/wallet-list';

export const paymentRoutes: Routes = [
  { path: '', component: PaymentHome },
  { path: 'wallet', component: Wallet },
  { path: 'wallets', component: WalletList },
  { path: 'wallets/:id', component: WalletDetails },
  { path: 'catalog', component: ProductList },
  { path: 'coupons/new', component: CreateCoupon },
  { path: 'coupons', component: CouponList },
];
