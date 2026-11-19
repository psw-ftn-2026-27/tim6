export interface TransactionDto {
  id: string;
  amount: number;
  createdAt: string;
  isCancelled: boolean;
  cancelledAt: string | null;
  cancellationReason: string | null;
}

export interface PaymentDto {
  id: string;
  entityId: string;
  name: string;
  price: number;
  paidAt: string;
}

export interface WalletDto {
  balance: number;
  transactions: TransactionDto[];
  payments: PaymentDto[];
}

export interface WalletSummaryDto {
  id: string;
  email: string | null;
  balance: number;
}

export interface WalletDetailsDto {
  id: string;
  email: string | null;
  balance: number;
  transactions: TransactionDto[];
}

export interface DepositDto {
  amount: number;
}

export interface CancellationDto {
  reason: string;
}

export interface CreateGiftCardDto {
  recipientEmail: string;
  amount: number;
  message: string;
}

export type GiftCardStatus = 'Valid' | 'Redeemed' | 'Expired';

export interface GiftCardDto {
  id: string;
  senderId: string;
  senderEmail: string | null;
  recipientId: string;
  recipientEmail: string | null;
  amount: number;
  message: string;
  createdAt: string;
  expiresAt: string;
  status: GiftCardStatus;
}

export interface ProductDto {
  id: string;
  entityId: string;
  name: string;
  price: number;
}

export interface CreateCouponDto {
  productId: string | null;
  discountPercentage: number;
  validUntil: string;
}

export interface CouponDto {
  id: string;
  ownerId: string;
  productId: string | null;
  productName: string | null;
  discountPercentage: number;
  validUntil: string;
  code: string;
}

export interface CartItemDto {
  entityId: string;
  name: string;
  price: number;
  isAvailable: boolean;
  couponDiscount: number;
  priceAfterCoupon: number;
}

export interface CartDto {
  items: CartItemDto[];
  totalPrice: number;
  walletBalance: number;
  totalAfterCoupon: number;
  giftCardCoverage: number;
  walletCharge: number;
  giftCardWaste: number;
}

export interface PurchaseDto {
  id: string;
  entityId: string;
  name: string;
  purchasedAt: string;
}
