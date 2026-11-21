"use client";

import { useState } from "react";
import type { ColumnDef } from "@tanstack/react-table";
import { ScrollText } from "lucide-react";
import { useRecentAuditLogs } from "@/lib/hooks/useAuditLogs";
import type { AuditLog } from "@/types/audit";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from "@/components/ui/select";
import { DataTable } from "@/components/shared/DataTable";
import { EmptyState } from "@/components/shared/EmptyState";

const COUNT_OPTIONS = [
  { value: 50, label: "۵۰ مورد اخیر" },
  { value: 100, label: "۱۰۰ مورد اخیر" },
  { value: 200, label: "۲۰۰ مورد اخیر" }
];

export default function AdminAuditLogsPage() {
  const [count, setCount] = useState(100);
  const { data: logs, isLoading } = useRecentAuditLogs(count);

  const columns: ColumnDef<AuditLog>[] = [
    {
      accessorKey: "action",
      header: "رویداد",
      cell: ({ row }) => (
        <Badge variant="secondary" dir="ltr">
          {row.original.action}
        </Badge>
      )
    },
    {
      accessorKey: "entityType",
      header: "موجودیت",
      cell: ({ row }) => (
        <span className="text-sm">
          {row.original.entityType}
          {row.original.entityId ? ` #${row.original.entityId}` : ""}
        </span>
      )
    },
    {
      accessorKey: "userId",
      header: "کاربر",
      cell: ({ row }) => (
        <span className="text-sm">{row.original.userId ?? "—"}</span>
      )
    },
    {
      accessorKey: "details",
      header: "جزئیات",
      cell: ({ row }) => (
        <span className="block max-w-md truncate text-xs text-muted-foreground">
          {row.original.details ?? "—"}
        </span>
      )
    },
    {
      accessorKey: "createdAt",
      header: "زمان",
      cell: ({ row }) => (
        <span className="whitespace-nowrap text-xs" dir="ltr">
          {new Date(row.original.createdAt).toLocaleString("fa-IR")}
        </span>
      )
    }
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold">گزارش ممیزی</h2>
          <p className="text-sm text-muted-foreground">
            رویدادهای ثبت‌شده‌ی سیستم (ورود، تاییدها، تغییرات)
          </p>
        </div>

        <Select
          value={String(count)}
          onValueChange={(value) => setCount(Number(value))}
        >
          <SelectTrigger className="w-44">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {COUNT_OPTIONS.map((option) => (
              <SelectItem key={option.value} value={String(option.value)}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {isLoading ? (
        <p className="text-sm text-muted-foreground">در حال بارگذاری...</p>
      ) : !logs || logs.length === 0 ? (
        <EmptyState
          icon={ScrollText}
          title="رویدادی ثبت نشده"
          description="با انجام عملیات، رویدادها اینجا ظاهر می‌شوند."
        />
      ) : (
        <DataTable columns={columns} data={logs} emptyMessage="رویدادی وجود ندارد." />
      )}
    </div>
  );
}