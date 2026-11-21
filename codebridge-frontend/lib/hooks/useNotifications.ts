"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { notificationsApi } from "@/lib/api/notifications";

// هر ۳۰ ثانیه اعلان‌ها و تعداد خوانده‌نشده‌ها تازه می‌شوند
const POLL_INTERVAL_MS = 30_000;

export const notificationKeys = {
  all: ["notifications"] as const,
  list: () => ["notifications", "list"] as const,
  unread: () => ["notifications", "unread"] as const
};

export function useNotifications(enabled = true) {
  return useQuery({
    queryKey: notificationKeys.list(),
    queryFn: () => notificationsApi.list(),
    enabled,
    refetchInterval: POLL_INTERVAL_MS,
    refetchOnWindowFocus: true
  });
}

export function useUnreadNotificationCount(enabled = true) {
  return useQuery({
    queryKey: notificationKeys.unread(),
    queryFn: () => notificationsApi.unreadCount(),
    enabled,
    refetchInterval: POLL_INTERVAL_MS
  });
}

export function useMarkAllNotificationsAsRead() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => notificationsApi.markAllAsRead(),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: notificationKeys.all })
  });
}
