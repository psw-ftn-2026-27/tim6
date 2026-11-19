import { Routes } from '@angular/router';
import { MyCart } from './cart/my-cart/my-cart';
import { ProductList } from './catalog/product-list/product-list';
import { Wallet } from './my-wallet/wallet/wallet';
import { PaymentHome } from './payment-home/payment-home';
import { MyPurchases } from './purchase/my-purchases/my-purchases';
import { WalletDetails } from './wallet-management/wallet-details/wallet-details';
import { WalletList } from './wallet-management/wallet-list/wallet-list';

export const paymentRoutes: Routes = [
  { path: '', component: PaymentHome },
  { path: 'wallet', component: Wallet },
  { path: 'wallets', component: WalletList },
  { path: 'wallets/:id', component: WalletDetails },
  { path: 'catalog', component: ProductList },
  { path: 'cart', component: MyCart },
  { path: 'purchases', component: MyPurchases },
];
