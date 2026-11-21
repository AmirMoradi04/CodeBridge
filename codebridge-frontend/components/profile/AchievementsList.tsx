"use client";

import { format } from "date-fns";
import { faIR } from "date-fns/locale";
import { Award, Rocket, Star, Trophy } from "lucide-react";
import type { LucideIcon } from "lucide-react";
import { useUserAchievements } from "@/lib/hooks/useAchievements";
import { AchievementType } from "@/types/achievement";
import { Skeleton } from "@/components/ui/skeleton";

const ICONS: Record<AchievementType, LucideIcon> = {
  [AchievementType.FirstProject]: Rocket,
  [AchievementType.FiveStarHero]: Star,
  [AchievementType.TopEarner]: Trophy
};

/** نشان‌های کسب‌شده‌ی کاربر؛ با نشان‌گر (tooltip) توضیحات هر دستاورد نمایش داده می‌شود. */
export function AchievementsList({ userId }: { userId?: number }) {
  const { data: achievements, isLoading } = useUserAchievements(userId);

  if (isLoading) {
    return (
      <div className="space-y-3">
        <Skeleton className="h-5 w-40" />
        <div className="flex flex-wrap gap-3">
          <Skeleton className="h-16 w-40" />
          <Skeleton className="h-16 w-40" />
        </div>
      </div>
    );
  }

  return (
    <section className="space-y-3">
      <div className="flex items-center gap-2">
        <Award className="h-4 w-4 text-primary" />
        <h3 className="text-sm font-semibold">دستاوردها و نشان‌ها</h3>
      </div>

      {!achievements || achievements.length === 0 ? (
        <div className="rounded-xl border border-dashed p-4 text-sm text-muted-foreground">
          هنوز دستاوردی کسب نکرده‌اید؛ با تکمیل پروژه، دریافت امتیاز ۵ ستاره و کسب درآمد،
          نشان‌های تازه‌ای باز می‌شود.
        </div>
      ) : (
        <div className="flex flex-wrap gap-3">
          {achievements.map((achievement) => {
            const Icon = ICONS[achievement.type] ?? Award;

            return (
              <div
                key={achievement.id}
                tabIndex={0}
                className="group relative inline-flex items-center gap-2 rounded-xl border bg-card px-3 py-2 outline-none focus-visible:ring-2 focus-visible:ring-primary"
              >
                <span className="flex h-8 w-8 items-center justify-center rounded-full bg-primary/10 text-primary">
                  <Icon className="h-4 w-4" />
                </span>

                <span className="text-sm font-medium">{achievement.title}</span>

                {/* نشان‌گر بدون وابستگی به کتابخانه‌ی رادیکس */}
                <span
                  role="tooltip"
                  className="pointer-events-none absolute bottom-full left-1/2 z-20 mb-2 w-56 -translate-x-1/2 rounded-lg border bg-popover p-2 text-xs font-normal text-popover-foreground opacity-0 shadow-md transition-opacity duration-150 group-hover:opacity-100 group-focus:opacity-100"
                >
                  {achievement.description}
                  <span className="mt-1 block text-muted-foreground" dir="ltr">
                    {format(new Date(achievement.earnedAt), "yyyy/MM/dd", { locale: faIR })}
                  </span>
                </span>
              </div>
            );
          })}
        </div>
      )}
    </section>
  );
}
