// هم‌راستا با enum NotificationType در بک‌اند
export const NotificationType = {
  System: 0,
  Project: 1,
  Wallet: 2,
  Message: 3
} as const;

export type NotificationType = (typeof NotificationType)[keyof typeof NotificationType];

export interface AppNotification {
  id: number;
  userId: number;
  title: string;
  message: string;
  isRead: boolean;
  link?: string | null;
  type: NotificationType;
  createdAt: string;
}

export const NOTIFICATION_TYPE_LABELS: Record<NotificationType, string> = {
  [NotificationType.System]: "سیستمی",
  [NotificationType.Project]: "پروژه",
  [NotificationType.Wallet]: "کیف پول",
  [NotificationType.Message]: "پیام"
};
