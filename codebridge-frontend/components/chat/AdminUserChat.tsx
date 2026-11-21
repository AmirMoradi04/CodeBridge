"use client";

import { useState } from "react";
import { ConversationType, type Conversation } from "@/types/message";
import { useConversations } from "@/lib/hooks/useMessaging";
import { ChatBox } from "@/components/chat/ChatBox";
import { ConversationList } from "@/components/chat/ConversationList";
import { Card } from "@/components/ui/card";

/** گفت‌وگوی ادمین-کاربر؛ برای پنل کارجو، کارفرما و برنامه‌نویس مشترک است. */
export function AdminUserChat({ userId }: { userId?: number }) {
  const { data: conversations = [], isLoading } = useConversations(Boolean(userId));
  const [activeId, setActiveId] = useState<number | undefined>();

  const direct = conversations.filter(
    (conversation) => conversation.type === ConversationType.AdminUser
  );

  const active: Conversation | undefined =
    direct.find((conversation) => conversation.id === activeId) ?? direct[0];

  return (
    <div className="grid gap-4 lg:grid-cols-[320px_1fr]">
      <Card className="max-h-[70vh] overflow-y-auto">
        {isLoading ? (
          <p className="p-6 text-center text-sm text-muted-foreground">در حال بارگذاری...</p>
        ) : (
          <ConversationList
            conversations={direct}
            activeId={active?.id}
            onSelect={(conversation) => setActiveId(conversation.id)}
            emptyHint="هنوز مکالمه‌ای با ادمین ساخته نشده است؛ ادمین می‌تواند گفت‌وگو را آغاز کند."
          />
        )}
      </Card>

      <Card className="min-h-[70vh] overflow-hidden">
        <ChatBox
          conversationId={active?.id}
          conversationType={active?.type}
          title={active?.title}
        />
      </Card>
    </div>
  );
}