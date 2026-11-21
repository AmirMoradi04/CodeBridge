"use client";

import { useState } from "react";
import { FileText, Upload } from "lucide-react";
import { toast } from "sonner";
import { getErrorMessage } from "@/lib/api/axios";
import { useAuth } from "@/lib/hooks/useAuth";
import { useMyResume, useSaveResume } from "@/lib/hooks/useResumes";
import { useUploadAttachment } from "@/lib/hooks/useAttachments";
import { ResumeAttachmentLink } from "@/components/resumes/ResumeAttachmentLink";
import { resumeSchema, type ResumeValues } from "@/lib/validators/resume";
import { RESUME_STATUS_CLASSES, RESUME_STATUS_LABELS } from "@/lib/constants";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage
} from "@/components/ui/form";

/** فرم ساخت یا ویرایش رزومه‌ی کارجو به همراه آپلود فایل PDF. */
export function ResumeForm() {
  const { user } = useAuth();
  const userId = user?.id ?? 0;

  const { data: resume, isLoading } = useMyResume(user?.id);
  const saveResume = useSaveResume(userId);
  const upload = useUploadAttachment();
  const [file, setFile] = useState<File | null>(null);

  const form = useForm<ResumeValues>({
    resolver: zodResolver(resumeSchema),
    defaultValues: { title: "", summary: "", skills: "" },
    values: {
      title: resume?.title ?? "",
      summary: resume?.summary ?? "",
      skills: resume?.skills ?? ""
    }
  });

  async function onSubmit(values: ResumeValues) {
    try {
      let attachmentId: number | undefined;

      if (file) {
        const attachment = await upload.mutateAsync({
          file,
          relatedEntityType: "Resume"
        });
        attachmentId = attachment.id;
      }

      await saveResume.mutateAsync({
        title: values.title,
        summary: values.summary,
        skills: values.skills,
        attachmentId: attachmentId ?? resume?.attachmentId ?? null
      });

      toast.success("رزومه ذخیره شد و در انتظار بررسی ادمین است.");
      setFile(null);
    } catch (error) {
      toast.error(getErrorMessage(error, "ذخیره‌ی رزومه ناموفق بود."));
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-3">
        <Skeleton className="h-12 w-full" />
        <Skeleton className="h-32 w-full" />
        <Skeleton className="h-12 w-2/3" />
      </div>
    );
  }

  return (
    <div className="space-y-5">
      {resume && (
        <div className="flex flex-wrap items-center gap-3 rounded-xl border p-4">
          <FileText className="h-5 w-5 text-primary" />
          <div className="flex-1">
            <p className="text-sm font-medium">وضعیت رزومه</p>
            <p className="text-xs text-muted-foreground">
              {resume.reviewedAt
                ? `آخرین بررسی: ${new Date(resume.reviewedAt).toLocaleDateString("fa-IR")}`
                : "هنوز بررسی نشده است."}
            </p>
          </div>
          <Badge className={RESUME_STATUS_CLASSES[resume.status]}>
            {RESUME_STATUS_LABELS[resume.status]}
          </Badge>

          {resume.attachmentId && (
            <ResumeAttachmentLink attachmentId={resume.attachmentId} />
          )}
        </div>
      )}

      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
          <FormField
            control={form.control}
            name="title"
            render={({ field }) => (
              <FormItem>
                <FormLabel>عنوان رزومه</FormLabel>
                <FormControl>
                  <Input placeholder="مثلاً: توسعه‌دهنده‌ی Backend" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="summary"
            render={({ field }) => (
              <FormItem>
                <FormLabel>خلاصه‌ی رزومه</FormLabel>
                <FormControl>
                  <Textarea
                    rows={5}
                    placeholder="درباره‌ی تجربه، تخصص و اهداف شغلی..."
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="skills"
            render={({ field }) => (
              <FormItem>
                <FormLabel>مهارت‌ها</FormLabel>
                <FormControl>
                  <Input placeholder="C#, SQL Server, Docker, ..." {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormItem>
            <FormLabel>فایل رزومه (PDF)</FormLabel>
            <FormControl>
              <Input
                type="file"
                accept=".pdf,.docx,application/pdf"
                onChange={(event) => setFile(event.target.files?.[0] ?? null)}
              />
            </FormControl>
            <p className="text-xs text-muted-foreground">
              فقط pdf، docx، png و jpg پذیرفته می‌شود.
            </p>
          </FormItem>

          <Button
            type="submit"
            disabled={saveResume.isPending || upload.isPending}
          >
            <Upload className="h-4 w-4" />
            {saveResume.isPending ? "در حال ذخیره..." : "ذخیره‌ی رزومه"}
          </Button>
        </form>
      </Form>
    </div>
  );
}
