// نوع رکوردهای فید فعالیت زنده
export type ActivityType = "user" | "project" | "review" | "deposit";

export interface ActivityFeedItem {
  type: ActivityType;
  title: string;
  description?: string | null;
  createdAt: string;
}
