"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  Briefcase,
  CalendarClock,
  FileText,
  FolderKanban,
  LayoutDashboard,
  MessageSquare,
  Newspaper,
  PlusCircle,
  Receipt,
  ScrollText,
  ShieldCheck,
  Star,
  Share2,
  Users,
  Wallet,
  Video,
  Code2
} from "lucide-react";
import { cn } from "@/lib/utils";
import { useAuth } from "@/lib/hooks/useAuth";
import { ROLE_LABELS } from "@/lib/constants";
import { Role } from "@/types/auth";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";

interface NavItem {
  href: string;
  label: string;
  icon: React.ElementType;
}

// منوی هر نقش دقیقاً مطابق پنل خودش است
const NAV_ITEMS: Record<Role, NavItem[]> = {
  [Role.JobSeeker]: [
    { href: "/jobseeker", label: "داشبورد", icon: LayoutDashboard },
    { href: "/feed", label: "صفحه اصلی و پست‌ها", icon: Newspaper },
    { href: "/jobseeker/projects", label: "پروژه‌های من", icon: FolderKanban },
    { href: "/jobseeker/daily-projects", label: "تسک‌های روزانه", icon: CalendarClock },
    { href: "/jobseeker/live-sessions", label: "جلسات زنده", icon: Video },
    { href: "/jobseeker/live-sessions/host", label: "برگزاری جلسه", icon: Video },
    { href: "/jobseeker/resumes", label: "رزومه‌های تاییدشده", icon: FileText },
    { href: "/jobseeker/resume", label: "رزومه‌ی من", icon: FileText },
    { href: "/jobseeker/profile", label: "پروفایل و امتیاز من", icon: Star },
    { href: "/jobseeker/referral", label: "کد معرف", icon: Share2 },
    { href: "/jobseeker/wallet", label: "کیف پول", icon: Wallet },
    { href: "/jobseeker/chat", label: "گفت‌وگو با ادمین", icon: MessageSquare }
  ],
  [Role.Employer]: [
    { href: "/employer", label: "داشبورد", icon: LayoutDashboard },
    { href: "/feed", label: "صفحه اصلی و پست‌ها", icon: Newspaper },
    { href: "/employer/projects/new", label: "ثبت پروژه‌ی جدید", icon: PlusCircle },
    { href: "/employer/daily-projects", label: "پروژه‌های روزانه", icon: CalendarClock },
    { href: "/employer/profile", label: "پروفایل و لوگو", icon: Star },
    { href: "/employer/referral", label: "کد معرف", icon: Share2 },
    { href: "/employer/wallet", label: "کیف پول", icon: Wallet },
    { href: "/employer/chat", label: "گفت‌وگو با ادمین", icon: MessageSquare }
  ],
  [Role.Admin]: [
    { href: "/admin", label: "داشبورد", icon: LayoutDashboard },
    { href: "/feed", label: "صفحه اصلی و پست‌ها", icon: Newspaper },
    { href: "/admin/users", label: "کاربران", icon: Users },
    { href: "/admin/projects", label: "پروژه‌ها", icon: Briefcase },
    { href: "/admin/daily-projects", label: "پروژه‌های روزانه", icon: CalendarClock },
    { href: "/admin/live-sessions", label: "جلسات زنده", icon: Video },
    { href: "/admin/wallets", label: "کیف پول ادمین", icon: Wallet },
    { href: "/admin/transactions", label: "تراکنش‌ها", icon: Receipt },
    { href: "/admin/resumes", label: "رزومه‌ها", icon: FileText },
    { href: "/admin/chat", label: "گفت‌وگوها", icon: MessageSquare },
    { href: "/admin/audit-logs", label: "گزارش ممیزی", icon: ScrollText },
    { href: "/admin/reviews", label: "نظرات و امتیازها", icon: Star }
  ]
};

export function SidebarContent() {
  const pathname = usePathname();
  const { user } = useAuth();
  const items = user ? NAV_ITEMS[user.role] : [];

  return (
    <div className="flex h-full flex-col">
      <div className="flex h-16 items-center gap-2 px-5">
        <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
          <Code2 className="h-5 w-5" />
        </span>
        <span className="text-lg font-bold tracking-tight">CodeBridge</span>
      </div>

      <Separator />

      <nav className="flex-1 space-y-1 overflow-y-auto p-3">
        {items.map((item) => {
          const active =
            item.href === pathname ||
            (item.href !== "/admin" &&
              item.href !== "/employer" &&
              item.href !== "/jobseeker" &&
              pathname.startsWith(`${item.href}/`));

          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors",
                active
                  ? "bg-primary text-primary-foreground"
                  : "text-muted-foreground hover:bg-accent hover:text-foreground"
              )}
            >
              <item.icon className="h-4 w-4" />
              {item.label}
            </Link>
          );
        })}
      </nav>

      <div className="p-4">
        <Separator className="mb-4" />
        <div className="flex items-center gap-2 text-xs text-muted-foreground">
          <ShieldCheck className="h-4 w-4" />
          <span>پنل</span>
          {user ? (
            <Badge variant="secondary">{ROLE_LABELS[user.role]}</Badge>
          ) : (
            <span>—</span>
          )}
        </div>
      </div>
    </div>
  );
}

export function Sidebar() {
  return (
    <aside className="sticky top-0 hidden h-screen w-64 shrink-0 border-l bg-card lg:block">
      <SidebarContent />
    </aside>
  );
}