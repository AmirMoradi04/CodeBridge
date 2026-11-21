"use client";

import { formatDistanceToNow } from "date-fns";
import { faIR } from "date-fns/locale";
import { cn } from "@/lib/utils";
import type { Message } from "@/types/message";

export function MessageBubble({
  message,
  isOwn
}: {
  message: Message;
  isOwn: boolean;
}) {
  return (
    <div className={cn("flex flex-col gap-1", isOwn ? "items-end" : "items-start")}>
      <div
        className={cn(
          "max-w-[80%] break-anywhere rounded-2xl px-4 py-2 text-sm shadow-sm",
          isOwn
            ? "rounded-bl-sm bg-primary text-primary-foreground"
            : "rounded-br-sm bg-muted text-foreground"
        )}
      >
        {!isOwn && (
          <p className="mb-1 text-xs font-semibold text-primary">{message.senderName}</p>
        )}
        <p className="whitespace-pre-wrap">{message.content}</p>
      </div>
      <span className="px-1 text-[10px] text-muted-foreground" dir="ltr">
        {formatDistanceToNow(new Date(message.createdAt), {
          addSuffix: true,
          locale: faIR
        })}
      </span>
    </div>
  );
}