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
