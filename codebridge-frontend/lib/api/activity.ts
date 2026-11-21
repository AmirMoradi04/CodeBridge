import { api } from "@/lib/api/axios";
import type { ActivityFeedItem } from "@/types/activity";

export const activityApi = {
  feed: (count = 10) =>
    api
      .get<ActivityFeedItem[]>("/activity/feed", { params: { count } })
      .then((r) => r.data)
};
