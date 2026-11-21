"use client";

import { formatDistanceToNow } from "date-fns";
import { faIR } from "date-fns/locale";
import {
  BadgeDollarSign,
  Briefcase,
  Sparkles,
  UserPlus
} from "lucide-react";
import type { LucideIcon } from "lucide-react";
import { useActivityFeed } from "@/lib/hooks/useActivity";
import type { ActivityType } from "@/types/activity";
import { Skeleton } from "@/components/ui/skeleton";

const ICONS: Record<ActivityType, LucideIcon> = {
  user: UserPlus,
  project: Briefcase,
  review: Sparkles,
  deposit: BadgeDollarSign
};

const COLORS: Record<ActivityType, string> = {
  user: "bg-sky-100 text-sky-700 dark:bg-sky-500/15 dark:text-sky-300",
  project: "bg-indigo-100 text-indigo-700 dark:bg-indigo-500/15 dark:text-indigo-300",
  review: "bg-amber-100 text-amber-700 dark:bg-amber-500/15 dark:text-amber-300",
  deposit: "bg-emerald-100 text-emerald-700 dark:bg-emerald-500/15 dark:text-emerald-300"
};

/** فید فعالیت زنده‌ی صفحه‌ی اصلی؛ هر ۱۰ ثانیه از سرور تازه می‌شود. */
export function LiveActivityFeed({ count = 8 }: { count?: number }) {
  const { data, isLoading, isError } = useActivityFeed(count);
  const items = data ?? [];

  return (
    <section className="rounded-2xl border bg-card p-4">
      <div className="mb-3 flex items-center justify-between gap-2">
        <h2 className="text-sm font-semibold">فعالیت‌های زنده</h2>
        <span className="flex items-center gap-1.5 text-xs text-muted-foreground">
          <span className="relative flex h-2 w-2">
            <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-emerald-400 opacity-75" />
            <span className="relative inline-flex h-2 w-2 rounded-full bg-emerald-500" />
          </span>
          به‌روز
        </span>
      </div>

      {isLoading ? (
        <div className="space-y-3">
          {Array.from({ length: 4 }).map((_, index) => (
            <div key={index} className="flex items-center gap-3">
              <Skeleton className="h-8 w-8 rounded-full" />
              <div className="flex-1 space-y-1.5">
                <Skeleton className="h-3 w-24" />
                <Skeleton className="h-3 w-40" />
              </div>
            </div>
          ))}
        </div>
      ) : isError ? (
        <p className="text-xs text-muted-foreground">
          دریافت فید فعالیت ممکن نشد؛ کمی بعد دوباره تلاش می‌کنیم.
        </p>
      ) : items.length === 0 ? (
        <p className="text-xs text-muted-foreground">هنوز فعالیتی ثبت نشده است.</p>
      ) : (
        <ul className="space-y-3">
          {items.map((item, index) => {
            const Icon = ICONS[item.type] ?? Sparkles;

            return (
              <li key={`${item.type}-${item.createdAt}-${index}`} className="flex items-start gap-3">
                <span
                  className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-full ${
                    COLORS[item.type] ?? "bg-muted text-muted-foreground"
                  }`}
                >
                  <Icon className="h-4 w-4" />
                </span>

                <div className="min-w-0">
                  <p className="truncate text-xs font-medium">{item.title}</p>
                  {item.description && (
                    <p className="truncate text-xs text-muted-foreground">{item.description}</p>
                  )}
                  <p className="mt-0.5 text-[11px] text-muted-foreground/80" dir="ltr">
                    {formatDistanceToNow(new Date(item.createdAt), {
                      addSuffix: true,
                      locale: faIR
                    })}
                  </p>
                </div>
              </li>
            );
          })}
        </ul>
      )}
    </section>
  );
}
