"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { usersApi } from "@/lib/api/users";
import type { Role } from "@/types/auth";
import type { DeveloperLevelType } from "@/types/project";
import type { UpdateProfileInput } from "@/types/user";

export const userKeys = {
  all: ["users"] as const,
  list: (role?: Role) => ["users", "list", role ?? "all"] as const,
  pending: () => ["users", "pending"] as const,
  profile: (userId: number) => ["users", "profile", userId] as const,
  byLevel: (level: DeveloperLevelType) => ["users", "level", level] as const
};

export function useUsers(role?: Role, enabled = true) {
  return useQuery({
    queryKey: userKeys.list(role),
    queryFn: () => usersApi.getAll(role),
    enabled
  });
}

export function usePendingUsers(enabled = true) {
  return useQuery({
    queryKey: userKeys.pending(),
    queryFn: () => usersApi.getPending(),
    enabled
  });
}

export function useUserProfile(userId?: number) {
  return useQuery({
    queryKey: userKeys.profile(userId ?? 0),
    queryFn: () => usersApi.getProfile(userId as number),
    enabled: Boolean(userId)
  });
}

export function useUpdateProfile(userId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: UpdateProfileInput) => usersApi.updateProfile(userId, input),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: userKeys.profile(userId) })
  });
}

export function useApproveUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (userId: number) => usersApi.approve(userId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: userKeys.all })
  });
}

export function useRejectUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (userId: number) => usersApi.reject(userId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: userKeys.all })
  });
}
