import { format } from "date-fns";
import { faIR } from "date-fns/locale";
import { FileText } from "lucide-react";
import { RESUME_STATUS_CLASSES, RESUME_STATUS_LABELS } from "@/lib/constants";
import type { Resume } from "@/types/resume";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ResumeAttachmentLink } from "@/components/resumes/ResumeAttachmentLink";

export function ResumeCard({
  resume,
  studentName,
  actions,
  showAttachment = true
}: {
  resume: Resume;
  studentName?: string;
  actions?: React.ReactNode;
  /** فایل رزومه فقط برای صاحب رزومه و ادمین قابل مشاهده است. */
  showAttachment?: boolean;
}) {
  return (
    <Card>
      <CardHeader className="flex-row items-start justify-between gap-3">
        <div className="flex items-center gap-2">
          <FileText className="h-5 w-5 text-primary" />
          <div>
            <CardTitle className="text-base">{resume.title}</CardTitle>
            {studentName && (
              <p className="text-xs text-muted-foreground">{studentName}</p>
            )}
          </div>
        </div>

        <Badge className={RESUME_STATUS_CLASSES[resume.status]}>
          {RESUME_STATUS_LABELS[resume.status]}
        </Badge>
      </CardHeader>

      <CardContent className="space-y-3">
        <p className="whitespace-pre-wrap break-anywhere text-sm text-muted-foreground">
          {resume.summary}
        </p>

        <div className="flex flex-wrap items-center gap-2">
          <span className="text-xs text-muted-foreground">فایل رزومه:</span>
          {!resume.attachmentId ? (
            <span className="text-xs text-muted-foreground">پیوست نشده است.</span>
          ) : showAttachment ? (
            <ResumeAttachmentLink attachmentId={resume.attachmentId} />
          ) : (
            <span className="text-xs text-muted-foreground">
              مشاهده‌ی فایل فقط برای ادمین مجاز است.
            </span>
          )}
        </div>

        <div className="flex flex-wrap gap-1.5">
          {resume.skills
            .split(/[,،]/)
            .map((skill) => skill.trim())
            .filter(Boolean)
            .map((skill) => (
              <Badge key={skill} variant="secondary">
                {skill}
              </Badge>
            ))}
        </div>

        <div className="flex items-center justify-between">
          <p className="text-xs text-muted-foreground">
            ثبت: {format(new Date(resume.createdAt), "yyyy/MM/dd", { locale: faIR })}
          </p>
          <div className="flex items-center gap-2">{actions}</div>
        </div>
      </CardContent>
    </Card>
  );
}