"use client";

import Link from "next/link";
import {
  Briefcase,
  CalendarClock,
  FileText,
  MessageSquare,
  ScrollText,
  Users
} from "lucide-react";
import { useAdminProjects, usePendingProjects } from "@/lib/hooks/useProjects";
import { usePendingUsers, useUsers } from "@/lib/hooks/useUsers";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

function QuickLink({
  href,
  icon: Icon,
  title,
  description
}: {
  href: string;
  icon: React.ElementType;
  title: string;
  description: string;
}) {
  return (
    <Link
      href={href}
      className="group flex items-center gap-3 rounded-xl border p-4 transition-colors hover:border-primary/50 hover:bg-accent"
    >
      <span className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10 text-primary">
        <Icon className="h-5 w-5" />
      </span>
      <span>
        <span className="block text-sm font-semibold group-hover:text-primary">
          {title}
        </span>
        <span className="block text-xs text-muted-foreground">{description}</span>
      </span>
    </Link>
  );
}

function StatCard({
  label,
  value,
  loading
}: {
  label: string;
  value: number;
  loading?: boolean;
}) {
  return (
    <Card>
      <CardContent className="p-4">
        {loading ? (
          <Skeleton className="h-10 w-20" />
        ) : (
          <p className="text-2xl font-bold">{value.toLocaleString("fa-IR")}</p>
        )}
        <p className="mt-1 text-xs text-muted-foreground">{label}</p>
      </CardContent>
    </Card>
  );
}

export default function AdminDashboardPage() {
  const users = useUsers();
  const pendingUsers = usePendingUsers();
  const projects = useAdminProjects();
  const pendingProjects = usePendingProjects();

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-lg font-semibold">داشبورد مدیریت</h2>
        <p className="text-sm text-muted-foreground">
          نمای کلی کاربران، پروژه‌ها و درخواست‌های در انتظار
        </p>
      </div>

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard label="کل کاربران" value={users.data?.length ?? 0} loading={users.isLoading} />
        <StatCard
          label="در انتظار تایید"
          value={pendingUsers.data?.length ?? 0}
          loading={pendingUsers.isLoading}
        />
        <StatCard
          label="کل پروژه‌ها"
          value={projects.data?.length ?? 0}
          loading={projects.isLoading}
        />
        <StatCard
          label="پروژه‌های در انتظار"
          value={pendingProjects.data?.length ?? 0}
          loading={pendingProjects.isLoading}
        />
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <QuickLink
          href="/admin/users"
          icon={Users}
          title="مدیریت کاربران"
          description="تایید یا رد حساب کاربران"
        />
        <QuickLink
          href="/admin/projects"
          icon={Briefcase}
          title="مدیریت پروژه‌ها"
          description="تایید، رد و انتشار پروژه‌ها"
        />
        <QuickLink
          href="/admin/resumes"
          icon={FileText}
          title="بررسی رزومه‌ها"
          description="رزومه‌های در انتظار بررسی"
        />
        <QuickLink
          href="/admin/chat"
          icon={MessageSquare}
          title="گفت‌وگوها"
          description="شروع و ادامه‌ی گفت‌وگو با کاربران"
        />
        <QuickLink
          href="/admin/daily-projects"
          icon={CalendarClock}
          title="پروژه‌های روزانه"
          description="تایید، رد و فعال‌سازی پروژه‌های روزانه"
        />
        <QuickLink
          href="/admin/audit-logs"
          icon={ScrollText}
          title="گزارش ممیزی"
          description="رویدادهای ثبت‌شده‌ی سیستم"
        />
      </div>
    </div>
  );
}