"use client";

import { useState } from "react";
import { useParams } from "next/navigation";
import { MessagesSquare } from "lucide-react";
import { ConversationType } from "@/types/message";
import { useConversations } from "@/lib/hooks/useMessaging";
import { useProject } from "@/lib/hooks/useProjects";
import { ChatBox } from "@/components/chat/ChatBox";
import { ConversationList } from "@/components/chat/ConversationList";
import { Card } from "@/components/ui/card";
import { EmptyState } from "@/components/shared/EmptyState";
import { Skeleton } from "@/components/ui/skeleton";
import { Badge } from "@/components/ui/badge";
import { PROJECT_STATUS_CLASSES, PROJECT_STATUS_LABELS } from "@/lib/constants";

/** گفت‌وگوی گروهی یک پروژه‌ی مشخص. */
export default function ProjectGroupChatPage() {
  const params = useParams<{ id: string }>();
  const projectId = Number(params.id);
  const [activeId, setActiveId] = useState<number | undefined>();

  const { data: project, isLoading: projectLoading } = useProject(projectId);
  const { data: conversations = [], isLoading: convLoading } = useConversations();

  const groups = conversations.filter(
    (conversation) =>
      conversation.type === ConversationType.ProjectGroup &&
      conversation.projectId === projectId
  );

  const active = groups.find((conversation) => conversation.id === activeId) ?? groups[0];
  const isLoading = projectLoading || convLoading;

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <MessagesSquare className="h-5 w-5 text-primary" />
          <div>
            {projectLoading ? (
              <Skeleton className="h-5 w-48" />
            ) : (
              <h2 className="text-lg font-semibold">{project?.title ?? `پروژه‌ی #${projectId}`}</h2>
            )}
            <div className="flex flex-wrap items-center gap-2">
              <p className="text-sm text-muted-foreground" dir="ltr">
                شناسه: #{projectId}
              </p>
              {project && (
                <Badge className={PROJECT_STATUS_CLASSES[project.status]}>
                  {PROJECT_STATUS_LABELS[project.status]}
                </Badge>
              )}
            </div>
          </div>
        </div>
      </div>

      {isLoading ? (
        <div className="grid gap-4 lg:grid-cols-[320px_1fr]">
          <Skeleton className="h-80 w-full" />
          <Skeleton className="h-80 w-full" />
        </div>
      ) : groups.length === 0 ? (
        <EmptyState
          icon={MessagesSquare}
          title="برای این پروژه گروهی ساخته نشده"
          description="گروه گفت‌وگوی پروژه توسط ادمین ایجاد می‌شود."
        />
      ) : (
        <div className="grid gap-4 lg:grid-cols-[320px_1fr]">
          <Card className="max-h-[70vh] overflow-y-auto">
            <ConversationList
              conversations={groups}
              activeId={active?.id}
              onSelect={(conversation) => setActiveId(conversation.id)}
              emptyHint="گروهی موجود نیست."
            />
          </Card>

          <Card className="min-h-[70vh] overflow-hidden">
            <ChatBox
              conversationId={active?.id}
              conversationType={active?.type}
              title={active?.title}
            />
          </Card>
        </div>
      )}
    </div>
  );
}
