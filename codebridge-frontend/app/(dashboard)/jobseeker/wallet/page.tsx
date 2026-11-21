"use client";

import { WalletIcon } from "lucide-react";
import { useMyWallet, useWalletTransactions } from "@/lib/hooks/useWallets";
import { WalletTransactionType } from "@/types/wallet";
import { EmptyState } from "@/components/shared/EmptyState";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Table, TableBody, TableCell, TableHead, TableHeader, TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";

const transactionLabels: Record<number, string> = {
  [WalletTransactionType.Deposit]: "واریز",
  [WalletTransactionType.DailyTaskPayment]: "تسک روزانه",
};

export default function JobSeekerWalletPage() {
  const { data: wallet, isLoading: walletLoading } = useMyWallet();
  const { data: transactions, isLoading: txLoading } = useWalletTransactions();

  // Filter to only show deposits and daily task payments (not withdrawals/platform fees)
  const relevantTransactions = (transactions ?? []).filter(
    (tx) => tx.type === WalletTransactionType.Deposit || tx.type === WalletTransactionType.DailyTaskPayment
  );

  return (
    <div className="space-y-6">
      <h1 className="text-xl font-semibold">کیف پول جستجوگر شغل</h1>

      {/* Balance Card */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <Card className="sm:col-span-2">
          <CardContent className="p-6">
            <p className="text-sm text-muted-foreground">موجودی قابل برداشت</p>
            {walletLoading ? (
              <Skeleton className="h-10 w-32 mt-2" />
            ) : (
              <p className="text-3xl font-bold mt-2">
                {(wallet?.balance ?? 0).toLocaleString("fa-IR")}
                <span className="mr-2 text-sm font-normal text-muted-foreground">تومان</span>
              </p>
            )}
            <p className="text-xs text-muted-foreground mt-2">
              موجودی از تسک‌های انجام‌شده و واریزی‌های کارفرما تشکیل شده است.
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-6">
            <p className="text-sm text-muted-foreground">کل کسب‌شده</p>
            {walletLoading ? (
              <Skeleton className="h-10 w-24 mt-2" />
            ) : (
              <p className="text-2xl font-bold mt-2">
                {(wallet?.totalDeposited ?? 0).toLocaleString("fa-IR")}
                <span className="mr-1 text-sm font-normal text-muted-foreground">تومان</span>
              </p>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Transaction History */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">تاریخچه‌ی تراکنش‌ها</CardTitle>
        </CardHeader>
        <CardContent>
          {txLoading ? (
            <div className="space-y-2">
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
            </div>
          ) : relevantTransactions.length === 0 ? (
            <EmptyState
              icon={WalletIcon}
              title="تراکنشی نیست"
              description="پس از انجام اولین تسک، تراکنش‌ها اینجا نمایش داده می‌شوند."
            />
          ) : (
            <div className="rounded-md border overflow-hidden">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>مبلغ</TableHead>
                    <TableHead>نوع</TableHead>
                    <TableHead>توضیحات</TableHead>
                    <TableHead>تاریخ</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {relevantTransactions.map((tx) => (
                    <TableRow key={tx.id}>
                      <TableCell className="font-semibold text-green-600">
                        +{tx.amount.toLocaleString("fa-IR")}
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline">{transactionLabels[tx.type] ?? tx.type}</Badge>
                      </TableCell>
                      <TableCell className="text-muted-foreground">
                        {tx.description || "—"}
                      </TableCell>
                      <TableCell className="text-muted-foreground text-sm">
                        {new Date(tx.createdAt).toLocaleDateString("fa-IR")}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}