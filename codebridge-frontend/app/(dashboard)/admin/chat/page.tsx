"use client";

import { useMemo, useState } from "react";
import { MessageSquare, Search, UserPlus } from "lucide-react";
import { toast } from "sonner";
import { getErrorMessage } from "@/lib/api/axios";
import { ConversationType, type Conversation } from "@/types/message";
import {
  useConversations,
  useCreateConversation
} from "@/lib/hooks/useMessaging";
import { useUsers } from "@/lib/hooks/useUsers";
import { ROLE_BADGE_CLASSES, ROLE_LABELS } from "@/lib/constants";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card } from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger
} from "@/components/ui/dialog";
import { ChatBox } from "@/components/chat/ChatBox";
import { ConversationList } from "@/components/chat/ConversationList";

export default function AdminChatPage() {
  const { data: conversations = [], isLoading } = useConversations();
  const [activeId, setActiveId] = useState<number | undefined>();
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState("");

  const { data: users = [] } = useUsers(undefined, open);
  const createConversation = useCreateConversation();

  const active: Conversation | undefined = conversations.find(
    (conversation) => conversation.id === activeId
  );

  const results = useMemo(() => {
    const term = query.trim().toLowerCase();

    return users
      .filter((user) => user.id !== 1 || true)
      .filter(
        (user) =>
          term.length === 0 ||
          user.email.toLowerCase().includes(term) ||
          `${user.firstName} ${user.lastName}`.toLowerCase().includes(term)
      )
      .slice(0, 8);
  }, [users, query]);

  async function handleStart(userId: number) {
    try {
      const conversation = await createConversation.mutateAsync({
        type: ConversationType.AdminUser,
        userId
      });

      setActiveId(conversation.id);
      setOpen(false);
      setQuery("");
      toast.success("گفت‌وگو آماده است.");
    } catch (error) {
      toast.error(getErrorMessage(error, "شروع گفت‌وگو ناموفق بود."));
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold">گفت‌وگوها</h2>
          <p className="text-sm text-muted-foreground">
            گفت‌وگوی ادمین-کاربر و گروه‌های پروژه‌ها
          </p>
        </div>

        <Dialog open={open} onOpenChange={setOpen}>
          <DialogTrigger asChild>
            <Button>
              <UserPlus className="h-4 w-4" />
              گفت‌وگوی جدید
            </Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>شروع گفت‌وگو</DialogTitle>
              <DialogDescription>
                کاربر را با ایمیل یا نام جستجو و گفت‌وگو را آغاز کنید.
              </DialogDescription>
            </DialogHeader>

            <div className="relative">
              <Search className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                value={query}
                onChange={(event) => setQuery(event.target.value)}
                placeholder="جستجوی ایمیل یا نام..."
                className="pe-9"
              />
            </div>

            <div className="max-h-72 space-y-1 overflow-y-auto">
              {results.length === 0 ? (
                <p className="p-4 text-center text-sm text-muted-foreground">
                  کاربری پیدا نشد.
                </p>
              ) : (
                results.map((user) => (
                  <button
                    key={user.id}
                    type="button"
                    onClick={() => handleStart(user.id)}
                    className="flex w-full items-center justify-between gap-3 rounded-lg border p-3 text-right transition-colors hover:bg-accent"
                  >
                    <span>
                      <span className="block text-sm font-medium">
                        {user.firstName} {user.lastName}
                      </span>
                      <span className="block text-xs text-muted-foreground" dir="ltr">
                        {user.email}
                      </span>
                    </span>
                    <Badge className={ROLE_BADGE_CLASSES[user.userRole]}>
                      {ROLE_LABELS[user.userRole]}
                    </Badge>
                  </button>
                ))
              )}
            </div>
          </DialogContent>
        </Dialog>
      </div>

      <div className="grid gap-4 lg:grid-cols-[320px_1fr]">
        <Card className="max-h-[70vh] overflow-y-auto">
          {isLoading ? (
            <p className="p-6 text-center text-sm text-muted-foreground">
              در حال بارگذاری...
            </p>
          ) : (
            <ConversationList
              conversations={conversations}
              activeId={active?.id}
              onSelect={(conversation) => setActiveId(conversation.id)}
              emptyHint="هنوز مکالمه‌ای وجود ندارد."
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
    </div>
  );
}