"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { liveSessionsApi } from "@/lib/api/live-sessions";
import type { CreateLiveSessionInput } from "@/types/live-session";

export const liveSessionKeys = {
  all: ["live-sessions"] as const,
  mine: () => ["live-sessions", "mine"] as const,
  available: () => ["live-sessions", "available"] as const,
  admin: () => ["live-sessions", "admin"] as const,
  detail: (sessionId: number) => ["live-sessions", "detail", sessionId] as const
};

function useInvalidateLiveSessions() {
  const queryClient = useQueryClient();

  return () => {
    void queryClient.invalidateQueries({ queryKey: liveSessionKeys.all });
    // پرداخت و تسویه موجودی کیف پول را تغییر می‌دهد
    void queryClient.invalidateQueries({ queryKey: ["wallets"] });
  };
}

export function useSeniorLiveSessions() {
  return useQuery({
    queryKey: liveSessionKeys.mine(),
    queryFn: () => liveSessionsApi.mine()
  });
}

export function useAvailableLiveSessions() {
  return useQuery({
    queryKey: liveSessionKeys.available(),
    queryFn: () => liveSessionsApi.available()
  });
}

export function useAdminLiveSessions() {
  return useQuery({
    queryKey: liveSessionKeys.admin(),
    queryFn: () => liveSessionsApi.admin()
  });
}

export function useLiveSession(sessionId?: number) {
  return useQuery({
    queryKey: liveSessionKeys.detail(sessionId ?? 0),
    queryFn: () => liveSessionsApi.byId(sessionId as number),
    enabled: Boolean(sessionId)
  });
}

export function useCreateLiveSession() {
  const invalidate = useInvalidateLiveSessions();

  return useMutation({
    mutationFn: (input: CreateLiveSessionInput) => liveSessionsApi.create(input),
    onSuccess: invalidate
  });
}

export function useJoinLiveSession() {
  const invalidate = useInvalidateLiveSessions();

  return useMutation({
    mutationFn: (sessionId: number) => liveSessionsApi.join(sessionId),
    onSuccess: invalidate
  });
}

export function useEndLiveSession() {
  const invalidate = useInvalidateLiveSessions();

  return useMutation({
    mutationFn: (sessionId: number) => liveSessionsApi.end(sessionId),
    onSuccess: invalidate
  });
}

export function useSetRecordingLink() {
  const invalidate = useInvalidateLiveSessions();

  return useMutation({
    mutationFn: ({
      sessionId,
      recordingLink
    }: {
      sessionId: number;
      recordingLink: string;
    }) => liveSessionsApi.setRecording(sessionId, recordingLink),
    onSuccess: invalidate
  });
}
