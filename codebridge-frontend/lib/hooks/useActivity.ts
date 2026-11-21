"use client";

import { useQuery } from "@tanstack/react-query";
import { activityApi } from "@/lib/api/activity";

export const activityKeys = {
  feed: (count: number) => ["activity", "feed", count] as const
};

/** فید فعالیت زنده؛ هر ۱۰ ثانیه تازه می‌شود. */
export function useActivityFeed(count = 10) {
  return useQuery({
    queryKey: activityKeys.feed(count),
    queryFn: () => activityApi.feed(count),
    refetchInterval: 10000,
    refetchIntervalInBackground: false
  });
}
