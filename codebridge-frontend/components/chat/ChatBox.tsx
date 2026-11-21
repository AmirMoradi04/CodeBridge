"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { Send } from "lucide-react";
import { toast } from "sonner";
import { getErrorMessage } from "@/lib/api/axios";
import { useAuth } from "@/lib/hooks/useAuth";
import {
  useMarkConversationAsRead,
  useMessages,
  useSendMessage
} from "@/lib/hooks/useMessaging";
import { CONVERSATION_TYPE_LABELS } from "@/lib/constants";
import type { ConversationType } from "@/types/message";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { MessageBubble } from "@/components/chat/MessageBubble";
import { EmptyState } from "@/components/shared/EmptyState";

interface ChatBoxProps {
  conversationId?: number;
  conversationType?: ConversationType;
  title?: string;
}

export function ChatBox({ conversationId, conversationType, title }: ChatBoxProps) {
  const { user } = useAuth();
  const { data: messages, isLoading } = useMessages(conversationId);
  const sendMessage = useSendMessage(conversationId);
  const markRead = useMarkConversationAsRead();
  const [text, setText] = useState("");
  const bottomRef = useRef<HTMLDivElement>(null);

  // با باز شدن پنجره‌ی گفت‌وگو، پیام‌ها خوانده‌شده علامت‌گذاری می‌شوند
  useEffect(() => {
    if (conversationId) markRead.mutate(conversationId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [conversationId]);

  // پیام‌ها ممکن است جدیدترین‌اول برسند؛ برای نمایش قدیمی‌تر در بالا مرتب می‌شوند
  const ordered = useMemo(
    () =>
      [...(messages ?? [])].sort(
        (a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
      ),
    [messages]
  );

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [ordered.length]);

  async function handleSend(event: React.FormEvent) {
    event.preventDefault();

    const content = text.trim();
    if (!content || !conversationId) return;

    try {
      await sendMessage.mutateAsync({ conversationId, content });
      setText("");
      markRead.mutate(conversationId);
    } catch (error) {
      toast.error(getErrorMessage(error, "ارسال پیام ناموفق بود."));
    }
  }

  if (!conversationId) {
    return (
      <div className="flex h-full items-center justify-center p-6">
        <EmptyState
          icon={Send}
          title="مکالمه‌ای انتخاب نشده"
          description="از فهرست کناری یک مکالمه را باز کنید."
        />
      </div>
    );
  }

  return (
    <div className="flex h-full flex-col">
      <div className="flex items-center justify-between border-b px-4 py-3">
        <div>
          <p className="text-sm font-semibold">{title ?? "گفت‌وگو"}</p>
          {conversationType !== undefined && (
            <p className="text-xs text-muted-foreground">
              {CONVERSATION_TYPE_LABELS[conversationType]}
            </p>
          )}
        </div>
        <span className="text-xs text-muted-foreground">پیام‌ها هر ۵ ثانیه تازه می‌شوند</span>
      </div>

      <div className="thin-scrollbar flex-1 space-y-4 overflow-y-auto p-4">
        {isLoading ? (
          <div className="space-y-3">
            <Skeleton className="h-14 w-2/3 rounded-2xl" />
            <Skeleton className="ml-auto h-14 w-1/2 rounded-2xl" />
            <Skeleton className="h-14 w-3/5 rounded-2xl" />
          </div>
        ) : ordered.length === 0 ? (
          <EmptyState
            icon={Send}
            title="هنوز پیامی نیست"
            description="اولین پیام را بنویسید."
          />
        ) : (
          ordered.map((message) => (
            <MessageBubble
              key={message.id}
              message={message}
              isOwn={message.senderId === user?.id}
            />
          ))
        )}
        <div ref={bottomRef} />
      </div>

      <form onSubmit={handleSend} className="flex items-center gap-2 border-t p-3">
        <Input
          value={text}
          onChange={(event) => setText(event.target.value)}
          placeholder="پیام خود را بنویسید..."
          maxLength={2000}
        />
        <Button
          type="submit"
          size="icon"
          disabled={sendMessage.isPending || text.trim().length === 0}
          aria-label="ارسال"
        >
          <Send className="h-4 w-4" />
        </Button>
      </form>
    </div>
  );
}