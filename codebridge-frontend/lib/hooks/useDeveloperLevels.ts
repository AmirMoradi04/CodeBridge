"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { developerLevelsApi } from "@/lib/api/developerLevels";

export const developerLevelKeys = {
  all: ["developer-levels"] as const,
  byUser: (userId: number) => ["developer-levels", "user", userId] as const,
  top: (count: number) => ["developer-levels", "top", count] as const
};

export function useDeveloperLevel(userId?: number) {
  return useQuery({
    queryKey: developerLevelKeys.byUser(userId ?? 0),
    queryFn: () => developerLevelsApi.byUser(userId as number),
    enabled: Boolean(userId)
  });
}

export function useTopDevelopers(count = 10) {
  return useQuery({
    queryKey: developerLevelKeys.top(count),
    queryFn: () => developerLevelsApi.top(count)
  });
}

export function useUpdateDeveloperScore(userId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (scoreDelta: number) =>
      developerLevelsApi.updateScore(userId, scoreDelta),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: developerLevelKeys.all })
  });
}
