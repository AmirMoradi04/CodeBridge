"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { format } from "date-fns";
import { faIR } from "date-fns/locale";
import { Check, Coins, Copy, Share2, Users } from "lucide-react";
import { toast } from "sonner";
import { useMyReferralInfo } from "@/lib/hooks/useReferrals";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { EmptyState } from "@/components/shared/EmptyState";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from "@/components/ui/table";

// مسیرهای مجاز پنل معرف؛ نقش‌های دیگر به پنل خودشان هدایت می‌شوند
const ALLOWED_ROLES = ["jobseeker", "employer"] as const;

export default function ReferralPage() {
  const params = useParams<{ role: string }>();
  const role = params.role;

  const { data: info, isLoading } = useMyReferralInfo();
  const [link, setLink] = useState("");
  const [copied, setCopied] = useState(false);

  // لینک باید از دامنه‌ی جاری ساخته شود تا روی لوکال و دامنه‌ی اصلی درست باشد
  useEffect(() => {
    if (!info?.referralCode) return;
    setLink(`${window.location.origin}/register?ref=${info.referralCode}`);
  }, [info?.referralCode]);

  async function copyLink() {
    if (!link) return;

    try {
      await navigator.clipboard.writeText(link);
      setCopied(true);
      toast.success("لینک معرف کپی شد.");
      setTimeout(() => setCopied(false), 2000);
    } catch {
      toast.error("کپی کردن ممکن نشد؛ لینک را دستی انتخاب کنید.");
    }
  }

  if (!ALLOWED_ROLES.includes(role as (typeof ALLOWED_ROLES)[number])) {
    return (
      <p className="rounded-xl border p-4 text-sm text-muted-foreground">
        این صفحه فقط برای پنل‌های کارجو، کارفرما و برنامه‌نویس در دسترس است.
      </p>
    );
  }

  const referred = info?.referredUsers ?? [];

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-lg font-semibold">سیستم معرف (زیرمجموعه‌گیری)</h2>
        <p className="text-sm text-muted-foreground">
          کد خود را با دیگران به اشتراک بگذارید و به ازای هر ثبت‌نام موفق پاداش بگیرید.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <Card>
          <CardContent className="p-4">
            <p className="text-sm text-muted-foreground">پاداش هر دعوت</p>
            {isLoading ? (
              <Skeleton className="mt-2 h-7 w-28" />
            ) : (
              <p className="mt-1 text-xl font-semibold">
                {(info?.rewardAmount ?? 0).toLocaleString("fa-IR")}
                <span className="ms-1 text-sm font-normal text-muted-foreground">تومان</span>
              </p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-4">
            <p className="text-sm text-muted-foreground">مجموع پاداش دریافتی</p>
            {isLoading ? (
              <Skeleton className="mt-2 h-7 w-28" />
            ) : (
              <p className="mt-1 text-xl font-semibold text-emerald-600 dark:text-emerald-400">
                {(info?.totalEarned ?? 0).toLocaleString("fa-IR")}
                <span className="ms-1 text-sm font-normal text-muted-foreground">تومان</span>
              </p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-4">
            <p className="text-sm text-muted-foreground">تعداد دعوت‌شدگان</p>
            {isLoading ? (
              <Skeleton className="mt-2 h-7 w-28" />
            ) : (
              <p className="mt-1 text-xl font-semibold">{referred.length.toLocaleString("fa-IR")}</p>
            )}
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">کد و لینک معرف شما</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          {isLoading ? (
            <Skeleton className="h-10 w-full" />
          ) : (
            <>
              <div className="flex flex-wrap items-center gap-3">
                <span className="text-sm text-muted-foreground">کد معرف:</span>
                <code
                  dir="ltr"
                  className="rounded-lg border bg-muted px-3 py-1.5 font-mono text-sm font-semibold tracking-widest"
                >
                  {info?.referralCode ?? "—"}
                </code>
              </div>

              <div className="flex flex-col gap-2 sm:flex-row">
                <Input
                  dir="ltr"
                  readOnly
                  value={link}
                  aria-label="لینک معرف"
                  placeholder="در حال ساخت لینک..."
                  className="flex-1 font-mono text-xs"
                />
                <Button type="button" onClick={() => void copyLink()} disabled={!link}>
                  {copied ? (
                    <Check className="h-4 w-4" />
                  ) : (
                    <Copy className="h-4 w-4" />
                  )}
                  {copied ? "کپی شد" : "کپی لینک"}
                </Button>
              </div>

              <p className="text-xs text-muted-foreground">
                هر کاربری که با این لینک ثبت‌نام کند، برای هر دو طرف{" "}
                {(info?.rewardAmount ?? 0).toLocaleString("fa-IR")} تومان واریز می‌شود.
              </p>
            </>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">کاربران دعوت‌شده</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <Skeleton className="h-32 w-full" />
          ) : referred.length === 0 ? (
            <EmptyState
              icon={Share2}
              title="هنوز کسی را دعوت نکرده‌اید"
              description="لینک معرف خود را کپی کنید و با اولین ثبت‌نام، اولین پاداش را بگیرید."
            />
          ) : (
            <div className="overflow-hidden rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>کاربر</TableHead>
                    <TableHead>ایمیل</TableHead>
                    <TableHead>تاریخ عضویت</TableHead>
                    <TableHead>وضعیت</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {referred.map((user) => (
                    <TableRow key={user.userId}>
                      <TableCell className="font-medium">
                        <span className="inline-flex items-center gap-2">
                          <Users className="h-4 w-4 text-muted-foreground" />
                          {user.firstName} {user.lastName}
                        </span>
                      </TableCell>
                      <TableCell dir="ltr" className="text-xs text-muted-foreground">
                        {user.email ?? "—"}
                      </TableCell>
                      <TableCell className="text-xs text-muted-foreground">
                        {format(new Date(user.joinedAt), "yyyy/MM/dd", { locale: faIR })}
                      </TableCell>
                      <TableCell>
                        <Badge
                          variant={user.isApproved ? "default" : "secondary"}
                          className="gap-1"
                        >
                          <Coins className="h-3 w-3" />
                          {user.isApproved ? "تاییدشده" : "در انتظار تایید"}
                        </Badge>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
