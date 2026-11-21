"use client";

import { useQuery } from "@tanstack/react-query";
import { achievementsApi } from "@/lib/api/achievements";

export const achievementKeys = {
  byUser: (userId: number) => ["achievements", "user", userId] as const
};

/** دستاوردهای کسب‌شده‌ی یک کاربر. */
export function useUserAchievements(userId?: number) {
  return useQuery({
    queryKey: achievementKeys.byUser(userId ?? 0),
    queryFn: () => achievementsApi.byUser(userId as number),
    enabled: Boolean(userId)
  });
}
