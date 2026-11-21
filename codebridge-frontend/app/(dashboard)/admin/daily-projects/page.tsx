"use client";

import { useState } from "react";
import type { DailyApplicationDto } from "@/types/daily-project";
import { toast } from "sonner";
import { getErrorMessage } from "@/lib/api/axios";
import { useAuth } from "@/lib/hooks/useAuth";
import {
  usePendingDailyProjects,
  useActiveDailyProjects,
  useApproveDailyProject,
  useRejectDailyProject,
  useActivateDailyProject,
  useApproveDailyTask,
  useRejectDailyTask,
  useDailyProjectApplications,
} from "@/lib/hooks/useDailyProjects";
import { DailyProjectStatus, DailyApplicationStatus } from "@/types/daily-project";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { EmptyState } from "@/components/shared/EmptyState";
import {
  Table, TableBody, TableCell, TableHead, TableHeader, TableRow,
} from "@/components/ui/table";
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger,
} from "@/components/ui/dialog";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Briefcase, CheckCircle, Clock, XCircle } from "lucide-react";
import { useQuery } from "@tanstack/react-query";
import { dailyProjectsApi } from "@/lib/api/daily-projects";

const STATUS_LABELS: Record<number, string> = {
  [DailyProjectStatus.Pending]: "در انتظار",
  [DailyProjectStatus.Approved]: "تایید شده",
  [DailyProjectStatus.Active]: "فعال",
  [DailyProjectStatus.Completed]: "تکمیل",
  [DailyProjectStatus.Rejected]: "رد شده",
};

const STATUS_COLORS: Record<number, string> = {
  [DailyProjectStatus.Pending]: "bg-yellow-100 text-yellow-800",
  [DailyProjectStatus.Approved]: "bg-blue-100 text-blue-800",
  [DailyProjectStatus.Active]: "bg-green-100 text-green-800",
  [DailyProjectStatus.Completed]: "bg-gray-100 text-gray-800",
  [DailyProjectStatus.Rejected]: "bg-red-100 text-red-800",
};

const APP_STATUS_LABELS: Record<number, string> = {
  [DailyApplicationStatus.Pending]: "در انتظار",
  [DailyApplicationStatus.InProgress]: "در حال انجام",
  [DailyApplicationStatus.Submitted]: "ارسال شده",
  [DailyApplicationStatus.Approved]: "تایید شده",
  [DailyApplicationStatus.Rejected]: "رد شده",
  [DailyApplicationStatus.Expired]: "منقضی",
};

export default function AdminDailyProjectsPage() {
  const { user } = useAuth();
  const { data: pendingProjects, isLoading: pendingLoading } = usePendingDailyProjects();
  const { data: activeProjects, isLoading: activeLoading } = useActiveDailyProjects();

  const approveMutation = useApproveDailyProject();
  const rejectMutation = useRejectDailyProject();
  const activateMutation = useActivateDailyProject();
  const approveTaskMutation = useApproveDailyTask();
  const rejectTaskMutation = useRejectDailyTask();

  // Reject dialog state
  const [rejectDialogOpen, setRejectDialogOpen] = useState(false);
  const [rejectTarget, setRejectTarget] = useState<{ type: "project" | "task"; id: number; reason: string }>({ type: "project", id: 0, reason: "" });

  // Applications view state
  const [viewingProjectId, setViewingProjectId] = useState<number | null>(null);
  const { data: applications } = useDailyProjectApplications(viewingProjectId ?? 0);

  const handleReject = async () => {
    if (!rejectTarget.reason.trim()) {
      toast.error("دلیل رد الزامی است.");
      return;
    }
    try {
      if (rejectTarget.type === "project") {
        await rejectMutation.mutateAsync({ projectId: rejectTarget.id, reason: rejectTarget.reason });
        toast.success("پروژه رد شد.");
      } else {
        await rejectTaskMutation.mutateAsync({ applicationId: rejectTarget.id, reason: rejectTarget.reason });
        toast.success("تسک رد شد.");
      }
      setRejectDialogOpen(false);
    } catch (err) {
      toast.error(getErrorMessage(err, "عملیات ناموفق بود."));
    }
  };

  const handleApprove = async (projectId: number) => {
    try {
      await approveMutation.mutateAsync(projectId);
      toast.success("پروژه تایید شد.");
    } catch (err) {
      toast.error(getErrorMessage(err, "تایید ناموفق بود."));
    }
  };

  const handleActivate = async (projectId: number) => {
    try {
      await activateMutation.mutateAsync(projectId);
      toast.success("پروژه فعال شد.");
    } catch (err) {
      toast.error(getErrorMessage(err, "فعال‌سازی ناموفق بود."));
    }
  };

  const handleApproveTask = async (applicationId: number) => {
    try {
      await approveTaskMutation.mutateAsync(applicationId);
      toast.success("تسک تایید شد و پرداخت انجام شد.");
    } catch (err) {
      toast.error(getErrorMessage(err, "تایید تسک ناموفق بود."));
    }
  };

  const allProjects = [...(pendingProjects ?? []), ...(activeProjects ?? [])];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-xl font-semibold">مدیریت پروژه‌های روزانه</h1>
        <p className="text-sm text-muted-foreground">تایید، رد و فعال‌سازی پروژه‌های روزانه</p>
      </div>

      {pendingLoading ? (
        <div className="space-y-2"><Skeleton className="h-12 w-full" /><Skeleton className="h-12 w-full" /></div>
      ) : allProjects.length === 0 ? (
        <EmptyState icon={Briefcase} title="پروژه‌ای نیست" description="هنوز پروژه روزانه‌ای ثبت نشده." />
      ) : (
        <div className="rounded-md border overflow-hidden">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>#</TableHead>
                <TableHead>عنوان</TableHead>
                <TableHead>کارفرما</TableHead>
                <TableHead>بودجه</TableHead>
                <TableHead>وضعیت</TableHead>
                <TableHead>ظرفیت</TableHead>
                <TableHead>عملیات</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {allProjects.map((project) => (
                <TableRow key={project.id}>
                  <TableCell>{project.id}</TableCell>
                  <TableCell className="font-medium">{project.title}</TableCell>
                  <TableCell className="text-muted-foreground">{project.employerName ?? "—"}</TableCell>
                  <TableCell>{project.totalBudget.toLocaleString("fa-IR")}</TableCell>
                  <TableCell>
                    <Badge className={STATUS_COLORS[project.status]}>
                      {STATUS_LABELS[project.status]}
                    </Badge>
                  </TableCell>
                  <TableCell>{project.filledSlots}/{project.capacity}</TableCell>
                  <TableCell>
                    <div className="flex flex-wrap gap-1">
                      {project.status === DailyProjectStatus.Pending && (
                        <>
                          <Button size="sm" onClick={() => handleApprove(project.id)} disabled={approveMutation.isPending}>
                            تایید
                          </Button>
                          <Button size="sm" variant="destructive" onClick={() => { setRejectTarget({ type: "project", id: project.id, reason: "" }); setRejectDialogOpen(true); }}>
                            رد
                          </Button>
                        </>
                      )}
                      {project.status === DailyProjectStatus.Approved && (
                        <Button size="sm" onClick={() => handleActivate(project.id)} disabled={activateMutation.isPending}>
                          فعال‌سازی
                        </Button>
                      )}
                      {project.status === DailyProjectStatus.Active && (
                        <Button size="sm" variant="outline" onClick={() => setViewingProjectId(project.id)}>
                          مشاهده درخواست‌ها
                        </Button>
                      )}
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      {/* Applications Drawer */}
      {viewingProjectId && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">درخواست‌های پروژه #{viewingProjectId}</CardTitle>
          </CardHeader>
          <CardContent>
            {!applications || applications.length === 0 ? (
              <p className="text-sm text-muted-foreground">درخواستی ثبت نشده.</p>
            ) : (
              <div className="space-y-3">
                {applications.map((app: DailyApplicationDto) => (
                  <div key={app.id} className="flex items-center justify-between rounded-lg border p-3">
                    <div>
                      <p className="text-sm font-medium">{app.developerName ?? `کاربر #${app.developerId}`}</p>
                      <p className="text-xs text-muted-foreground">
                        وضعیت: <span className="font-semibold">{APP_STATUS_LABELS[app.status]}</span>
                        {app.startedAt && ` | شروع: ${new Date(app.startedAt).toLocaleDateString("fa-IR")}`}
                      </p>
                    </div>
                    {app.status === DailyApplicationStatus.Submitted && (
                      <div className="flex gap-2">
                        <Button size="sm" onClick={() => handleApproveTask(app.id)}>
                          <CheckCircle className="h-3 w-3" /> تایید تسک
                        </Button>
                        <Button size="sm" variant="destructive" onClick={() => { setRejectTarget({ type: "task", id: app.id, reason: "" }); setRejectDialogOpen(true); }}>
                          <XCircle className="h-3 w-3" /> رد تسک
                        </Button>
                      </div>
                    )}
                    {app.status === DailyApplicationStatus.Approved && (
                      <Badge className="bg-green-100 text-green-800"><CheckCircle className="h-3 w-3 mr-1" />تایید شده</Badge>
                    )}
                  </div>
                ))}
              </div>
            )}
            <Button variant="ghost" size="sm" className="mt-3" onClick={() => setViewingProjectId(null)}>بستن</Button>
          </CardContent>
        </Card>
      )}

      {/* Reject Dialog */}
      <Dialog open={rejectDialogOpen} onOpenChange={setRejectDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>رد {rejectTarget.type === "project" ? "پروژه" : "تسک"}</DialogTitle>
          </DialogHeader>
          <div className="space-y-3">
            <Label>دلیل رد</Label>
            <Textarea
              value={rejectTarget.reason}
              onChange={(e) => setRejectTarget({ ...rejectTarget, reason: e.target.value })}
              placeholder="دلیل رد را بنویسید..."
              rows={3}
            />
            <Button variant="destructive" className="w-full" onClick={handleReject}>
              رد {rejectTarget.type === "project" ? "پروژه" : "تسک"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
