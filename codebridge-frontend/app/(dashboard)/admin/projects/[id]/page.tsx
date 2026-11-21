"use client";

import { useState } from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  ArrowRight,
  Check,
  CheckCircle2,
  Globe,
  EyeOff,
  MessagesSquare,
  Plus,
  Send,
  X
} from "lucide-react";
import { toast } from "sonner";
import { getErrorMessage } from "@/lib/api/axios";
import { useAuth } from "@/lib/hooks/useAuth";
import {
  useAdminProjects,
  useApproveProject,
  useAssignDeveloper,
  useProject,
  useProjectMembers,
  usePublishProject,
  useRejectProject
} from "@/lib/hooks/useProjects";
import { useUsers } from "@/lib/hooks/useUsers";
import { useConversations, useCreateConversation } from "@/lib/hooks/useMessaging";
import { ConversationType } from "@/types/message";
import { ProjectStatus, type Project } from "@/types/project";
import { Role } from "@/types/auth";
import {
  isLevelSufficient,
  LEVEL_LABELS,
  PROJECT_STATUS_CLASSES,
  PROJECT_STATUS_LABELS
} from "@/lib/constants";
import { rejectProjectSchema, type RejectProjectValues } from "@/lib/validators/project";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Textarea } from "@/components/ui/textarea";
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
  DialogFooter,
  DialogHeader,
  DialogTitle
} from "@/components/ui/dialog";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage
} from "@/components/ui/form";
import { ProjectMembers } from "@/components/projects/ProjectMembers";
import { ProjectContractPanel } from "@/components/projects/ProjectContractPanel";
import { EmptyState } from "@/components/shared/EmptyState";

export default function AdminProjectDetailPage() {
  const params = useParams<{ id: string }>();
  const projectId = Number(params.id);
  const router = useRouter();
  const { user } = useAuth();

  const [openReject, setOpenReject] = useState(false);
  const [developerId, setDeveloperId] = useState<string>("");

  const { data: project, isLoading } = useProject(projectId);
  // بک‌اند قانون سطح حداقلی را اجرا می‌کند؛ لیست هم با همان قانون فیلتر می‌شود
  const { data: allJobSeekers = [] } = useUsers(Role.JobSeeker, Boolean(project));
  const developers = allJobSeekers.filter(
    (candidate) =>
      project &&
      typeof candidate.level === "number" &&
      isLevelSufficient(project.requiredLevel, candidate.level)
  );
  const { data: conversations = [] } = useConversations();
  const { data: members = [] } = useProjectMembers(projectId);

  const invalidate = () => {
    // re-fetch project list after actions
  };

  const approveProject = useApproveProject();
  const rejectProject = useRejectProject();
  const publishProject = usePublishProject();
  const assignDeveloper = useAssignDeveloper();
  const createConversation = useCreateConversation();

  const group = conversations.find(
    (conversation) =>
      conversation.type === ConversationType.ProjectGroup &&
      conversation.projectId === projectId
  );

  const rejectForm = useForm<RejectProjectValues>({
    resolver: zodResolver(rejectProjectSchema),
    defaultValues: { reason: "" }
  });

  if (isLoading) {
    return (
      <div className="space-y-3">
        <Skeleton className="h-8 w-1/3" />
        <Skeleton className="h-40 w-full" />
        <Skeleton className="h-40 w-full" />
      </div>
    );
  }

  if (!project) {
    return (
      <EmptyState
        icon={CheckCircle2}
        title="پروژه پیدا نشد"
        description="ممکن است حذف شده باشد."
        action={
          <Button asChild variant="outline">
            <Link href="/admin/projects">
              <ArrowRight className="h-4 w-4" />
              بازگشت به فهرست
            </Link>
          </Button>
        }
      />
    );
  }

  const isPending = project.status === ProjectStatus.Pending;
  const canAssign = project.status === ProjectStatus.Approved || project.status === ProjectStatus.InProgress;
  const canPublish = project.status === ProjectStatus.Approved && !project.isPublic;

  async function handleApprove() {
    try {
      await approveProject.mutateAsync(projectId);
      toast.success("پروژه تایید شد.");
    } catch (error) {
      toast.error(getErrorMessage(error, "تایید پروژه ناموفق بود."));
    }
  }

  async function handlePublish() {
    try {
      await publishProject.mutateAsync(projectId);
      toast.success(
        project && (project.isPublic ? "پروژه از انتشار خارج شد." : "پروژه منتشر شد.")
      );
    } catch (error) {
      toast.error(getErrorMessage(error, "انتشار پروژه ناموفق بود."));
    }
  }

  async function handleReject(values: RejectProjectValues) {
    try {
      await rejectProject.mutateAsync({ projectId, reason: values.reason });
      toast.success("پروژه رد شد.");
      setOpenReject(false);
      rejectForm.reset({ reason: "" });
    } catch (error) {
      toast.error(getErrorMessage(error, "رد پروژه ناموفق بود."));
    }
  }

  async function handleAssign() {
    if (!developerId) {
      toast.info("یک برنامه‌نویس انتخاب کنید.");
      return;
    }
    try {
      await assignDeveloper.mutateAsync({
        projectId,
        developerId: Number(developerId)
      });
      toast.success("برنامه‌نویس به پروژه تخصیص یافت.");
      setDeveloperId("");
    } catch (error) {
      toast.error(getErrorMessage(error, "تخصیص برنامه‌نویس ناموفق بود."));
    }
  }

  async function handleCreateGroup() {
    try {
      await createConversation.mutateAsync({
        type: ConversationType.ProjectGroup,
        projectId
      });
      toast.success("گروه گفت‌وگوی پروژه ساخته شد.");
    } catch (error) {
      toast.error(getErrorMessage(error, "ساخت گروه ناموفق بود."));
    }
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <Button asChild variant="ghost" size="sm" className="mb-2">
            <Link href="/admin/projects">
              <ArrowRight className="h-4 w-4" />
              بازگشت به فهرست پروژه‌ها
            </Link>
          </Button>
          <div className="flex flex-wrap items-center gap-2">
            <h2 className="text-lg font-semibold">{project.title}</h2>
            <Badge className={PROJECT_STATUS_CLASSES[project.status]}>
              {PROJECT_STATUS_LABELS[project.status]}
            </Badge>
            <Badge variant="outline">{LEVEL_LABELS[project.requiredLevel]}</Badge>
            {project.isPublic ? (
              <Badge variant="secondary" className="text-emerald-600">
                <Globe className="me-1 h-3 w-3" />
                منتشرشده
              </Badge>
            ) : (
              <Badge variant="outline" className="text-muted-foreground">
                <EyeOff className="me-1 h-3 w-3" />
                منتشرنشده
              </Badge>
            )}
          </div>
          <p className="mt-1 text-sm text-muted-foreground" dir="ltr">
            شناسه‌ی پروژه: #{project.id}
          </p>
        </div>

        <div className="flex flex-wrap gap-2">
          {isPending && (
            <>
              <Button
                onClick={handleApprove}
                disabled={approveProject.isPending}
              >
                <Check className="h-4 w-4" />
                تایید پروژه
              </Button>
              <Button
                variant="destructive"
                onClick={() => {
                  rejectForm.reset({ reason: "" });
                  setOpenReject(true);
                }}
              >
                <X className="h-4 w-4" />
                رد پروژه
              </Button>
            </>
          )}

          {/* backend فقط انتشار دارد؛ دکمه فقط وقتی پروژه منتشر نشده باشد نمایش داده می‌شود */}
          {canPublish && (
            <Button
              onClick={handlePublish}
              disabled={publishProject.isPending}
            >
              <Globe className="h-4 w-4" />
              انتشار پروژه
            </Button>
          )}
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        {/* Project Info */}
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle className="text-base">اطلاعات پروژه</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4 text-sm">
            <p className="whitespace-pre-wrap break-anywhere text-muted-foreground">
              {project.description}
            </p>

            <dl className="grid grid-cols-2 gap-4 sm:grid-cols-3">
              <div>
                <dt className="text-xs text-muted-foreground">بودجه</dt>
                <dd className="font-semibold" dir="ltr">
                  {project.budget.toLocaleString("fa-IR")} تومان
                </dd>
              </div>
              <div>
                <dt className="text-xs text-muted-foreground">کارفرما</dt>
                <dd className="font-semibold">{project.employerName ?? "—"}</dd>
              </div>
              <div>
                <dt className="text-xs text-muted-foreground">برنامه‌نویس</dt>
                <dd className="font-semibold">
                  {project.assignedDeveloperName ?? "تخصیص نیافته"}
                </dd>
              </div>
              <div>
                <dt className="text-xs text-muted-foreground">سطح موردنیاز</dt>
                <dd className="font-semibold">{LEVEL_LABELS[project.requiredLevel]}</dd>
              </div>
              <div>
                <dt className="text-xs text-muted-foreground">تاریخ ثبت</dt>
                <dd className="font-semibold" dir="ltr">
                  {new Date(project.createdAt).toLocaleDateString("fa-IR")}
                </dd>
              </div>
              {project.publishedAt && (
                <div>
                  <dt className="text-xs text-muted-foreground">تاریخ انتشار</dt>
                  <dd className="font-semibold" dir="ltr">
                    {new Date(project.publishedAt).toLocaleDateString("fa-IR")}
                  </dd>
                </div>
              )}
            </dl>

            {project.adminNote && (
              <div className="rounded-lg border border-amber-200 bg-amber-50 p-3 dark:border-amber-800 dark:bg-amber-950">
                <p className="text-xs font-medium text-amber-700 dark:text-amber-400">
                  یادداشت ادمین (دلیل رد)
                </p>
                <p className="mt-1 text-sm text-amber-800 dark:text-amber-300">
                  {project.adminNote}
                </p>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Assign Developer + Chat Group */}
        <Card>
          <CardHeader>
            <CardTitle className="text-base">تخصیص برنامه‌نویس</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <Select value={developerId} onValueChange={setDeveloperId}>
              <SelectTrigger>
                <SelectValue placeholder="انتخاب برنامه‌نویس" />
              </SelectTrigger>
              <SelectContent>
                {developers.map((developer) => (
                  <SelectItem key={developer.id} value={String(developer.id)}>
                    {developer.firstName} {developer.lastName} —{" "}
                    {LEVEL_LABELS[developer.level ?? 0]}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <p className="text-xs text-muted-foreground">
              فقط کارجویانی نمایش داده می‌شوند که سطحشان برای پروژه با سطح{" "}
              {LEVEL_LABELS[project.requiredLevel]} کافی باشد.
            </p>

            <Button
              onClick={handleAssign}
              disabled={!canAssign || !developerId || assignDeveloper.isPending}
            >
              تخصیص به پروژه
            </Button>

            <div className="border-t pt-4">
              {group ? (
                <Button asChild variant="outline">
                  <Link href={`/jobseeker/projects/${project.id}/chat`}>
                    <MessagesSquare className="h-4 w-4" />
                    ورود به گروه گفت‌وگو
                  </Link>
                </Button>
              ) : (
                <Button
                  variant="outline"
                  onClick={handleCreateGroup}
                  disabled={createConversation.isPending}
                >
                  <Plus className="h-4 w-4" />
                  ساخت گروه گفت‌وگوی پروژه
                </Button>
              )}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Contract & Settlement */}
      <ProjectContractPanel project={project} role={Role.Admin} />

      {/* Members */}
      <ProjectMembers projectId={projectId} canManage />

      {/* Reject Dialog */}
      <Dialog open={openReject} onOpenChange={setOpenReject}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>رد پروژه</DialogTitle>
            <DialogDescription>دلیل رد پروژه‌ی «{project.title}» برای کارفرما نمایش داده می‌شود.</DialogDescription>
          </DialogHeader>

          <Form {...rejectForm}>
            <form onSubmit={rejectForm.handleSubmit(handleReject)} className="space-y-4">
              <FormField
                control={rejectForm.control}
                name="reason"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>دلیل رد</FormLabel>
                    <FormControl>
                      <Textarea rows={4} placeholder="شرح دلیل رد..." {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <DialogFooter>
                <Button type="submit" variant="destructive" disabled={rejectProject.isPending}>
                  ثبت رد پروژه
                </Button>
              </DialogFooter>
            </form>
          </Form>
        </DialogContent>
      </Dialog>
    </div>
  );
}