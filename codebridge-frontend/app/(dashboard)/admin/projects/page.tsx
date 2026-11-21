"use client";

import { useState } from "react";
import Link from "next/link";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import type { ColumnDef } from "@tanstack/react-table";
import {
  Briefcase,
  Check,
  CheckCircle2,
  Eye,
  Globe,
  EyeOff,
  UserPlus,
  X
} from "lucide-react";
import { toast } from "sonner";
import { getErrorMessage } from "@/lib/api/axios";
import {
  useAdminProjects,
  useApproveProject,
  useAssignDeveloper,
  usePublishProject,
  useRejectProject
} from "@/lib/hooks/useProjects";
import { useUsers } from "@/lib/hooks/useUsers";
import {
  isLevelSufficient,
  LEVEL_LABELS,
  PROJECT_STATUS_CLASSES,
  PROJECT_STATUS_LABELS
} from "@/lib/constants";
import { ProjectStatus, type Project } from "@/types/project";
import { Role } from "@/types/auth";
import { rejectProjectSchema, type RejectProjectValues } from "@/lib/validators/project";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
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
import { Input } from "@/components/ui/input";
import { DataTable } from "@/components/shared/DataTable";
import { EmptyState } from "@/components/shared/EmptyState";
import { Skeleton } from "@/components/ui/skeleton";

type StatusTab = "all" | ProjectStatus;

const STATUS_TABS: { value: StatusTab; label: string }[] = [
  { value: "all", label: "همه" },
  { value: ProjectStatus.Pending, label: PROJECT_STATUS_LABELS[ProjectStatus.Pending] },
  { value: ProjectStatus.Approved, label: PROJECT_STATUS_LABELS[ProjectStatus.Approved] },
  { value: ProjectStatus.InProgress, label: PROJECT_STATUS_LABELS[ProjectStatus.InProgress] },
  { value: ProjectStatus.Completed, label: PROJECT_STATUS_LABELS[ProjectStatus.Completed] }
];

export default function AdminProjectsPage() {
  const [statusTab, setStatusTab] = useState<StatusTab>("all");
  const [rejecting, setRejecting] = useState<Project | null>(null);
  const [assigning, setAssigning] = useState<Project | null>(null);
  const [developerQuery, setDeveloperQuery] = useState("");

  const projectsQuery = useAdminProjects(
    statusTab === "all" ? undefined : statusTab
  );

  const approveProject = useApproveProject();
  const rejectProject = useRejectProject();
  const publishProject = usePublishProject();
  const assignDeveloper = useAssignDeveloper();

  const form = useForm<RejectProjectValues>({
    resolver: zodResolver(rejectProjectSchema),
    defaultValues: { reason: "" }
  });

  const assignForm = useForm<{ developerId: number }>({
    defaultValues: { developerId: 0 }
  });

  // فهرست کارجویان (برنامه‌نویسان) — فعال هنگام باز بودن دیالوگ تخصیص
  const { data: developers = [], isLoading: developersLoading } = useUsers(
    Role.JobSeeker,
    assigning !== null
  );

  // فقط کارجویانی که سطحشان برای پروژه کافی باشد نمایش داده می‌شوند
  const eligibleDevelopers = developers.filter((dev) => {
    const level = dev.level ?? 0;
    if (!assigning) return true;
    return isLevelSufficient(assigning.requiredLevel, level);
  });

  const developerResults = eligibleDevelopers.filter((dev) => {
    const term = developerQuery.trim().toLowerCase();
    if (!term) return true;
    return (
      dev.email.toLowerCase().includes(term) ||
      `${dev.firstName} ${dev.lastName}`.toLowerCase().includes(term)
    );
  });

  async function handleApprove(projectId: number) {
    try {
      await approveProject.mutateAsync(projectId);
      toast.success("پروژه تایید شد.");
    } catch (error) {
      toast.error(getErrorMessage(error, "تایید پروژه ناموفق بود."));
    }
  }

  async function handleReject(values: RejectProjectValues) {
    if (!rejecting) return;
    try {
      await rejectProject.mutateAsync({ projectId: rejecting.id, reason: values.reason });
      toast.success("پروژه رد شد.");
      setRejecting(null);
      form.reset({ reason: "" });
    } catch (error) {
      toast.error(getErrorMessage(error, "رد پروژه ناموفق بود."));
    }
  }

  async function handleAssign(values: { developerId: number }) {
    if (!assigning) return;
    try {
      await assignDeveloper.mutateAsync({ projectId: assigning.id, developerId: values.developerId });
      toast.success("برنامه‌نویس به پروژه تخصیص یافت.");
      setAssigning(null);
      assignForm.reset({ developerId: 0 });
      setDeveloperQuery("");
    } catch (error) {
      toast.error(getErrorMessage(error, "تخصیص برنامه‌نویس ناموفق بود."));
    }
  }

  async function handlePublish(projectId: number) {
    try {
      await publishProject.mutateAsync(projectId);
      toast.success("پروژه منتشر شد.");
    } catch (error) {
      toast.error(getErrorMessage(error, "انتشار پروژه ناموفق بود."));
    }
  }

  const columns: ColumnDef<Project>[] = [
    {
      accessorKey: "id",
      header: "شناسه",
      cell: ({ row }) => (
        <span className="font-mono text-xs text-muted-foreground" dir="ltr">
          #{row.original.id}
        </span>
      )
    },
    {
      accessorKey: "title",
      header: "عنوان",
      cell: ({ row }) => (
        <div className="min-w-0">
          <p className="truncate font-medium">{row.original.title}</p>
          <p className="truncate text-xs text-muted-foreground">
            {row.original.employerName ?? "کارفرما"}
          </p>
        </div>
      )
    },
    {
      accessorKey: "budget",
      header: "بودجه",
      cell: ({ row }) => (
        <span dir="ltr">{row.original.budget.toLocaleString("fa-IR")} تومان</span>
      )
    },
    {
      accessorKey: "requiredLevel",
      header: "سطح موردنیاز",
      cell: ({ row }) => (
        <span className="text-sm">{LEVEL_LABELS[row.original.requiredLevel]}</span>
      )
    },
    {
      accessorKey: "status",
      header: "وضعیت",
      cell: ({ row }) => (
        <Badge className={PROJECT_STATUS_CLASSES[row.original.status]}>
          {PROJECT_STATUS_LABELS[row.original.status]}
        </Badge>
      )
    },
    {
      accessorKey: "isPublic",
      header: "منتشرشده",
      cell: ({ row }) =>
        row.original.isPublic ? (
          <CheckCircle2 className="h-4 w-4 text-emerald-600" />
        ) : (
          <X className="h-4 w-4 text-muted-foreground" />
        )
    },
    {
      id: "actions",
      header: "عملیات",
      cell: ({ row }) => {
        const project = row.original;
        const isPending = project.status === ProjectStatus.Pending;

        return (
          <div className="flex flex-wrap items-center gap-1.5">
            {isPending && (
              <>
                <Button
                  size="sm"
                  variant="outline"
                  disabled={approveProject.isPending}
                  onClick={() => handleApprove(project.id)}
                >
                  <Check className="h-3.5 w-3.5" />
                  تایید
                </Button>

                <Button
                  size="sm"
                  variant="destructive"
                  onClick={() => {
                    form.reset({ reason: "" });
                    setRejecting(project);
                  }}
                >
                  <X className="h-3.5 w-3.5" />
                  رد
                </Button>
              </>
            )}

            <Button
              size="sm"
              variant="ghost"
              disabled={
                !isPending ||
                project.assignedDeveloperId != null ||
                project.isPublic
              }
              onClick={() => setAssigning(project)}
              title="تخصیص برنامه‌نویس"
            >
              <UserPlus className="h-3.5 w-3.5" />
            </Button>

            {!project.isPublic && (
              <Button
                size="sm"
                variant="ghost"
                disabled={publishProject.isPending}
                onClick={() => handlePublish(project.id)}
                title="انتشار پروژه"
              >
                <Globe className="h-3.5 w-3.5" />
              </Button>
            )}

            {project.isPublic && (
              <Button
                size="sm"
                variant="ghost"
                title="عدم انتشار"
                disabled={publishProject.isPending}
                onClick={() => handlePublish(project.id)}
              >
                <EyeOff className="h-3.5 w-3.5" />
              </Button>
            )}

            <Button asChild size="sm" variant="ghost">
              <Link href={`/admin/projects/${project.id}`}>
                <Eye className="h-3.5 w-3.5" />
                جزئیات
              </Link>
            </Button>
          </div>
        );
      }
    }
  ];

  const projects = projectsQuery.data ?? [];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold">مدیریت پروژه‌ها</h2>
          <p className="text-sm text-muted-foreground">
            تایید، رد، تخصیص برنامه‌نویس و انتشار پروژه‌ها
          </p>
        </div>
      </div>

      <Tabs value={String(statusTab)} onValueChange={(v) => setStatusTab(v as StatusTab)}>
        <TabsList>
          {STATUS_TABS.map((tab) => (
            <TabsTrigger key={tab.value} value={String(tab.value)}>
              {tab.label}
            </TabsTrigger>
          ))}
        </TabsList>
      </Tabs>

      {projectsQuery.isLoading ? (
        <div className="space-y-2">
          <Skeleton className="h-12 w-full" />
          <Skeleton className="h-12 w-full" />
          <Skeleton className="h-12 w-full" />
        </div>
      ) : projects.length === 0 ? (
        <EmptyState
          icon={Briefcase}
          title="پروژه‌ای با این فیلتر پیدا نشد"
          description="فیلتر وضعیت را تغییر دهید."
        />
      ) : (
        <DataTable columns={columns} data={projects} emptyMessage="پروژه‌ای وجود ندارد." />
      )}

      {/* Reject Dialog */}
      <Dialog open={rejecting !== null} onOpenChange={(open) => !open && setRejecting(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>رد پروژه</DialogTitle>
            <DialogDescription>
              دلیل رد پروژه‌ی «{rejecting?.title}» برای کارفرما نمایش داده می‌شود.
            </DialogDescription>
          </DialogHeader>

          <Form {...form}>
            <form onSubmit={form.handleSubmit(handleReject)} className="space-y-4">
              <FormField
                control={form.control}
                name="reason"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>دلیل رد</FormLabel>
                    <FormControl>
                      <Textarea
                        rows={4}
                        placeholder="توضیح دهید چرا پروژه رد شد..."
                        {...field}
                      />
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

      {/* Assign Developer Dialog */}
      <Dialog
        open={assigning !== null}
        onOpenChange={(open) => {
          if (!open) {
            setAssigning(null);
            assignForm.reset({ developerId: 0 });
            setDeveloperQuery("");
          }
        }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>تخصیص برنامه‌نویس</DialogTitle>
            <DialogDescription>
              برنامه‌نویس مورد نظر را برای پروژه‌ی «{assigning?.title}» انتخاب کنید.
            </DialogDescription>
          </DialogHeader>

          <div className="relative">
            <Input
              value={developerQuery}
              onChange={(event) => setDeveloperQuery(event.target.value)}
              placeholder="جستجوی ایمیل یا نام برنامه‌نویس..."
            />
          </div>

          <div className="max-h-72 space-y-1 overflow-y-auto">
            {developersLoading ? (
              <div className="space-y-2 p-2">
                <Skeleton className="h-10 w-full" />
                <Skeleton className="h-10 w-full" />
              </div>
            ) : developerResults.length === 0 ? (
              <p className="p-4 text-center text-sm text-muted-foreground">
                برنامه‌نویسی پیدا نشد.
              </p>
            ) : (
              developerResults.map((dev) => (
                <button
                  key={dev.id}
                  type="button"
                  onClick={() => assignForm.setValue("developerId", dev.id)}
                  className={`flex w-full items-center justify-between gap-3 rounded-lg border p-3 text-right transition-colors hover:bg-accent ${
                    assignForm.watch("developerId") === dev.id
                      ? "border-primary bg-accent"
                      : ""
                  }`}
                >
                  <span>
                    <span className="block text-sm font-medium">
                      {dev.firstName} {dev.lastName}
                    </span>
                    <span className="block text-xs text-muted-foreground" dir="ltr">
                      {dev.email}
                    </span>
                  </span>
                  {assignForm.watch("developerId") === dev.id && (
                    <Check className="h-4 w-4 text-primary" />
                  )}
                </button>
              ))
            )}
          </div>

          <DialogFooter>
            <Button
              type="button"
              disabled={
                assignForm.watch("developerId") === 0 || assignDeveloper.isPending
              }
              onClick={assignForm.handleSubmit(handleAssign)}
            >
              تخصیص برنامه‌نویس
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}