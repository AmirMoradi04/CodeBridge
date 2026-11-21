"use client";

import { format } from "date-fns";
import { faIR } from "date-fns/locale";
import { Video } from "lucide-react";
import { useAdminLiveSessions } from "@/lib/hooks/useLiveSessions";
import { LiveSessionStatus } from "@/types/live-session";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { EmptyState } from "@/components/shared/EmptyState";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from "@/components/ui/table";

const STATUS_LABELS: Record<LiveSessionStatus, string> = {
  [LiveSessionStatus.Scheduled]: "زمان‌بندی‌شده",
  [LiveSessionStatus.Live]: "در حال برگزاری",
  [LiveSessionStatus.Ended]: "پایان‌یافته",
  [LiveSessionStatus.Cancelled]: "لغوشده"
};

const STATUS_CLASSES: Record<LiveSessionStatus, string> = {
  [LiveSessionStatus.Scheduled]: "bg-sky-100 text-sky-800 dark:bg-sky-500/15 dark:text-sky-300",
  [LiveSessionStatus.Live]: "bg-emerald-100 text-emerald-800 dark:bg-emerald-500/15 dark:text-emerald-300",
  [LiveSessionStatus.Ended]: "bg-muted text-muted-foreground",
  [LiveSessionStatus.Cancelled]: "bg-rose-100 text-rose-800 dark:bg-rose-500/15 dark:text-rose-300"
};

export default function AdminLiveSessionsPage() {
  const { data: sessions, isLoading } = useAdminLiveSessions();

  const totalRevenue = (sessions ?? []).reduce(
    (sum, session) => sum + session.totalRevenue,
    0
  );

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-xl font-semibold">پایش جلسه‌های زنده</h1>
        <p className="text-sm text-muted-foreground">
          فهرست جلسه‌های برگزارشده و درآمد ناخالص هر جلسه
        </p>
      </div>

      {isLoading ? (
        <div className="space-y-2">
          <Skeleton className="h-12 w-full" />
          <Skeleton className="h-12 w-full" />
          <Skeleton className="h-12 w-full" />
        </div>
      ) : !sessions || sessions.length === 0 ? (
        <EmptyState
          icon={Video}
          title="جلسه‌ای ثبت نشده است"
          description="جلسه‌های ساخته‌شده توسط برنامه‌نویسان ارشد اینجا نمایش داده می‌شوند."
        />
      ) : (
        <>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div className="rounded-xl border p-4">
              <p className="text-xs text-muted-foreground">تعداد جلسه‌ها</p>
              <p className="text-2xl font-semibold">
                {sessions.length.toLocaleString("fa-IR")}
              </p>
            </div>
            <div className="rounded-xl border p-4">
              <p className="text-xs text-muted-foreground">
                درآمد ناخالص کل (تومان)
              </p>
              <p className="text-2xl font-semibold">
                {totalRevenue.toLocaleString("fa-IR")}
              </p>
            </div>
          </div>

          <div className="overflow-hidden rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>#</TableHead>
                  <TableHead>عنوان</TableHead>
                  <TableHead>برگزارکننده</TableHead>
                  <TableHead>پروژه</TableHead>
                  <TableHead>زمان</TableHead>
                  <TableHead>قیمت</TableHead>
                  <TableHead>شرکت‌کننده</TableHead>
                  <TableHead>درآمد جلسه</TableHead>
                  <TableHead>وضعیت</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {sessions.map((session) => (
                  <TableRow key={session.id}>
                    <TableCell>{session.id}</TableCell>
                    <TableCell className="font-medium">{session.title}</TableCell>
                    <TableCell className="text-muted-foreground">
                      {session.seniorName ?? `#${session.seniorId}`}
                    </TableCell>
                    <TableCell className="text-muted-foreground">
                      {session.projectTitle ?? `#${session.projectId}`}
                    </TableCell>
                    <TableCell className="text-muted-foreground">
                      {format(new Date(session.scheduledAt), "yyyy/MM/dd HH:mm", {
                        locale: faIR
                      })}
                    </TableCell>
                    <TableCell>{session.price.toLocaleString("fa-IR")}</TableCell>
                    <TableCell>{session.participantCount}</TableCell>
                    <TableCell>
                      {session.totalRevenue.toLocaleString("fa-IR")}
                    </TableCell>
                    <TableCell>
                      <Badge className={STATUS_CLASSES[session.status]}>
                        {STATUS_LABELS[session.status]}
                      </Badge>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </>
      )}
    </div>
  );
}
