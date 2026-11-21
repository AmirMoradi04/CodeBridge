import { api } from "@/lib/api/axios";
import type { AppNotification } from "@/types/notification";

export const notificationsApi = {
  list: () =>
    api.get<AppNotification[]>("/notifications").then((response) => response.data),

  unreadCount: () =>
    api
      .get<number>("/notifications/unread-count")
      .then((response) => response.data),

  markAllAsRead: () =>
    api.post("/notifications/mark-read").then((response) => response.data)
};
