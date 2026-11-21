"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { resumesApi } from "@/lib/api/resumes";
import type { SaveResumeInput } from "@/types/resume";

export const resumeKeys = {
  all: ["resumes"] as const,
  byUser: (userId: number) => ["resumes", "user", userId] as const,
  pending: () => ["resumes", "pending"] as const,
  approved: () => ["resumes", "approved"] as const
};

export function useMyResume(userId?: number) {
  return useQuery({
    queryKey: resumeKeys.byUser(userId ?? 0),
    queryFn: () => resumesApi.byUser(userId as number),
    enabled: Boolean(userId),
    retry: false
  });
}

export function usePendingResumes() {
  return useQuery({
    queryKey: resumeKeys.pending(),
    queryFn: () => resumesApi.pending()
  });
}

export function useApprovedResumes() {
  return useQuery({
    queryKey: resumeKeys.approved(),
    queryFn: () => resumesApi.approved()
  });
}

export function useSaveResume(userId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: SaveResumeInput) => resumesApi.save(userId, input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: resumeKeys.all })
  });
}

export function useApproveResume() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (resumeId: number) => resumesApi.approve(resumeId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: resumeKeys.all })
  });
}

export function useRejectResume() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (resumeId: number) => resumesApi.reject(resumeId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: resumeKeys.all })
  });
}
