export interface TransactionDto {
  id: string;
  amount: number;
  createdAt: string;
  isCancelled: boolean;
  cancelledAt: string | null;
  cancellationReason: string | null;
}

export interface WalletDto {
  balance: number;
  transactions: TransactionDto[];
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
  discountPercentage: number;
  validUntil: string;
  code: string;
}
