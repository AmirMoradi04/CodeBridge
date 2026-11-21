"use client";

import { useAuth } from "@/lib/hooks/useAuth";
import { AdminUserChat } from "@/components/chat/AdminUserChat";

export default function JobSeekerChatPage() {
  const { user } = useAuth();

  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-lg font-semibold">گفت‌وگو با ادمین</h2>
        <p className="text-sm text-muted-foreground">
          سوالات خود را درباره‌ی تایید حساب، رزومه یا پروژه‌ها بپرسید.
        </p>
      </div>

      <AdminUserChat userId={user?.id} />
    </div>
  );
}