import type { Role } from "@/types/auth";

// WalletOwnerType enum (Employer=0, Developer=1, Admin=2)
export const WalletOwnerType = { Employer: 0, Developer: 1, Admin: 2 } as const;
export type WalletOwnerType = (typeof WalletOwnerType)[keyof typeof WalletOwnerType];

// WalletTransactionType enum  
export const WalletTransactionType = {
  Deposit: 0,
  Withdrawal: 1,
  ProjectPayment: 2,
  DailyTaskPayment: 3,
  PlatformFee: 4,
  Refund: 5
} as const;
export type WalletTransactionType = (typeof WalletTransactionType)[keyof typeof WalletTransactionType];

export interface WalletDto {
  id: number;
  userId: number;
  ownerType: WalletOwnerType;
  balance: number;
  totalDeposited: number;
  totalWithdrawn: number;
  createdAt: string;
  updatedAt: string;
  transactions?: WalletTransactionDto[];
}

export interface WalletTransactionDto {
  id: number;
  walletId: number;
  amount: number;
  type: WalletTransactionType;
  description: string;
  relatedEntityId?: number | null;
  createdAt: string;
}

export interface AdminTransactionDto {
  id: number;
  walletId: number;
  userId: number;
  userName?: string | null;
  userEmail?: string | null;
  amount: number;
  type: WalletTransactionType;
  description?: string | null;
  relatedEntityId?: number | null;
  createdAt: string;
}

export interface DepositInput {
  amount: number;
  description?: string;
}export interface WithdrawInput {
  amount: number;
  description?: string;
}

export interface CreditDebitInput {
  targetUserId: number;
  amount: number;
  description?: string;
}
