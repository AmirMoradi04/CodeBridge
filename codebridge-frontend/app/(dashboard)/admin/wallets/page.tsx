"use client";

import { useState } from "react";
import { WalletIcon } from "lucide-react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useAuth } from "@/lib/hooks/useAuth";
import { useAdminWallet, useWalletTransactions } from "@/lib/hooks/useWallets";
import { walletsApi } from "@/lib/api/wallets";
import { getErrorMessage } from "@/lib/api/axios";
import { WalletTransactionType } from "@/types/wallet";
import { EmptyState } from "@/components/shared/EmptyState";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { toast } from "sonner";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";

const creditSchema = z.object({
  userId: z.coerce.number().int().positive("آیدی کاربر معتبر نیست"),
  amount: z.coerce.number().min(10000, "حداقل مبلغ ۱۰,۰۰۰ تومان است"),
  description: z.string().optional(),
});

type CreditFormValues = z.infer<typeof creditSchema>;

const transactionTypeLabels: Record<number, string> = {
  [WalletTransactionType.Deposit]: "واریز",
  [WalletTransactionType.ProjectPayment]: "پرداخت پروژه",
  [WalletTransactionType.PlatformFee]: "کارمزد",
};

export default function AdminWalletPage() {
  const { user } = useAuth();
  const { data: wallet, isLoading: walletLoading } = useAdminWallet();
  const { data: allTransactions, isLoading: txLoading } = useWalletTransactions();

  const [dialogOpen, setDialogOpen] = useState(false);

  const form = useForm<CreditFormValues>({
    resolver: zodResolver(creditSchema),
    defaultValues: { userId: 0, amount: 0, description: "" },
  });

  const onCredit = async (values: CreditFormValues) => {
    if (!user?.id) return;
    try {
      await walletsApi.creditUser({
        targetUserId: values.userId,
        amount: values.amount,
        description: values.description
      });
      toast.success("کاربر با موفقیت شارژ شد.");
      setDialogOpen(false);
      form.reset();
    } catch (err) {
      toast.error(getErrorMessage(err, "شارژ کاربر ناموفق بود."));
    }
  };

  const filteredTransactions = allTransactions?.filter(
    (tx) =>
      tx.type === WalletTransactionType.Deposit ||
      tx.type === WalletTransactionType.ProjectPayment
  );

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-lg font-semibold">کیف پول ادمین</h2>
        <p className="text-sm text-muted-foreground">
          مدیریت موجودی و شارژ کاربران
        </p>
      </div>

      {/* Balance Cards */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <Card>
          <CardContent className="p-4">
            <p className="text-sm text-muted-foreground">موجودی</p>
            {walletLoading ? (
              <Skeleton className="h-8 w-24 mt-1" />
            ) : (
              <p className="text-2xl font-bold mt-1">
                {(wallet?.balance ?? 0).toLocaleString("fa-IR")}
                <span className="text-sm font-normal text-muted-foreground mr-1">تومان</span>
              </p>
            )}
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <p className="text-sm text-muted-foreground">کل واریزی</p>
            {walletLoading ? (
              <Skeleton className="h-8 w-24 mt-1" />
            ) : (
              <p className="text-2xl font-bold mt-1">
                {(wallet?.totalDeposited ?? 0).toLocaleString("fa-IR")}
                <span className="text-sm font-normal text-muted-foreground mr-1">تومان</span>
              </p>
            )}
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <p className="text-sm text-muted-foreground">کل برداشتی</p>
            {walletLoading ? (
              <Skeleton className="h-8 w-24 mt-1" />
            ) : (
              <p className="text-2xl font-bold mt-1">
                {(wallet?.totalWithdrawn ?? 0).toLocaleString("fa-IR")}
                <span className="text-sm font-normal text-muted-foreground mr-1">تومان</span>
              </p>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Credit User Button */}
      <div className="flex justify-end">
        <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
          <DialogTrigger asChild>
            <Button>شارژ کاربر</Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>شارژ کاربر</DialogTitle>
            </DialogHeader>
            <Form {...form}>
              <form onSubmit={form.handleSubmit(onCredit)} className="space-y-4">
                <FormField
                  control={form.control}
                  name="userId"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>آیدی کاربر</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          min={1}
                          placeholder="مثال: ۱۲"
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="amount"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>مبلغ (تومان)</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          min={10000}
                          placeholder="مثال: ۵۰۰۰۰۰"
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="description"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>توضیحات</FormLabel>
                      <FormControl>
                        <Input placeholder="توضیحات (اختیاری)..." {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <Button
                  type="submit"
                  className="w-full"
                  disabled={form.formState.isSubmitting}
                >
                  {form.formState.isSubmitting ? "در حال شارژ..." : "شارژ"}
                </Button>
              </form>
            </Form>
          </DialogContent>
        </Dialog>
      </div>

      {/* Transaction History */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">تاریخچه تراکنش‌ها</CardTitle>
        </CardHeader>
        <CardContent>
          {txLoading ? (
            <div className="space-y-2">
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
            </div>
          ) : !filteredTransactions || filteredTransactions.length === 0 ? (
            <EmptyState
              icon={WalletIcon}
              title="تراکنشی وجود ندارد"
              description="تراکنش‌های واریز و پرداخت پروژه اینجا نمایش داده می‌شوند."
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
                  {filteredTransactions.map((tx) => (
                    <TableRow key={tx.id}>
                      <TableCell className="font-semibold">
                        {tx.amount.toLocaleString("fa-IR")}
                      </TableCell>
                      <TableCell>
                        {transactionTypeLabels[tx.type] ?? tx.type}
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
