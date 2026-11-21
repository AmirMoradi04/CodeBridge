"use client";

import { useQuery } from "@tanstack/react-query";
import { auditApi } from "@/lib/api/audit";

export const auditKeys = {
  all: ["audit-logs"] as const,
  recent: (count: number) => ["audit-logs", "recent", count] as const
};

export function useRecentAuditLogs(count = 100) {
  return useQuery({
    queryKey: auditKeys.recent(count),
    queryFn: () => auditApi.recent(count)
  });
}
