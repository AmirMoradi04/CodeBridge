"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useTheme } from "next-themes";
import { Code2, LogOut, Menu, Moon, Sun, UserRound } from "lucide-react";
import { useAuth } from "@/lib/hooks/useAuth";
import { getInitials, getName, ROLE_LABELS } from "@/lib/constants";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { Sheet, SheetContent, SheetTrigger } from "@/components/ui/sheet";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger
} from "@/components/ui/dropdown-menu";
import { SidebarContent } from "@/components/layout/Sidebar";
import { NotificationBell } from "@/components/layout/NotificationBell";

function ThemeToggle() {
  const { resolvedTheme, setTheme } = useTheme();
  const [mounted, setMounted] = useState(false);

  useEffect(() => setMounted(true), []);

  return (
    <Button
      variant="ghost"
      size="icon"
      aria-label="تغییر پوسته روشن/تیره"
      onClick={() => setTheme(resolvedTheme === "dark" ? "light" : "dark")}
    >
      {mounted ? (
        resolvedTheme === "dark" ? (
          <Sun className="h-5 w-5" />
        ) : (
          <Moon className="h-5 w-5" />
        )
      ) : (
        <span className="h-5 w-5" />
      )}
    </Button>
  );
}

/** هدر عمومی برای صفحات خارج از پنل. */
export function PublicHeader() {
  const { isAuthenticated } = useAuth();

  return (
    <header className="sticky top-0 z-40 border-b bg-background/90 backdrop-blur">
      <div className="mx-auto flex h-16 max-w-5xl items-center gap-3 px-4">
        <Link href="/" className="flex items-center gap-2 font-bold">
          <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
            <Code2 className="h-5 w-5" />
          </span>
          CodeBridge
        </Link>

        <div className="ms-auto flex items-center gap-2">
          <ThemeToggle />
          {isAuthenticated ? (
            <Button asChild variant="default">
              <Link href="/jobseeker">ورود به پنل</Link>
            </Button>
          ) : (
            <>
              <Button asChild variant="ghost">
                <Link href="/login">ورود</Link>
              </Button>
              <Button asChild>
                <Link href="/register">ثبت‌نام</Link>
              </Button>
            </>
          )}
        </div>
      </div>
    </header>
  );
}

const PAGE_TITLES: Record<string, string> = {
  "/admin": "پنل مدیریت",
  "/admin/users": "مدیریت کاربران",
  "/admin/projects": "مدیریت پروژه‌ها",
  "/admin/resumes": "بررسی رزومه‌ها",
  "/admin/chat": "گفت‌وگوها",
  "/admin/audit-logs": "گزارش ممیزی",
  "/admin/reviews": "نظرات و امتیازها",
  "/employer/referral": "کد معرف",
  "/developer/referral": "کد معرف",
  "/jobseeker/referral": "کد معرف",
  "/developer/profile": "پروفایل و امتیاز",
  "/admin/transactions": "تراکنش‌ها",
  "/admin/wallets": "کیف پول ادمین",
  "/admin/daily-projects": "پروژه‌های روزانه",
  "/employer/wallet": "کیف پول",
  "/employer/daily-projects": "پروژه‌های روزانه",
  "/employer/profile": "پروفایل شرکت",
  "/jobseeker/wallet": "کیف پول",
  "/jobseeker/daily-projects": "تسک‌های روزانه",
  "/jobseeker/projects": "پروژه‌های من",
  "/jobseeker/live-sessions": "جلسات زنده",
  "/jobseeker/live-sessions/host": "برگزاری جلسه",
  "/jobseeker/profile": "پروفایل و امتیاز",
  "/feed": "صفحه اصلی و پست‌ها",
  "/employer": "پنل کارفرما",
  "/employer/projects": "جزئیات پروژه",
  "/employer/projects/new": "ثبت پروژه‌ی جدید",
  "/employer/chat": "گفت‌وگو با ادمین",
  "/jobseeker": "پنل کارجو",
  "/jobseeker/resume": "رزومه‌ی من",
  "/jobseeker/resumes": "رزومه‌های تاییدشده",
  "/jobseeker/chat": "گفت‌وگو با ادمین"
};

/** هدر داخل پنل با منوی موبایل و منوی کاربر. */
export function Header() {
  const pathname = usePathname();
  const router = useRouter();
  const { user, logout } = useAuth();
  const [sheetOpen, setSheetOpen] = useState(false);

  const title =
    PAGE_TITLES[pathname] ??
    PAGE_TITLES[
      Object.keys(PAGE_TITLES)
        .filter((key) => pathname.startsWith(key))
        .sort((a, b) => b.length - a.length)[0] ??
        ""
    ] ??
    "داشبورد";

  async function handleLogout() {
    await logout();
    router.push("/login");
  }

  return (
    <header className="sticky top-0 z-40 flex h-16 items-center gap-3 border-b bg-background/90 px-4 backdrop-blur">
      <Sheet open={sheetOpen} onOpenChange={setSheetOpen}>
        <SheetTrigger asChild>
          <Button variant="ghost" size="icon" className="lg:hidden" aria-label="منو">
            <Menu className="h-5 w-5" />
          </Button>
        </SheetTrigger>
        <SheetContent side="right" className="w-72 p-0">
          <SidebarContent />
        </SheetContent>
      </Sheet>

      <h1 className="text-sm font-semibold sm:text-base">{title}</h1>

      <div className="ms-auto flex items-center gap-2">
        <NotificationBell />
        <ThemeToggle />

        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" className="gap-2 px-2">
              <Avatar className="h-7 w-7">
                <AvatarFallback>
                  {getInitials({ firstName: user?.firstName, lastName: user?.lastName })}
                </AvatarFallback>
              </Avatar>
              <span className="hidden text-sm sm:inline">
                {getName({ firstName: user?.firstName, lastName: user?.lastName })}
              </span>
            </Button>
          </DropdownMenuTrigger>

          <DropdownMenuContent align="end" className="w-64">
            <DropdownMenuLabel>
              <div className="flex flex-col gap-1">
                <span>
                  {getName({ firstName: user?.firstName, lastName: user?.lastName })}
                </span>
                <span className="text-xs font-normal text-muted-foreground">
                  {user?.email}
                </span>
                {user ? (
                  <Badge variant="secondary" className="w-fit">
                    {ROLE_LABELS[user.role]}
                  </Badge>
                ) : null}
              </div>
            </DropdownMenuLabel>
            <DropdownMenuSeparator />
            <DropdownMenuItem
              onClick={() => user && router.push(`/jobseeker`)}
              className="gap-2"
            >
              <UserRound className="h-4 w-4" />
              صفحه‌ی نخست پنل
            </DropdownMenuItem>
            <DropdownMenuItem onClick={handleLogout} className="gap-2 text-destructive">
              <LogOut className="h-4 w-4" />
              خروج از حساب
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
      <Separator className="hidden" />
    </header>
  );
}