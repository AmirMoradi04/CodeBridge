"use client";

import { FileText } from "lucide-react";
import { toast } from "sonner";
import { getErrorMessage } from "@/lib/api/axios";
import { useApproveResume, usePendingResumes, useRejectResume } from "@/lib/hooks/useResumes";
import { useUserProfile } from "@/lib/hooks/useUsers";
import { getName } from "@/lib/constants";
import { ResumeCard } from "@/components/resumes/ResumeCard";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { EmptyState } from "@/components/shared/EmptyState";
import type { Resume } from "@/types/resume";

function JobSeekerName({ userId }: { userId: number }) {
  const { data: profile, isLoading } = useUserProfile(userId);

  if (isLoading) return <Skeleton className="h-4 w-36" />;
  if (!profile) return <span>کاربر #{userId}</span>;

  return <span>{getName(profile)}</span>;
}

export default function AdminResumesPage() {
  const { data: resumes, isLoading } = usePendingResumes();
  const approveResume = useApproveResume();
  const rejectResume = useRejectResume();

  async function handleApprove(resumeId: number) {
    try {
      await approveResume.mutateAsync(resumeId);
      toast.success("رزومه تایید شد.");
    } catch (error) {
      toast.error(getErrorMessage(error, "تایید رزومه ناموفق بود."));
    }
  }

  async function handleReject(resumeId: number) {
    try {
      await rejectResume.mutateAsync(resumeId);
      toast.success("رزومه رد شد.");
    } catch (error) {
      toast.error(getErrorMessage(error, "رد رزومه ناموفق بود."));
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-lg font-semibold">بررسی رزومه‌ها</h2>
        <p className="text-sm text-muted-foreground">
          رزومه‌های کارجویان در انتظار بررسی را تایید یا رد کنید.
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
          title="رزومه‌ای در انتظار بررسی نیست"
          description="رزومه‌های جدید کارجویان اینجا نمایش داده می‌شوند."
        />
      ) : (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          {resumes.map((resume) => (
            <div key={resume.id} className="space-y-1">
              {/* Job seeker name above the card */}
              <div className="flex items-center gap-2 px-1">
                <span className="text-xs text-muted-foreground">
                  کارجو:
                </span>
                <JobSeekerName userId={resume.userId} />
              </div>
              <ResumeCard
                resume={resume}
                actions={
                  <>
                    <Button
                      size="sm"
                      disabled={approveResume.isPending}
                      onClick={() => handleApprove(resume.id)}
                    >
                      تایید
                    </Button>
                    <Button
                      size="sm"
                      variant="destructive"
                      disabled={rejectResume.isPending}
                      onClick={() => handleReject(resume.id)}
                    >
                      رد
                    </Button>
                  </>
                }
              />
            </div>
          ))}
        </div>
      )}
    </div>
  );
}