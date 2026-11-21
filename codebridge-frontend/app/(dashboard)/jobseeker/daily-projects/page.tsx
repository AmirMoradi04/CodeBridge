"use client";

import { useState, useCallback } from "react";
import { Briefcase } from "lucide-react";
import { useAuth } from "@/lib/hooks/useAuth";
import { useActiveDailyProjects, useApplyDailyProject, useStartDailyTask, useSubmitDailyTask } from "@/lib/hooks/useDailyProjects";
import { attachmentsApi } from "@/lib/api/attachments";
import { getErrorMessage } from "@/lib/api/axios";
import { Timer } from "@/components/shared/Timer";
import { EmptyState } from "@/components/shared/EmptyState";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { toast } from "sonner";

function ProjectCardSkeleton() {
  return (
    <Card>
      <CardContent className="p-4">
        <Skeleton className="h-5 w-3/4 mb-2" />
        <Skeleton className="h-4 w-1/2 mb-3" />
        <div className="flex gap-2 mb-3">
          <Skeleton className="h-5 w-20" />
          <Skeleton className="h-5 w-20" />
        </div>
        <Skeleton className="h-4 w-full" />
      </CardContent>
    </Card>
  );
}

export default function DailyProjectsPage() {
  const { user } = useAuth();
  const { data: projects, isLoading } = useActiveDailyProjects();

  const applyMutation = useApplyDailyProject();
  const startTaskMutation = useStartDailyTask();
  const submitTaskMutation = useSubmitDailyTask();

  const [activeProjectId, setActiveProjectId] = useState<number | null>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleApply = async (projectId: number) => {
    try {
      await applyMutation.mutateAsync(projectId);
      toast.success("درخواست شما با موفقیت ثبت شد.");
    } catch (err) {
      toast.error(getErrorMessage(err, "خطا در ثبت درخواست."));
    }
  };

  const handleStart = async (projectId: number) => {
    if (!user?.id) return;
    try {
      await startTaskMutation.mutateAsync({ dailyProjectId: projectId });
      setActiveProjectId(projectId);
      toast.success("تسک با موفقیت شروع شد.");
    } catch (err) {
      toast.error(getErrorMessage(err, "خطا در شروع تسک."));
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) setSelectedFile(file);
  };

  const handleSubmitTask = async (projectId: number) => {
    if (!selectedFile) {
      toast.error("لطفاً فایل اثبات را آپلود کنید.");
      return;
    }
    setIsSubmitting(true);
    try {
      const attachment = await attachmentsApi.upload(selectedFile, "DailyProject", projectId);
      await submitTaskMutation.mutateAsync({
        dailyProjectId: projectId,
        proofAttachmentId: attachment.id,
      });
      toast.success("تسک با موفقیت ارسال شد.");
      setActiveProjectId(null);
      setSelectedFile(null);
    } catch (err) {
      toast.error(getErrorMessage(err, "خطا در ارسال تسک."));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-semibold">تسک‌های روزانه</h1>
      </div>

      {isLoading ? (
        <div className="space-y-3">
          <ProjectCardSkeleton />
          <ProjectCardSkeleton />
          <ProjectCardSkeleton />
        </div>
      ) : !projects || projects.length === 0 ? (
        <EmptyState
          icon={Briefcase}
          title="تسک روزانه‌ای موجود نیست"
          description="به‌زودی تسک‌های جدید اضافه می‌شوند."
        />
      ) : (
        <div className="space-y-4">
          {projects.map((project) => {
            const isActive = activeProjectId === project.id;
            const filledPercent = project.capacity > 0
              ? Math.round((project.filledSlots / project.capacity) * 100)
              : 0;
            const deadline = new Date(Date.now() + project.deadlineSeconds * 1000);

            return (
              <Card key={project.id}>
                <CardContent className="p-4 space-y-3">
                  <div className="flex items-start justify-between gap-3">
                    <div className="flex-1">
                      <h3 className="font-semibold">{project.title}</h3>
                      {project.description && (
                        <p className="text-sm text-muted-foreground mt-1">
                          {project.description}
                        </p>
                      )}
                    </div>
                    <Badge variant={project.isPaid ? "default" : "secondary"}>
                      {project.isPaid ? "پرداخت شده" : "در انتظار پرداخت"}
                    </Badge>
                  </div>

                  <div className="flex flex-wrap gap-3 text-sm">
                    <span className="text-muted-foreground">
                      بودجه: <span className="font-semibold text-foreground">
                        {project.totalBudget.toLocaleString("fa-IR")} تومان
                      </span>
                    </span>
                    <span className="text-muted-foreground">
                      قیمت هر واحد: <span className="font-semibold text-foreground">
                        {project.pricePerUnit.toLocaleString("fa-IR")} تومان
                      </span>
                    </span>
                  </div>

                  <div className="space-y-1">
                    <div className="flex justify-between text-xs text-muted-foreground">
                      <span>ظرفیت پر شده</span>
                      <span>{project.filledSlots} / {project.capacity}</span>
                    </div>
                    <div className="h-2 w-full rounded-full bg-muted">
                      <div
                        className="h-2 rounded-full bg-primary transition-all"
                        style={{ width: `${filledPercent}%` }}
                      />
                    </div>
                  </div>

                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2 text-sm">
                      <span className="text-muted-foreground">ددلاین:</span>
                      <Timer expiresAt={deadline} />
                    </div>

                    <div className="flex gap-2">
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => handleApply(project.id)}
                        disabled={applyMutation.isPending}
                      >
                        درخواست
                      </Button>

                      <Button
                        size="sm"
                        onClick={() => handleStart(project.id)}
                        disabled={startTaskMutation.isPending}
                      >
                        شروع تسک
                      </Button>
                    </div>
                  </div>

                  {isActive && (
                    <div className="rounded-lg border bg-muted/50 p-4 space-y-3">
                      <p className="text-sm font-medium">ارسال اثبات انجام تسک</p>
                      {project.instructions && (
                        <p className="text-sm text-muted-foreground">{project.instructions}</p>
                      )}
                      <div className="flex items-center gap-3">
                        <input
                          type="file"
                          id={`file-${project.id}`}
                          className="text-sm"
                          onChange={handleFileChange}
                        />
                      </div>
                      <div className="flex gap-2">
                        <Button
                          size="sm"
                          onClick={() => handleSubmitTask(project.id)}
                          disabled={isSubmitting || submitTaskMutation.isPending}
                        >
                          {isSubmitting ? "در حال ارسال..." : "ارسال تسک"}
                        </Button>
                        <Button
                          size="sm"
                          variant="ghost"
                          onClick={() => {
                            setActiveProjectId(null);
                            setSelectedFile(null);
                          }}
                        >
                          انصراف
                        </Button>
                      </div>
                    </div>
                  )}
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}
