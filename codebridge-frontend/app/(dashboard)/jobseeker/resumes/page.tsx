"use client";

import { FileText } from "lucide-react";
import { useApprovedResumes } from "@/lib/hooks/useResumes";
import { ResumeCard } from "@/components/resumes/ResumeCard";
import { Skeleton } from "@/components/ui/skeleton";
import { EmptyState } from "@/components/shared/EmptyState";

export default function DeveloperResumesPage() {
  const { data: resumes, isLoading } = useApprovedResumes();

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-lg font-semibold">رزومه‌های تاییدشده</h2>
        <p className="text-sm text-muted-foreground">
          فهرست فقط‌خواندنی رزومه‌های بررسی‌شده توسط ادمین
        </p>
      </div>

      {isLoading ? (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          <Skeleton className="h-56 w-full" />
          <Skeleton className="h-56 w-full" />
        </div>
      ) : !resumes || resumes.length === 0 ? (
        <EmptyState
          icon={FileText}
          title="رزومه‌ی تاییدشده‌ای وجود ندارد"
          description="پس از تایید رزومه‌ها توسط ادمین، اینجا نمایش داده می‌شوند."
        />
      ) : (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          {resumes.map((resume) => (
            // برنامه‌نویس فقط متن رزومه را می‌بیند؛ فایل در اختیار صاحب رزومه و ادمین است
            <ResumeCard key={resume.id} resume={resume} showAttachment={false} />
          ))}
        </div>
      )}
    </div>
  );
}