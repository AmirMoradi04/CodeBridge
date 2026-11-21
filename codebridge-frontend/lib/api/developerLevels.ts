import { api } from "@/lib/api/axios";
import type { DeveloperLevel } from "@/types/user";
import type { DeveloperLevelType } from "@/types/project";

export const developerLevelsApi = {
  byUser: (userId: number) =>
    api
      .get<DeveloperLevel>(`/developerlevels/user/${userId}`)
      .then((response) => response.data),

  byLevel: (level: DeveloperLevelType) =>
    api
      .get<DeveloperLevel[]>(`/developerlevels/level/${level}`)
      .then((response) => response.data),

  top: (count = 10) =>
    api
      .get<DeveloperLevel[]>("/developerlevels/top", { params: { count } })
      .then((response) => response.data),

  updateScore: (userId: number, scoreDelta: number) =>
    api
      .patch(`/developerlevels/user/${userId}/score`, null, { params: { scoreDelta } })
      .then((response) => response.data)
};
