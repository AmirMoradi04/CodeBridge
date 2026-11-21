"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { walletsApi } from "@/lib/api/wallets";
import type { DepositInput, WithdrawInput } from "@/types/wallet";

export const walletKeys = {
  all: ["wallets"] as const,
  me: () => ["wallets", "me"] as const,
  transactions: () => ["wallets", "transactions"] as const,
  admin: () => ["wallets", "admin"] as const,
  systemTransactions: () => ["wallets", "system-transactions"] as const,
};

export function useMyWallet() {
  return useQuery({
    queryKey: walletKeys.me(),
    queryFn: () => walletsApi.getMyWallet(),
  });
}

export function useWalletTransactions() {
  return useQuery({
    queryKey: walletKeys.transactions(),
    queryFn: () => walletsApi.getMyTransactions(),
  });
}

export function useAdminWallet() {
  return useQuery({
    queryKey: walletKeys.admin(),
    queryFn: () => walletsApi.getAdminWallet(),
  });
}

/** فهرست سراسری همه‌ی تراکنش‌های سیستم (فقط ادمین). */
export function useSystemTransactions() {
  return useQuery({
    queryKey: walletKeys.systemTransactions(),
    queryFn: () => walletsApi.getSystemTransactions(),
  });
}

export function useDeposit() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (input: DepositInput) => walletsApi.deposit(input),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: walletKeys.me() });
      void qc.invalidateQueries({ queryKey: walletKeys.transactions() });
    },
  });
}

export function useWithdraw() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (input: WithdrawInput) => walletsApi.withdraw(input),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: walletKeys.me() });
      void qc.invalidateQueries({ queryKey: walletKeys.transactions() });
    },
  });
}

export function useEmployerDepositToEscrow() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, amount }: { projectId: number; amount: number }) =>
      walletsApi.deposit({ amount, description: `واریز بودجه پروژه ${projectId}` }),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: walletKeys.me() });
      void qc.invalidateQueries({ queryKey: walletKeys.transactions() });
    },
  });
}

export function useCreditUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (input: import("@/types/wallet").CreditDebitInput) =>
      walletsApi.creditUser(input),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: walletKeys.admin() });
      void qc.invalidateQueries({ queryKey: walletKeys.transactions() });
    },
  });
}
