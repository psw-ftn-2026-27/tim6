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

export interface ProductDto {
  id: string;
  entityId: string;
  name: string;
  price: number;
}

export interface CartItemDto {
  entityId: string;
  name: string;
  price: number;
  isAvailable: boolean;
}

export interface CartDto {
  items: CartItemDto[];
  totalPrice: number;
  walletBalance: number;
}

export interface PurchaseDto {
  id: string;
  entityId: string;
  name: string;
  purchasedAt: string;
}
