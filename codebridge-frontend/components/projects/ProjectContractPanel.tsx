"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { getErrorMessage } from "@/lib/api/axios";
import { useCreateContract, useProjectContract, useSignContract, useSignNda } from "@/lib/hooks/useContracts";
import { useDepositProject, useFinalApproveProject } from "@/lib/hooks/useProjects";
import { useMyWallet } from "@/lib/hooks/useWallets";
import { ContractStatus } from "@/types/contract";
import { ProjectStatus, type Project } from "@/types/project";
import { Role } from "@/types/auth";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";

const contractSchema = z.object({
  totalAmount: z.number().positive("مبلغ قرارداد باید بیشتر از صفر باشد."),
  platformFee: z.number().nonnegative("سهم پلتفرم نمی‌تواند منفی باشد."),
  developerShare: z.number().nonnegative("سهم برنامه‌نویس نمی‌تواند منفی باشد.")
}).refine((values) => values.totalAmount === values.platformFee + values.developerShare, {
  message: "مجموع سهم‌ها باید با مبلغ کل برابر باشد.",
  path: ["developerShare"]
});

const depositSchema = z.object({
  amount: z.number().positive("مبلغ واریز باید بیشتر از صفر باشد.")
});

type ContractValues = z.infer<typeof contractSchema>;
type DepositValues = z.infer<typeof depositSchema>;

const contractLabels: Record<number, string> = {
  [ContractStatus.Draft]: "پیش‌نویس",
  [ContractStatus.Signed]: "امضاشده",
  [ContractStatus.Active]: "فعال",
  [ContractStatus.Completed]: "تکمیل‌شده",
  [ContractStatus.Cancelled]: "لغوشده"
};

function toman(value: number) {
  return `${value.toLocaleString("fa-IR")} تومان`;
}

export function ProjectContractPanel({
  project,
  role,
  embedded = false
}: {
  project: Project;
  role: Role;
  embedded?: boolean;
}) {
  const { data: contract, isLoading, isError } = useProjectContract(project.id);
  const wallet = useMyWallet();
  const createContract = useCreateContract();
  const signContract = useSignContract(project.id);
  const signNda = useSignNda(project.id);
  const deposit = useDepositProject();
  const finalApprove = useFinalApproveProject();
  const remaining = contract ? Math.max(0, contract.totalAmount - contract.depositPaid) : 0;

  const contractForm = useForm<ContractValues>({
    resolver: zodResolver(contractSchema),
    defaultValues: { totalAmount: project.budget, platformFee: 0, developerShare: project.budget }
  });
  const depositForm = useForm<DepositValues>({
    resolver: zodResolver(depositSchema),
    defaultValues: { amount: 0 }
  });

  // مقدار پیش‌فرض واریز پس از دریافت قرارداد تنظیم می‌شود
  useEffect(() => {
    if (remaining > 0) depositForm.setValue("amount", remaining);
  }, [remaining, depositForm]);

  async function onCreate(values: ContractValues) {
    try {
      await createContract.mutateAsync({
        projectId: project.id,
        totalAmount: values.totalAmount,
        platformFee: values.platformFee,
        developerShare: values.developerShare
      });
      toast.success("قرارداد پروژه ساخته شد.");
    } catch (error) {
      toast.error(getErrorMessage(error, "ساخت قرارداد انجام نشد."));
    }
  }

  async function onDeposit(values: DepositValues) {
    try {
      await deposit.mutateAsync({ projectId: project.id, amount: values.amount });
      toast.success("مبلغ به حساب امانی واریز شد.");
    } catch (error) {
      toast.error(getErrorMessage(error, "واریز به حساب امانی انجام نشد."));
    }
  }

  async function runAction(action: () => Promise<unknown>, success: string, fallback: string) {
    try {
      await action();
      toast.success(success);
    } catch (error) {
      toast.error(getErrorMessage(error, fallback));
    }
  }

  const body = (
    <div className={embedded ? "space-y-4 border-t pt-4" : "space-y-4"}>
        {isError ? (
          <p className="text-sm text-destructive">دریافت وضعیت قرارداد ممکن نشد؛ صفحه را تازه‌سازی کنید.</p>
        ) : contract ? (
          <>
            <div className="flex flex-wrap items-center justify-between gap-2">
              <Badge variant="outline">وضعیت: {contractLabels[contract.status] ?? "نامشخص"}</Badge>
              <span className="text-xs text-muted-foreground" dir="ltr">قرارداد #{contract.id}</span>
            </div>
            <dl className="grid gap-3 rounded-lg border p-3 text-sm sm:grid-cols-2">
              <div><dt className="text-muted-foreground">مبلغ کل</dt><dd className="font-medium">{toman(contract.totalAmount)}</dd></div>
              <div><dt className="text-muted-foreground">واریزشده به امانی</dt><dd className="font-medium">{toman(contract.depositPaid)}</dd></div>
              <div><dt className="text-muted-foreground">مانده‌ی پرداخت</dt><dd className="font-medium">{toman(remaining)}</dd></div>
              <div><dt className="text-muted-foreground">NDA</dt><dd className="font-medium">{contract.isNdaSigned ? "امضاشده" : "امضا نشده"}</dd></div>
            </dl>

            {role === Role.Employer && contract.status === ContractStatus.Draft && (
              <Button type="button" disabled={signContract.isPending} onClick={() => void runAction(() => signContract.mutateAsync(contract.id), "قرارداد امضا شد.", "امضای قرارداد انجام نشد.")}>
                تایید و امضای قرارداد
              </Button>
            )}

            {role === Role.Employer &&
              remaining > 0 &&
              (contract.status === ContractStatus.Signed || contract.status === ContractStatus.Active) && (
              <form onSubmit={depositForm.handleSubmit(onDeposit)} className="space-y-3 rounded-lg border p-3">
                <p className="text-sm font-medium">واریز از موجودی کیف پول به حساب امانی</p>
                <p className="text-xs text-muted-foreground">موجودی کیف پول: {wallet.data ? toman(wallet.data.balance) : wallet.isLoading ? "در حال بارگذاری…" : "دریافت نشد"}</p>
                <label className="block space-y-1 text-sm" htmlFor={`deposit-${project.id}`}>
                  مبلغ واریز (حداکثر {toman(remaining)})
                  <Input id={`deposit-${project.id}`} type="number" min="1" max={remaining} step="0.01" {...depositForm.register("amount", { valueAsNumber: true })} />
                </label>
                {depositForm.formState.errors.amount && <p className="text-xs text-destructive">{depositForm.formState.errors.amount.message}</p>}
                <Button type="submit" disabled={deposit.isPending || !wallet.data || wallet.data.balance < 1}>
                  واریز به حساب امانی
                </Button>
              </form>
            )}

            {role === Role.JobSeeker && !contract.isNdaSigned && (
              <Button type="button" variant="outline" disabled={signNda.isPending} onClick={() => void runAction(() => signNda.mutateAsync(contract.id), "توافق‌نامه‌ی NDA امضا شد.", "امضای NDA انجام نشد.")}>
                امضای توافق‌نامه‌ی NDA
              </Button>
            )}

            {role === Role.Admin && project.status === ProjectStatus.InProgress && !project.isFinalApproved && (
              <Button type="button" disabled={!contract.isNdaSigned || !contract.isFullyPaid || finalApprove.isPending} onClick={() => void runAction(() => finalApprove.mutateAsync(project.id), "تسویه نهایی انجام و پروژه تایید شد.", "تایید نهایی انجام نشد.")}>
                تایید نهایی و پرداخت از امانی
              </Button>
            )}

            {project.isFinalApproved && <p className="text-sm font-medium text-emerald-600">تسویه نهایی پروژه انجام شده است.</p>}
          </>
        ) : role === Role.Admin ? (
          <form onSubmit={contractForm.handleSubmit(onCreate)} className="space-y-3">
            <p className="text-sm text-muted-foreground">برای پروژه‌ی تخصیص‌یافته، قرارداد را با مبلغ و سهم‌های دقیق بسازید.</p>
            <label className="block space-y-1 text-sm" htmlFor={`contract-total-${project.id}`}>مبلغ کل قرارداد
              <Input id={`contract-total-${project.id}`} type="number" min="0.01" step="0.01" {...contractForm.register("totalAmount", { valueAsNumber: true })} />
            </label>
            {contractForm.formState.errors.totalAmount && <p className="text-xs text-destructive">{contractForm.formState.errors.totalAmount.message}</p>}
            <label className="block space-y-1 text-sm" htmlFor={`contract-fee-${project.id}`}>سهم پلتفرم
              <Input id={`contract-fee-${project.id}`} type="number" min="0" step="0.01" {...contractForm.register("platformFee", { valueAsNumber: true })} />
            </label>
            {contractForm.formState.errors.platformFee && <p className="text-xs text-destructive">{contractForm.formState.errors.platformFee.message}</p>}
            <label className="block space-y-1 text-sm" htmlFor={`contract-dev-${project.id}`}>سهم برنامه‌نویس
              <Input id={`contract-dev-${project.id}`} type="number" min="0" step="0.01" {...contractForm.register("developerShare", { valueAsNumber: true })} />
            </label>
            {contractForm.formState.errors.developerShare && <p className="text-xs text-destructive">{contractForm.formState.errors.developerShare.message}</p>}
            <Button type="submit" disabled={createContract.isPending || !project.assignedDeveloperId}>ساخت قرارداد</Button>
            {!project.assignedDeveloperId && <p className="text-xs text-muted-foreground">ابتدا یک برنامه‌نویس به پروژه تخصیص دهید.</p>}
          </form>
        ) : (
          <p className="text-sm text-muted-foreground">هنوز قراردادی برای این پروژه ثبت نشده است.</p>
        )}
    </div>
  );

  if (embedded) {
    return (
      <div className="w-full space-y-3">
        <p className="text-sm font-semibold">قرارداد و تسویه</p>
        {isLoading ? (
          <p className="text-sm text-muted-foreground">در حال دریافت اطلاعات قرارداد…</p>
        ) : (
          body
        )}
      </div>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">قرارداد و تسویه‌ی پروژه</CardTitle>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <p className="text-sm text-muted-foreground">در حال دریافت اطلاعات قرارداد…</p>
        ) : (
          body
        )}
      </CardContent>
    </Card>
  );
}
