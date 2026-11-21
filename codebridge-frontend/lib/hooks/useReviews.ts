"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { reviewsApi } from "@/lib/api/reviews";
import type { CreateReviewInput } from "@/types/review";

export const reviewKeys = {
  all: ["reviews"] as const,
  byUser: (userId: number) => ["reviews", "user", userId] as const,
  byProject: (projectId: number) => ["reviews", "project", projectId] as const
};

export function useUserReviews(targetUserId?: number) {
  return useQuery({
    queryKey: reviewKeys.byUser(targetUserId ?? 0),
    queryFn: () => reviewsApi.byUser(targetUserId as number),
    enabled: Boolean(targetUserId)
  });
}

export function useProjectReviews(projectId?: number) {
  return useQuery({
    queryKey: reviewKeys.byProject(projectId ?? 0),
    queryFn: () => reviewsApi.byProject(projectId as number),
    enabled: Boolean(projectId)
  });
}

export function useAllReviews() {
  return useQuery({
    queryKey: [...reviewKeys.all, "all"],
    queryFn: reviewsApi.all
  });
}

export function useSubmitReview() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: CreateReviewInput) => reviewsApi.create(input),
    onSuccess: (_data, input) => {
      void queryClient.invalidateQueries({ queryKey: reviewKeys.all });
      // میانگین امتیاز برنامه‌نویس هم باید تازه شود
      void queryClient.invalidateQueries({ queryKey: ["developer-levels"] });
      void queryClient.invalidateQueries({
        queryKey: reviewKeys.byProject(input.projectId)
      });
    }
  });
}
