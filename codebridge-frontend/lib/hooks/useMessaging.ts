"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { messagingApi } from "@/lib/api/messaging";
import type { CreateConversationInput, SendMessageInput } from "@/types/message";

export const messagingKeys = {
  all: ["messaging"] as const,
  conversations: () => ["messaging", "conversations"] as const,
  conversation: (conversationId: number) =>
    ["messaging", "conversation", conversationId] as const,
  messages: (conversationId: number) =>
    ["messaging", "messages", conversationId] as const
};

export function useConversations(enabled = true) {
  return useQuery({
    queryKey: messagingKeys.conversations(),
    queryFn: () => messagingApi.conversations(),
    enabled
  });
}

export function useConversation(conversationId?: number) {
  return useQuery({
    queryKey: messagingKeys.conversation(conversationId ?? 0),
    queryFn: () => messagingApi.conversation(conversationId as number),
    enabled: Boolean(conversationId)
  });
}

// هر ۵ ثانیه پیام‌ها تازه می‌شوند تا گفت‌وگو زنده به نظر برسد
export function useMessages(conversationId?: number) {
  return useQuery({
    queryKey: messagingKeys.messages(conversationId ?? 0),
    queryFn: () => messagingApi.messages(conversationId as number),
    enabled: Boolean(conversationId),
    refetchInterval: 5000,
    refetchOnWindowFocus: false
  });
}

export function useSendMessage(conversationId?: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: SendMessageInput) => messagingApi.send(input),
    onSuccess: () => {
      if (!conversationId) return;

      void queryClient.invalidateQueries({
        queryKey: messagingKeys.messages(conversationId)
      });
    }
  });
}

export function useCreateConversation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: CreateConversationInput) => messagingApi.create(input),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: messagingKeys.conversations() })
  });
}

export function useMarkConversationAsRead() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (conversationId: number) => messagingApi.markRead(conversationId),
    onSuccess: (_data, conversationId) => {
      void queryClient.invalidateQueries({
        queryKey: messagingKeys.conversations()
      });
      void queryClient.invalidateQueries({
        queryKey: messagingKeys.conversation(conversationId)
      });
    }
  });
}

export function useAddParticipant(conversationId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (userId: number) => messagingApi.addParticipant(conversationId, userId),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: messagingKeys.conversation(conversationId)
      });
      void queryClient.invalidateQueries({
        queryKey: messagingKeys.conversations()
      });
    }
  });
}
