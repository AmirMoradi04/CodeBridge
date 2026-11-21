"use client";

import { useState } from "react";
import { format } from "date-fns";
import { faIR } from "date-fns/locale";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { toast } from "sonner";
import {
  CalendarClock,
  ExternalLink,
  Link2,
  Loader2,
  Square,
  Users,
  Video
} from "lucide-react";
import { getErrorMessage } from "@/lib/api/axios";
import { useAssignedProjects } from "@/lib/hooks/useProjects";
import {
  useCreateLiveSession,
  useEndLiveSession,
  useSeniorLiveSessions,
  useSetRecordingLink
} from "@/lib/hooks/useLiveSessions";
import { LiveSessionStatus, type LiveSession } from "@/types/live-session";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Skeleton } from "@/components/ui/skeleton";
import { EmptyState } from "@/components/shared/EmptyState";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage
} from "@/components/ui/form";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from "@/components/ui/select";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle
} from "@/components/ui/dialog";

const sessionSchema = z.object({
  projectId: z.number().min(1, "پروژه را انتخاب کنید"),
  title: z.string().min(3, "عنوان حداقل ۳ حرف است"),
  description: z.string().optional(),
  price: z.number().min(0, "قیمت نمی‌تواند منفی باشد"),
  scheduledAt: z.string().min(1, "زمان جلسه الزامی است"),
  durationMinutes: z
    .number()
    .min(5, "حداقل ۵ دقیقه")
    .max(600, "حداکثر ۶۰۰ دقیقه"),
  meetingLink: z.string().url("لینک معتبر (http/https) وارد کنید")
});

type SessionValues = z.infer<typeof sessionSchema>;

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
  onEnd,
  onRecording,
  isEnding
}: {
  session: LiveSession;
  onEnd: (session: LiveSession) => void;
  onRecording: (session: LiveSession) => void;
  isEnding: boolean;
}) {
  const canEnd =
    session.status === LiveSessionStatus.Scheduled ||
    session.status === LiveSessionStatus.Live;

  return (
    <Card>
      <CardHeader className="flex-row items-start justify-between gap-3">
        <div className="space-y-1">
          <CardTitle className="text-base">{session.title}</CardTitle>
          <p className="text-xs text-muted-foreground">
            پروژه: {session.projectTitle ?? `#${session.projectId}`}
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
          <span>{session.durationMinutes} دقیقه</span>
          <span className="inline-flex items-center gap-1">
            <Users className="h-3.5 w-3.5" />
            {session.participantCount} شرکت‌کننده
          </span>
          <span>{session.price.toLocaleString("fa-IR")} تومان</span>
          <span>درآمد: {session.totalRevenue.toLocaleString("fa-IR")} تومان</span>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          {session.meetingLink && (
            <Button asChild variant="outline" size="sm">
              <a href={session.meetingLink} target="_blank" rel="noreferrer">
                <ExternalLink className="h-4 w-4" />
                لینک جلسه
              </a>
            </Button>
          )}

          {canEnd && (
            <Button
              size="sm"
              variant="secondary"
              onClick={() => onEnd(session)}
              disabled={isEnding}
            >
              {isEnding ? (
                <Loader2 className="h-4 w-4 animate-spin" />
              ) : (
                <Square className="h-4 w-4" />
              )}
              پایان جلسه
            </Button>
          )}

          {session.status === LiveSessionStatus.Ended && (
            <>
              <Button size="sm" onClick={() => onRecording(session)}>
                <Link2 className="h-4 w-4" />
                {session.recordingLink ? "ویرایش لینک ضبط" : "افزودن لینک ضبط"}
              </Button>
              {session.recordingLink && (
                <Button asChild variant="ghost" size="sm">
                  <a href={session.recordingLink} target="_blank" rel="noreferrer">
                    <ExternalLink className="h-4 w-4" />
                    مشاهده‌ی ضبط
                  </a>
                </Button>
              )}
            </>
          )}
        </div>
      </CardContent>
    </Card>
  );
}

export default function DeveloperLiveSessionsPage() {
  const { data: projects, isLoading: projectsLoading } = useAssignedProjects();
  const { data: sessions, isLoading } = useSeniorLiveSessions();
  const createSession = useCreateLiveSession();
  const endSession = useEndLiveSession();
  const setRecording = useSetRecordingLink();

  const [recordingTarget, setRecordingTarget] = useState<LiveSession | null>(null);
  const [recordingLink, setRecordingLink] = useState("");

  const form = useForm<SessionValues>({
    resolver: zodResolver(sessionSchema),
    defaultValues: {
      projectId: 0,
      title: "",
      description: "",
      price: 0,
      scheduledAt: "",
      durationMinutes: 60,
      meetingLink: ""
    }
  });

  async function onSubmit(values: SessionValues) {
    try {
      await createSession.mutateAsync({
        projectId: values.projectId,
        title: values.title,
        description: values.description ?? "",
        price: values.price,
        scheduledAt: new Date(values.scheduledAt).toISOString(),
        durationMinutes: values.durationMinutes,
        meetingLink: values.meetingLink
      });

      toast.success("جلسه‌ی زنده ساخته شد.");
      form.reset();
    } catch (error) {
      toast.error(getErrorMessage(error, "ساخت جلسه ناموفق بود."));
    }
  }

  async function handleEnd(session: LiveSession) {
    try {
      await endSession.mutateAsync(session.id);
      toast.success("جلسه پایان یافت و تسویه با شما انجام شد.");
    } catch (error) {
      toast.error(getErrorMessage(error, "پایان جلسه ناموفق بود."));
    }
  }

  function openRecordingDialog(session: LiveSession) {
    setRecordingTarget(session);
    setRecordingLink(session.recordingLink ?? "");
  }

  async function handleSaveRecording() {
    if (!recordingTarget) return;

    try {
      await setRecording.mutateAsync({
        sessionId: recordingTarget.id,
        recordingLink: recordingLink.trim()
      });
      toast.success("لینک ضبط ذخیره شد.");
      setRecordingTarget(null);
    } catch (error) {
      toast.error(getErrorMessage(error, "ذخیره‌ی لینک ضبط ناموفق بود."));
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2">
        <Video className="h-5 w-5 text-primary" />
        <div>
          <h2 className="text-lg font-semibold">جلسه‌های زنده‌ی من</h2>
          <p className="text-sm text-muted-foreground">
            برای پروژه‌های تخصیص‌یافته جلسه بسازید و لینک برگزاری را با کارجویان
            به اشتراک بگذارید.
          </p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">ساخت جلسه‌ی جدید</CardTitle>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <FormField
                  control={form.control}
                  name="projectId"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>پروژه</FormLabel>
                      <Select
                        value={field.value ? String(field.value) : ""}
                        onValueChange={(value) => field.onChange(Number(value))}
                        disabled={projectsLoading}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder="یک پروژه انتخاب کنید" />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {(projects ?? []).map((project) => (
                            <SelectItem key={project.id} value={String(project.id)}>
                              {project.title}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                      <FormDescription>
                        فقط پروژه‌هایی که به شما تخصیص یافته‌اند قابل انتخاب‌اند.
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="title"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>عنوان جلسه</FormLabel>
                      <FormControl>
                        <Input placeholder="مثلاً: ساخت درگاه پرداخت" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <FormField
                control={form.control}
                name="description"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>توضیحات</FormLabel>
                    <FormControl>
                      <Textarea
                        rows={3}
                        placeholder="در این جلسه چه چیزی ساخته می‌شود؟"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <FormField
                  control={form.control}
                  name="price"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>قیمت هر نفر (تومان)</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          inputMode="numeric"
                          min={0}
                          dir="ltr"
                          placeholder="150000"
                          onChange={(event) =>
                            field.onChange(
                              event.target.value ? Number(event.target.value) : 0
                            )
                          }
                          value={String(field.value)}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="durationMinutes"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>مدت جلسه (دقیقه)</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          inputMode="numeric"
                          min={5}
                          max={600}
                          dir="ltr"
                          onChange={(event) =>
                            field.onChange(
                              event.target.value
                                ? Number(event.target.value)
                                : 60
                            )
                          }
                          value={String(field.value)}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <FormField
                  control={form.control}
                  name="scheduledAt"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>زمان شروع</FormLabel>
                      <FormControl>
                        <Input type="datetime-local" dir="ltr" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="meetingLink"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>لینک جلسه</FormLabel>
                      <FormControl>
                        <Input
                          dir="ltr"
                          placeholder="https://meet.google.com/..."
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <Button type="submit" disabled={createSession.isPending}>
                {createSession.isPending ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : (
                  <Video className="h-4 w-4" />
                )}
                ساخت جلسه
              </Button>
            </form>
          </Form>
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
          title="جلسه‌ای ساخته نشده است"
          description="اولین جلسه‌ی زنده‌ی خود را از فرم بالا بسازید."
        />
      ) : (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
          {sessions.map((session) => (
            <SessionCard
              key={session.id}
              session={session}
              onEnd={handleEnd}
              onRecording={openRecordingDialog}
              isEnding={endSession.isPending}
            />
          ))}
        </div>
      )}

      <Dialog
        open={Boolean(recordingTarget)}
        onOpenChange={(open) => !open && setRecordingTarget(null)}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>لینک ضبط جلسه</DialogTitle>
            <DialogDescription>
              لینک ضبط فقط بعد از پایان جلسه و برای شرکت‌کننده‌ها قابل مشاهده
              است.
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-3">
            <Input
              dir="ltr"
              placeholder="https://drive.google.com/..."
              value={recordingLink}
              onChange={(event) => setRecordingLink(event.target.value)}
            />
            <Button
              className="w-full"
              onClick={handleSaveRecording}
              disabled={setRecording.isPending || recordingLink.trim().length === 0}
            >
              {setRecording.isPending && (
                <Loader2 className="h-4 w-4 animate-spin" />
              )}
              ذخیره‌ی لینک ضبط
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
