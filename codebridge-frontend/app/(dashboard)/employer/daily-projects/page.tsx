"use client";

import { useState } from "react";
import Link from "next/link";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  ArrowUpRight,
  Banknote,
  Briefcase,
  Coins,
  Plus,
  Users,
} from "lucide-react";
import { DailyProjectStatus } from "@/types/daily-project";
import { useAuth } from "@/lib/hooks/useAuth";
import { useEmployerDailyProjects, dailyProjectKeys } from "@/lib/hooks/useDailyProjects";
import { dailyProjectsApi } from "@/lib/api/daily-projects";
import { getErrorMessage } from "@/lib/api/axios";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Badge } from "@/components/ui/badge";
import { EmptyState } from "@/components/shared/EmptyState";

const DAILY_STATUS_LABELS: Record<number, string> = {
  [DailyProjectStatus.Pending]: "در انتظار تایید",
  [DailyProjectStatus.Approved]: "تاییدشده",
  [DailyProjectStatus.Active]: "فعال",
  [DailyProjectStatus.Completed]: "تکمیل‌شده",
};

const DAILY_STATUS_CLASSES: Record<number, string> = {
  [DailyProjectStatus.Pending]: "bg-amber-100 text-amber-800 dark:bg-amber-500/15 dark:text-amber-300",
  [DailyProjectStatus.Approved]: "bg-sky-100 text-sky-800 dark:bg-sky-500/15 dark:text-sky-300",
  [DailyProjectStatus.Active]: "bg-emerald-100 text-emerald-800 dark:bg-emerald-500/15 dark:text-emerald-300",
  [DailyProjectStatus.Completed]: "bg-gray-100 text-gray-600 dark:bg-gray-500/15 dark:text-gray-400",
};

export default function EmployerDailyProjectsPage() {
  const { user } = useAuth();
  const qc = useQueryClient();
  const { data: projects, isLoading } = useEmployerDailyProjects(user?.id ?? 0);

  const depositMutation = useMutation({
    mutationFn: (projectId: number) => dailyProjectsApi.depositFunds(projectId),
    onSuccess: () => {
      toast.success("بودجه با موفقیت واریز شد.");
      void qc.invalidateQueries({ queryKey: dailyProjectKeys.all });
    },
    onError: (error) => {
      toast.error(getErrorMessage(error, "واریز بودجه ناموفق بود."));
    },
  });

  function handleDeposit(projectId: number) {
    depositMutation.mutate(projectId);
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold">پروژه‌های روزانه‌ی من</h2>
          <p className="text-sm text-muted-foreground">
            لیست پروژه‌های روزانه و وضعیت آنها
          </p>
        </div>

        <div className="flex gap-2">
          <Button asChild>
            <Link href="/employer/daily-projects/new">
              <Plus className="h-4 w-4" />
              ثبت پروژه‌ی روزانه‌ی جدید
            </Link>
          </Button>
        </div>
      </div>

      {isLoading ? (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          <Skeleton className="h-56 w-full" />
          <Skeleton className="h-56 w-full" />
        </div>
      ) : !projects || projects.length === 0 ? (
        <EmptyState
          icon={Briefcase}
          title="هنوز پروژه‌ی روزانه‌ای ثبت نکرده‌اید"
          description="اولین پروژه‌ی روزانه‌ی خود را بسازید تا پس از تایید ادمین فعال شود."
          action={
            <Button asChild>
              <Link href="/employer/daily-projects/new">ثبت پروژه</Link>
            </Button>
          }
        />
      ) : (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          {projects.map((project) => (
            <DailyProjectCard
              key={project.id}
              project={project}
              onDeposit={() => handleDeposit(project.id)}
              isDepositLoading={depositMutation.isPending}
            />
          ))}
        </div>
      )}
    </div>
  );
}

function DailyProjectCard({
  project,
  onDeposit,
  isDepositLoading,
}: {
  project: import("@/types/daily-project").DailyProjectDto;
  onDeposit: () => void;
  isDepositLoading: boolean;
}) {
  const canDeposit =
    project.status === DailyProjectStatus.Approved && !project.isPaid;

  return (
    <Card className="flex h-full flex-col">
      <CardHeader className="gap-3">
        <div className="flex items-start justify-between gap-3">
          <CardTitle className="text-base leading-6">{project.title}</CardTitle>
          <Badge className={DAILY_STATUS_CLASSES[project.status] ?? ""}>
            {DAILY_STATUS_LABELS[project.status] ?? "نامشخص"}
          </Badge>
        </div>

        <div className="flex flex-wrap items-center gap-3 text-xs text-muted-foreground">
          <span className="inline-flex items-center gap-1">
            <Coins className="h-3.5 w-3.5" />
            {project.totalBudget.toLocaleString("fa-IR")} تومان
          </span>
          <span className="inline-flex items-center gap-1">
            <ArrowUpRight className="h-3.5 w-3.5" />
            {project.pricePerUnit.toLocaleString("fa-IR")} / واحد
          </span>
          <span className="inline-flex items-center gap-1">
            <Users className="h-3.5 w-3.5" />
            {project.filledSlots}/{project.capacity}
          </span>
        </div>
      </CardHeader>

      <CardContent className="flex flex-1 flex-col gap-4">
        {project.description && (
          <p className="line-clamp-3 text-sm text-muted-foreground">
            {project.description}
          </p>
        )}

        <div className="space-y-1 text-xs">
          {project.employerName && (
            <p className="text-muted-foreground">کارفرما: {project.employerName}</p>
          )}
          <p className="text-muted-foreground">
            وضعیت پرداخت:{" "}
            {project.isPaid ? (
              <span className="text-emerald-600 dark:text-emerald-400">پرداخت شده</span>
            ) : (
              <span className="text-amber-600 dark:text-amber-400">پرداخت نشده</span>
            )}
          </p>
          {project.adminNote && (
            <p className="text-muted-foreground">یادداشت ادمین: {project.adminNote}</p>
          )}
        </div>

        <div className="mt-auto flex flex-wrap items-center gap-2">
          {canDeposit && (
            <Button
              size="sm"
              onClick={onDeposit}
              disabled={isDepositLoading}
            >
              <Banknote className="h-4 w-4" />
              واریز بودجه
            </Button>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
