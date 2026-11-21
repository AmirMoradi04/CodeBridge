"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { attachmentsApi } from "@/lib/api/attachments";

export const attachmentKeys = {
  all: ["attachments"] as const,
  mine: () => ["attachments", "mine"] as const
};

export function useAttachment(attachmentId?: number) {
  return useQuery({
    queryKey: ["attachments", "by-id", attachmentId ?? 0],
    queryFn: () => attachmentsApi.byId(attachmentId as number),
    enabled: Boolean(attachmentId)
  });
}

export function useMyAttachments(enabled = true) {
  return useQuery({
    queryKey: attachmentKeys.mine(),
    queryFn: () => attachmentsApi.mine(),
    enabled
  });
}

export function useUploadAttachment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      file,
      relatedEntityType,
      relatedEntityId
    }: {
      file: File;
      relatedEntityType: string;
      relatedEntityId?: number;
    }) => attachmentsApi.upload(file, relatedEntityType, relatedEntityId),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: attachmentKeys.all })
  });
}

export function useDeleteAttachment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (attachmentId: number) => attachmentsApi.remove(attachmentId),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: attachmentKeys.all })
  });
}
