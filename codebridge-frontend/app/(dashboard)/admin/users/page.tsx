"use client";

import { useState } from "react";
import type { ColumnDef } from "@tanstack/react-table";
import { Check, Users, X } from "lucide-react";
import { toast } from "sonner";
import { getErrorMessage } from "@/lib/api/axios";
import { useApproveUser, useRejectUser, useUsers } from "@/lib/hooks/useUsers";
import { ROLE_BADGE_CLASSES, ROLE_LABELS } from "@/lib/constants";
import { Role } from "@/types/auth";
import type { User } from "@/types/user";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from "@/components/ui/select";
import { DataTable } from "@/components/shared/DataTable";
import { EmptyState } from "@/components/shared/EmptyState";

export default function AdminUsersPage() {
  const [roleFilter, setRoleFilter] = useState<string>("all");
  const usersQuery = useUsers(roleFilter === "all" ? undefined : (Number(roleFilter) as Role));
  const approveUser = useApproveUser();
  const rejectUser = useRejectUser();

  async function handleApprove(userId: number) {
    try {
      await approveUser.mutateAsync(userId);
      toast.success("حساب کاربر تایید شد.");
    } catch (error) {
      toast.error(getErrorMessage(error, "تایید حساب ناموفق بود."));
    }
  }

  async function handleReject(userId: number) {
    try {
      await rejectUser.mutateAsync(userId);
      toast.success("حساب کاربر رد شد.");
    } catch (error) {
      toast.error(getErrorMessage(error, "رد حساب ناموفق بود."));
    }
  }

  const columns: ColumnDef<User>[] = [
    {
      accessorKey: "firstName",
      header: "نام",
      cell: ({ row }) => (
        <div className="min-w-0">
          <p className="truncate font-medium">
            {row.original.firstName} {row.original.lastName}
          </p>
          <p className="truncate text-xs text-muted-foreground" dir="ltr">
            {row.original.email}
          </p>
        </div>
      )
    },
    {
      accessorKey: "userRole",
      header: "نقش",
      cell: ({ row }) => (
        <Badge className={ROLE_BADGE_CLASSES[row.original.userRole]}>
          {ROLE_LABELS[row.original.userRole]}
        </Badge>
      )
    },
    {
      accessorKey: "isApproved",
      header: "وضعیت",
      cell: ({ row }) =>
        row.original.isApproved ? (
          <Badge variant="secondary">تاییدشده</Badge>
        ) : (
          <Badge variant="outline" className="text-amber-600">
            در انتظار تایید
          </Badge>
        )
    },
    {
      id: "actions",
      header: "عملیات",
      cell: ({ row }) => (
        <div className="flex items-center gap-2">
          <Button
            size="sm"
            variant="outline"
            disabled={row.original.isApproved || approveUser.isPending}
            onClick={() => handleApprove(row.original.id)}
          >
            <Check className="h-4 w-4" />
            تایید
          </Button>
          <Button
            size="sm"
            variant="destructive"
            disabled={!row.original.isApproved || rejectUser.isPending}
            onClick={() => handleReject(row.original.id)}
          >
            <X className="h-4 w-4" />
            رد
          </Button>
        </div>
      )
    }
  ];

  const data = usersQuery.data ?? [];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold">مدیریت کاربران</h2>
          <p className="text-sm text-muted-foreground">
            تایید یا رد حساب کاربران و فیلتر بر اساس نقش
          </p>
        </div>

        <Select value={roleFilter} onValueChange={setRoleFilter}>
          <SelectTrigger className="w-48">
            <SelectValue placeholder="فیلتر نقش" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">همه‌ی نقش‌ها</SelectItem>
            <SelectItem value={String(Role.Admin)}>ادمین</SelectItem>
            <SelectItem value={String(Role.JobSeeker)}>کارجو (برنامه‌نویس)</SelectItem>
            <SelectItem value={String(Role.Employer)}>کارفرما</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {usersQuery.isLoading ? (
        <p className="text-sm text-muted-foreground">در حال بارگذاری...</p>
      ) : data.length === 0 ? (
        <EmptyState icon={Users} title="کاربری با این نقش پیدا نشد" />
      ) : (
        <DataTable columns={columns} data={data} emptyMessage="کاربری وجود ندارد." />
      )}
    </div>
  );
}