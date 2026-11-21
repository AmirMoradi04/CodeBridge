"use client";

import type { ColumnDef } from "@tanstack/react-table";
import { Receipt } from "lucide-react";
import { useSystemTransactions } from "@/lib/hooks/useWallets";
import { WalletTransactionType, type AdminTransactionDto } from "@/types/wallet";
import { Badge } from "@/components/ui/badge";
import { DataTable } from "@/components/shared/DataTable";
import { EmptyState } from "@/components/shared/EmptyState";

const TYPE_LABELS: Record<WalletTransactionType, string> = {
  [WalletTransactionType.Deposit]: "واریز",
  [WalletTransactionType.Withdrawal]: "برداشت",
  [WalletTransactionType.ProjectPayment]: "پرداخت پروژه",
  [WalletTransactionType.DailyTaskPayment]: "پرداخت تسک",
  [WalletTransactionType.PlatformFee]: "کارمزد",
  [WalletTransactionType.Refund]: "بازگشت وجه"
};

// تراکنش‌های ورودی سبز و خروجی قرمز نمایش داده می‌شوند
const CREDIT_TYPES = new Set<WalletTransactionType>([
  WalletTransactionType.Deposit,
  WalletTransactionType.ProjectPayment,
  WalletTransactionType.DailyTaskPayment,
  WalletTransactionType.Refund
]);

const formatAmount = (amount: number) =>
  new Intl.NumberFormat("fa-IR").format(Math.abs(amount));

export default function AdminTransactionsPage() {
  const { data: transactions, isLoading } = useSystemTransactions();

  const columns: ColumnDef<AdminTransactionDto>[] = [
    {
      accessorKey: "createdAt",
      header: "تاریخ",
      cell: ({ row }) => (
        <span className="whitespace-nowrap text-xs" dir="ltr">
          {new Date(row.original.createdAt).toLocaleString("fa-IR")}
        </span>
      )
    },
    {
      accessorKey: "userName",
      header: "کاربر",
      cell: ({ row }) => (
        <div className="flex flex-col">
          <span className="text-sm">{row.original.userName ?? "—"}</span>
          <span className="text-xs text-muted-foreground" dir="ltr">
            {row.original.userEmail ?? `#${row.original.userId}`}
          </span>
        </div>
      )
    },
    {
      accessorKey: "amount",
      header: "مبلغ (تومان)",
      cell: ({ row }) => (
        <span
          className={
            CREDIT_TYPES.has(row.original.type)
              ? "font-semibold text-emerald-600 dark:text-emerald-400"
              : "font-semibold text-destructive"
          }
        >
          {formatAmount(row.original.amount)}
        </span>
      )
    },
    {
      accessorKey: "type",
      header: "نوع",
      cell: ({ row }) => (
        <Badge variant="secondary">{TYPE_LABELS[row.original.type] ?? "—"}</Badge>
      )
    },
    {
      accessorKey: "description",
      header: "توضیحات",
      cell: ({ row }) => (
        <span className="block max-w-md truncate text-xs text-muted-foreground">
          {row.original.description ?? "—"}
        </span>
      )
    }
  ];

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-lg font-semibold">تراکنش‌های سیستم</h2>
        <p className="text-sm text-muted-foreground">
          همه‌ی واریز، برداشت و پرداخت‌های ثبت‌شده در کیف پول‌ها
        </p>
      </div>

      {isLoading ? (
        <p className="text-sm text-muted-foreground">در حال بارگذاری...</p>
      ) : !transactions || transactions.length === 0 ? (
        <EmptyState
          icon={Receipt}
          title="تراکنشی ثبت نشده"
          description="با انجام واریز یا پرداخت، تراکنش‌ها اینجا نمایش داده می‌شوند."
        />
      ) : (
        <DataTable
          columns={columns}
          data={transactions}
          emptyMessage="تراکنشی وجود ندارد."
          pageSize={15}
        />
      )}
    </div>
  );
}
