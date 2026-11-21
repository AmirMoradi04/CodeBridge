"use client";

import Link from "next/link";
import { formatDistanceToNow } from "date-fns";
import { faIR } from "date-fns/locale";
import {
  Bell,
  BellOff,
  Briefcase,
  CheckCheck,
  Info,
  MessageSquare,
  Wallet
} from "lucide-react";
import {
  useMarkAllNotificationsAsRead,
  useNotifications,
  useUnreadNotificationCount
} from "@/lib/hooks/useNotifications";
import { useAuth } from "@/lib/hooks/useAuth";
import { NotificationType, type AppNotification } from "@/types/notification";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";

const MAX_VISIBLE = 8;

const TYPE_ICONS: Record<NotificationType, React.ElementType> = {
  [NotificationType.System]: Info,
  [NotificationType.Project]: Briefcase,
  [NotificationType.Wallet]: Wallet,
  [NotificationType.Message]: MessageSquare
};

function NotificationRow({ notification }: { notification: AppNotification }) {
  const Icon = TYPE_ICONS[notification.type] ?? Info;

  const content = (
    <div
      className={cn(
        "flex gap-2 rounded-lg p-2 transition-colors",
        notification.isRead ? "opacity-70" : "bg-accent/60",
        notification.link && "hover:bg-accent"
      )}
    >
      <Icon className="mt-0.5 h-4 w-4 shrink-0 text-muted-foreground" />
      <div className="min-w-0 space-y-0.5">
        <p className="truncate text-sm font-medium">{notification.title}</p>
        <p className="text-xs leading-5 text-muted-foreground">{notification.message}</p>
        <p className="text-[11px] text-muted-foreground" dir="ltr">
          {formatDistanceToNow(new Date(notification.createdAt), {
            addSuffix: true,
            locale: faIR
          })}
        </p>
      </div>
    </div>
  );

  return notification.link ? (
    <Link href={notification.link} className="block">
      {content}
    </Link>
  ) : (
    content
  );
}

/** زنگ اعلان مشترک همه‌ی پنل‌ها؛ هر ۳۰ ثانیه تازه می‌شود. */
export function NotificationBell() {
  const { isAuthenticated } = useAuth();
  const { data: notifications = [], isLoading } = useNotifications(isAuthenticated);
  const { data: unreadCount = 0 } = useUnreadNotificationCount(isAuthenticated);
  const markAllAsRead = useMarkAllNotificationsAsRead();

  const visible = notifications.slice(0, MAX_VISIBLE);

  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button
          variant="ghost"
          size="icon"
          className="relative"
          aria-label="اعلان‌ها"
        >
          <Bell className="h-5 w-5" />
          {unreadCount > 0 ? (
            <span className="absolute -end-0.5 -top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-destructive px-1 text-[10px] font-bold text-destructive-foreground">
              {unreadCount > 9 ? "۹+" : unreadCount}
            </span>
          ) : null}
        </Button>
      </PopoverTrigger>

      <PopoverContent align="end" className="w-80 p-0">
        <div className="flex items-center justify-between border-b px-3 py-2">
          <span className="text-sm font-semibold">اعلان‌ها</span>
          <Button
            variant="ghost"
            size="sm"
            className="h-7 gap-1 px-2 text-xs"
            onClick={() => markAllAsRead.mutate()}
            disabled={markAllAsRead.isPending || unreadCount === 0}
          >
            <CheckCheck className="h-3.5 w-3.5" />
            خواندن همه
          </Button>
        </div>

        <div className="max-h-80 space-y-1 overflow-y-auto p-2">
          {isLoading ? (
            <p className="p-3 text-xs text-muted-foreground">در حال بارگذاری...</p>
          ) : visible.length === 0 ? (
            <div className="flex flex-col items-center gap-2 p-6 text-center text-muted-foreground">
              <BellOff className="h-6 w-6" />
              <p className="text-xs">اعلانی وجود ندارد.</p>
            </div>
          ) : (
            visible.map((notification) => (
              <NotificationRow key={notification.id} notification={notification} />
            ))
          )}
        </div>
      </PopoverContent>
    </Popover>
  );
}
