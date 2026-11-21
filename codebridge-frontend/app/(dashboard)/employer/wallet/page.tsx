"use client";

import { useState } from "react";
import { WalletIcon } from "lucide-react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useAuth } from "@/lib/hooks/useAuth";
import { useMyWallet, useWalletTransactions, useDeposit } from "@/lib/hooks/useWallets";
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

const depositSchema = z.object({
  amount: z.coerce.number().min(10000, "حداقل مبلغ واریز ۱۰,۰۰۰ تومان است"),
  description: z.string().optional(),
});

type DepositFormValues = z.infer<typeof depositSchema>;

const transactionLabels: Record<number, string> = {
  [WalletTransactionType.Deposit]: "واریز",
  [WalletTransactionType.Withdrawal]: "برداشت",
  [WalletTransactionType.ProjectPayment]: "پرداخت پروژه",
  [WalletTransactionType.DailyTaskPayment]: "تسک روزانه",
  [WalletTransactionType.PlatformFee]: "کارمزد",
};

export default function EmployerWalletPage() {
  const { user } = useAuth();
  const { data: wallet, isLoading: walletLoading } = useMyWallet();
  const { data: transactions, isLoading: txLoading } = useWalletTransactions();
  const depositMutation = useDeposit();

  const [dialogOpen, setDialogOpen] = useState(false);

  const form = useForm<DepositFormValues>({
    resolver: zodResolver(depositSchema),
    defaultValues: { amount: 0, description: "" },
  });

  const onDeposit = async (values: DepositFormValues) => {
    try {
      await depositMutation.mutateAsync({
        amount: values.amount,
        description: values.description,
      });
      toast.success("واریز با موفقیت انجام شد.");
      setDialogOpen(false);
      form.reset();
    } catch (err) {
      toast.error(getErrorMessage(err, "خطا در واریز."));
    }
  };

  return (
    <div className="space-y-6">
      <h1 className="text-xl font-semibold">کیف پول کارفرما</h1>

      {/* Balance Cards */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <Card>
          <CardContent className="p-4">
            <p className="text-sm text-muted-foreground">موجودی</p>
            {walletLoading ? (
              <Skeleton className="h-8 w-24 mt-1" />
            ) : (
              <p className="text-2xl font-bold mt-1">
                {wallet?.balance.toLocaleString("fa-IR") ?? 0}
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
                {wallet?.totalDeposited.toLocaleString("fa-IR") ?? 0}
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
                {wallet?.totalWithdrawn.toLocaleString("fa-IR") ?? 0}
                <span className="text-sm font-normal text-muted-foreground mr-1">تومان</span>
              </p>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Deposit Button */}
      <div className="flex justify-end">
        <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
          <DialogTrigger asChild>
            <Button>واریز وجه</Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>واریز وجه</DialogTitle>
            </DialogHeader>
            <Form {...form}>
              <form onSubmit={form.handleSubmit(onDeposit)} className="space-y-4">
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
                      <FormLabel>توضیحات (اختیاری)</FormLabel>
                      <FormControl>
                        <Input placeholder="توضیحات..." {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <Button
                  type="submit"
                  className="w-full"
                  disabled={depositMutation.isPending}
                >
                  {depositMutation.isPending ? "در حال واریز..." : "واریز"}
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
          ) : !transactions || transactions.length === 0 ? (
            <EmptyState
              icon={WalletIcon}
              title="تراکنشی وجود ندارد"
              description="پس از اولین واریز، تراکنش‌ها اینجا نمایش داده می‌شوند."
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
                  {transactions.map((tx) => (
                    <TableRow key={tx.id}>
                      <TableCell className="font-semibold">
                        {tx.amount.toLocaleString("fa-IR")}
                      </TableCell>
                      <TableCell>
                        {transactionLabels[tx.type] ?? tx.type}
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
