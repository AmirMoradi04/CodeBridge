"use client";

import { useAuth } from "@/lib/hooks/useAuth";
import { AdminUserChat } from "@/components/chat/AdminUserChat";

export default function EmployerChatPage() {
  const { user } = useAuth();

  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-lg font-semibold">گفت‌وگو با ادمین</h2>
        <p className="text-sm text-muted-foreground">
          پیگیری وضعیت پروژه‌ها، قراردادها و تیم پروژه.
        </p>
      </div>

      <AdminUserChat userId={user?.id} />
    </div>
  );
}