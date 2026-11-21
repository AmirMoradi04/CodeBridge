import { api } from "@/lib/api/axios";
import type {
  AdminTransactionDto,
  CreditDebitInput,
  DepositInput,
  WalletDto,
  WalletTransactionDto,
  WithdrawInput
} from "@/types/wallet";

export const walletsApi = {
  getMyWallet: () => api.get<WalletDto>("/wallets/me").then((r) => r.data),

  getMyWalletWithTransactions: () =>
    api.get<WalletDto>("/wallets/me/transactions").then((r) => r.data),

  getMyTransactions: async () => {
    const wallet = await walletsApi.getMyWalletWithTransactions();
    return wallet.transactions ?? [];
  },

  deposit: (input: DepositInput) =>
    api.post<WalletDto>("/wallets/deposit", input).then((r) => r.data),

  withdraw: (input: WithdrawInput) =>
    api.post<WalletDto>("/wallets/withdraw", input).then((r) => r.data),

  getAdminWallet: () =>
    api.get<WalletDto>("/wallets/admin-wallet").then((r) => r.data),

  getAdminTransactions: async () => {
    const wallet = await walletsApi.getAdminWallet();
    return wallet.transactions ?? [];
  },

  // گزارش سراسری تراکنش‌ها برای ادمین
  getSystemTransactions: () =>
    api
      .get<AdminTransactionDto[]>("/wallets/admin/transactions")
      .then((r) => r.data),

  creditUser: (input: CreditDebitInput) =>
    api.post<WalletDto>("/wallets/credit", input).then((r) => r.data),

  debitUser: (input: CreditDebitInput) =>
    api.post<WalletDto>("/wallets/debit", input).then((r) => r.data)
};
