"use client";

import { cn } from "@/lib/utils";
import { CONVERSATION_TYPE_LABELS } from "@/lib/constants";
import { ConversationType, type Conversation } from "@/types/message";
import { Badge } from "@/components/ui/badge";
import { MessageSquare, Users } from "lucide-react";

interface ConversationListProps {
  conversations: Conversation[];
  activeId?: number;
  onSelect: (conversation: Conversation) => void;
  emptyHint?: string;
}

export function ConversationList({
  conversations,
  activeId,
  onSelect,
  emptyHint = "مکالمه‌ای وجود ندارد."
}: ConversationListProps) {
  if (conversations.length === 0) {
    return <p className="p-6 text-center text-sm text-muted-foreground">{emptyHint}</p>;
  }

  return (
    <div className="space-y-1 p-2">
      {conversations.map((conversation) => (
        <button
          key={conversation.id}
          type="button"
          onClick={() => onSelect(conversation)}
          className={cn(
            "flex w-full flex-col gap-1.5 rounded-lg px-3 py-3 text-right transition-colors",
            activeId === conversation.id
              ? "bg-primary/10 text-foreground ring-1 ring-primary/40"
              : "hover:bg-accent"
          )}
        >
          <span className="flex items-center gap-2">
            <MessageSquare className="h-4 w-4 text-primary" />
            <span className="flex-1 truncate text-sm font-medium">{conversation.title}</span>
            {conversation.unreadCount > 0 && (
              <span className="flex h-5 min-w-5 items-center justify-center rounded-full bg-primary px-1.5 text-[10px] font-bold text-primary-foreground">
                {conversation.unreadCount > 99 ? "۹۹+" : conversation.unreadCount}
              </span>
            )}
          </span>

          <span className="flex flex-wrap items-center gap-2">
            <Badge variant="secondary" className="text-[10px]">
              {CONVERSATION_TYPE_LABELS[conversation.type]}
            </Badge>

            {conversation.type === ConversationType.ProjectGroup && (
              <Badge variant="outline" className="text-[10px]">
                پروژه #{conversation.projectId}
              </Badge>
            )}
          </span>

          <span className="flex items-center gap-1 text-xs text-muted-foreground">
            <Users className="h-3 w-3" />
            {conversation.participants.map((p) => p.displayName).join("، ")}
          </span>
        </button>
      ))}
    </div>
  );
}