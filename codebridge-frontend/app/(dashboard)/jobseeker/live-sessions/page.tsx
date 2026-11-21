"use client";

import { useState } from "react";
import Link from "next/link";
import { format } from "date-fns";
import { faIR } from "date-fns/locale";
import { toast } from "sonner";
import {
  CalendarClock,
  CheckCircle2,
  ExternalLink,
  Loader2,
  Timer,
  Users,
  Video,
  Wallet
} from "lucide-react";
import { getErrorMessage } from "@/lib/api/axios";
import {
  useAvailableLiveSessions,
  useJoinLiveSession
} from "@/lib/hooks/useLiveSessions";
import { LiveSessionStatus, type LiveSession } from "@/types/live-session";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { EmptyState } from "@/components/shared/EmptyState";

const STATUS_LABELS: Record<LiveSessionStatus, string> = {
  [LiveSessionStatus.Scheduled]: "زمان‌بندی‌شده",
  [LiveSessionStatus.Live]: "در حال برگزاری",
  [LiveSessionStatus.Ended]: "پایان‌یافته",
  [LiveSessionStatus.Cancelled]: "لغوشده"
};

const STATUS_CLASSES: Record<LiveSessionStatus, string> = {
  [LiveSessionStatus.Scheduled]: "bg-sky-100 text-sky-800 dark:bg-sky-500/15 dark:text-sky-300",
  [LiveSessionStatus.Live]: "bg-emerald-100 text-emerald-800 dark:bg-emerald-500/15 dark:text-emerald-300",
  [LiveSessionStatus.Ended]: "bg-muted text-muted-foreground",
  [LiveSessionStatus.Cancelled]: "bg-rose-100 text-rose-800 dark:bg-rose-500/15 dark:text-rose-300"
};

function SessionCard({
  session,
  onJoin,
  isJoining
}: {
  session: LiveSession;
  onJoin: (session: LiveSession) => void;
  isJoining: boolean;
}) {
  return (
    <Card>
      <CardHeader className="flex-row items-start justify-between gap-3">
        <div className="space-y-1">
          <CardTitle className="text-base">{session.title}</CardTitle>
          <p className="text-xs text-muted-foreground">
            {session.seniorName ?? "برنامه‌نویس ارشد"}
            {session.projectTitle ? ` • پروژه: ${session.projectTitle}` : ""}
          </p>
        </div>
        <Badge className={STATUS_CLASSES[session.status]}>
          {STATUS_LABELS[session.status]}
        </Badge>
      </CardHeader>

      <CardContent className="space-y-3">
        {session.description && (
          <p className="whitespace-pre-wrap break-anywhere text-sm text-muted-foreground">
            {session.description}
          </p>
        )}

        <div className="flex flex-wrap gap-x-4 gap-y-1 text-xs text-muted-foreground">
          <span className="inline-flex items-center gap-1">
            <CalendarClock className="h-3.5 w-3.5" />
            {format(new Date(session.scheduledAt), "yyyy/MM/dd HH:mm", {
              locale: faIR
            })}
          </span>
          <span className="inline-flex items-center gap-1">
            <Timer className="h-3.5 w-3.5" />
            {session.durationMinutes} دقیقه
          </span>
          <span className="inline-flex items-center gap-1">
            <Users className="h-3.5 w-3.5" />
            {session.participantCount} نفر
          </span>
        </div>

        <div className="flex flex-wrap items-center justify-between gap-2 border-t pt-3">
          <span className="text-sm font-medium">
            {session.price > 0
              ? `${session.price.toLocaleString("fa-IR")} تومان`
              : "رایگان"}
          </span>

          {session.isJoined && session.meetingLink ? (
            <div className="flex items-center gap-2">
              <Badge variant="secondary" className="gap-1">
                <CheckCircle2 className="h-3.5 w-3.5" />
                ثبت‌نام شده
              </Badge>
              <Button asChild size="sm">
                <a href={session.meetingLink} target="_blank" rel="noreferrer">
                  <ExternalLink className="h-4 w-4" />
                  ورود به جلسه
                </a>
              </Button>
            </div>
          ) : session.isJoined ? (
            <Badge variant="secondary">ثبت‌نام شده</Badge>
          ) : (
            <Button
              size="sm"
              onClick={() => onJoin(session)}
              disabled={isJoining}
            >
              {isJoining ? (
                <Loader2 className="h-4 w-4 animate-spin" />
              ) : (
                <Wallet className="h-4 w-4" />
              )}
              ثبت‌نام و پرداخت
            </Button>
          )}
        </div>
      </CardContent>
    </Card>
  );
}

export default function JobSeekerLiveSessionsPage() {
  const { data: sessions, isLoading } = useAvailableLiveSessions();
  const joinSession = useJoinLiveSession();
  const [joiningId, setJoiningId] = useState<number | null>(null);

  async function handleJoin(session: LiveSession) {
    setJoiningId(session.id);

    try {
      await joinSession.mutateAsync(session.id);
      toast.success(
        "ثبت‌نام انجام شد؛ لینک جلسه بعد از پرداخت در کارت جلسه نمایش داده می‌شود."
      );
    } catch (error) {
      toast.error(getErrorMessage(error, "ثبت‌نام در جلسه ناموفق بود."));
    } finally {
      setJoiningId(null);
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2">
        <Video className="h-5 w-5 text-primary" />
        <div>
          <h2 className="text-lg font-semibold">تماشای کار برنامه‌نویس ارشد</h2>
          <p className="text-sm text-muted-foreground">
            با پرداخت هزینه‌ی هر جلسه، لینک ورود به جلسه‌ی زنده برای شما فعال
            می‌شود.
          </p>
        </div>
      </div>

      <Card className="border-dashed">
        <CardContent className="flex flex-wrap items-center justify-between gap-3 py-4 text-sm">
          <span className="inline-flex items-center gap-2 text-muted-foreground">
            <Wallet className="h-4 w-4" />
            پرداخت از موجودی کیف پول انجام می‌شود.
          </span>
          <Button asChild variant="outline" size="sm">
            <Link href="/jobseeker/wallet">شارژ کیف پول</Link>
          </Button>
        </CardContent>
      </Card>

      {isLoading ? (
        <div className="space-y-3">
          <Skeleton className="h-44 w-full" />
          <Skeleton className="h-44 w-full" />
        </div>
      ) : !sessions || sessions.length === 0 ? (
        <EmptyState
          icon={Video}
          title="جلسه‌ی فعالی وجود ندارد"
          description="جلسه‌های جدید برنامه‌نویسان ارشد اینجا نمایش داده می‌شوند."
        />
      ) : (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
          {sessions.map((session) => (
            <SessionCard
              key={session.id}
              session={session}
              onJoin={handleJoin}
              isJoining={joiningId === session.id}
            />
          ))}
        </div>
      )}
    </div>
  );
}
