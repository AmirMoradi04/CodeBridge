import { api } from "@/lib/api/axios";
import type { Achievement } from "@/types/achievement";

export const achievementsApi = {
  byUser: (userId: number) =>
    api.get<Achievement[]>(`/achievements/user/${userId}`).then((r) => r.data)
};
