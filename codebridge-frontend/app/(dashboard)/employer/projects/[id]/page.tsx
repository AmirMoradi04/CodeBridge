"use client";

import { useParams } from "next/navigation";
import Link from "next/link";
import { format } from "date-fns";
import { faIR } from "date-fns/locale";
import { ArrowRight, Briefcase, Calendar, Coins, UserRound } from "lucide-react";
import { useProject } from "@/lib/hooks/useProjects";
import { useProjectReviews } from "@/lib/hooks/useReviews";
import { useAuth } from "@/lib/hooks/useAuth";
import { Role } from "@/types/auth";
import { LEVEL_LABELS, PROJECT_STATUS_CLASSES, PROJECT_STATUS_LABELS } from "@/lib/constants";
import { ProjectStatus, type Project } from "@/types/project";
import { ProjectContractPanel } from "@/components/projects/ProjectContractPanel";
import { ReviewForm } from "@/components/projects/ReviewForm";
import { StarRating } from "@/components/shared/StarRating";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

function ReviewItem({
  rating,
  reviewerName,
  comment,
  createdAt
}: {
  rating: number;
  reviewerName?: string | null;
  comment?: string | null;
  createdAt: string;
}) {
  return (
    <div className="rounded-lg border p-3">
      <div className="flex items-center justify-between gap-2">
        <span className="text-sm font-medium">{reviewerName ?? "کارفرما"}</span>
        <StarRating value={rating} size="sm" />
      </div>
      {comment && <p className="mt-2 text-sm text-muted-foreground">{comment}</p>}
      <p className="mt-2 text-xs text-muted-foreground" dir="ltr">
        {format(new Date(createdAt), "yyyy/MM/dd", { locale: faIR })}
      </p>
    </div>
  );
}

export default function EmployerProjectDetailPage() {
  const params = useParams<{ id: string }>();
  const projectId = Number(params.id);
  const { user } = useAuth();

  const { data: project, isLoading, isError } = useProject(projectId);
  const { data: reviews, isLoading: reviewsLoading } = useProjectReviews(projectId);

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-56 w-full" />
      </div>
    );
  }

  if (isError || !project) {
    return (
      <p className="rounded-xl border border-destructive/40 p-4 text-sm text-destructive">
        دریافت پروژه ممکن نشد؛{" "}
        <Link href="/employer" className="font-medium underline">
          بازگشت به پنل
        </Link>
      </p>
    );
  }

  // فقط کارفرمای همین پروژه می‌تواند نظر ثبت کند و فقط بعد از تکمیل
  const isOwner = user?.id === project.employerId;
  const hasReviewed = (reviews ?? []).some((review) => review.reviewerId === user?.id);
  const canReview =
    isOwner &&
    project.status === ProjectStatus.Completed &&
    !hasReviewed &&
    Boolean(project.assignedDeveloperId);

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-3">
          <Button asChild variant="ghost" size="sm">
            <Link href="/employer">
              <ArrowRight className="h-4 w-4" />
              بازگشت
            </Link>
          </Button>
          <h2 className="text-lg font-semibold">جزئیات پروژه</h2>
        </div>

        {canReview && (
          <ReviewForm
            projectId={project.id}
            targetUserId={project.assignedDeveloperId as number}
            targetName={project.assignedDeveloperName}
          />
        )}
      </div>

      <Card>
        <CardHeader className="gap-3">
          <div className="flex items-start justify-between gap-3">
            <CardTitle className="text-base leading-6">{project.title}</CardTitle>
            <Badge className={PROJECT_STATUS_CLASSES[project.status]}>
              {PROJECT_STATUS_LABELS[project.status]}
            </Badge>
          </div>

          <div className="flex flex-wrap items-center gap-3 text-xs text-muted-foreground">
            <span className="inline-flex items-center gap-1">
              <Coins className="h-3.5 w-3.5" />
              {project.budget.toLocaleString("fa-IR")} تومان
            </span>
            <span className="inline-flex items-center gap-1">
              <Briefcase className="h-3.5 w-3.5" />
              {LEVEL_LABELS[project.requiredLevel]}
            </span>
            <span className="inline-flex items-center gap-1">
              <Calendar className="h-3.5 w-3.5" />
              {format(new Date(project.createdAt), "yyyy/MM/dd", { locale: faIR })}
            </span>
            <span className="inline-flex items-center gap-1">
              <UserRound className="h-3.5 w-3.5" />
              {project.assignedDeveloperName ?? "تخصیص نیافته"}
            </span>
          </div>
        </CardHeader>

        <CardContent className="space-y-4">
          <p className="whitespace-pre-line text-sm text-muted-foreground">{project.description}</p>

          <ProjectContractPanel project={project as Project} role={Role.Employer} embedded />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">نظرات این پروژه</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          {reviewsLoading ? (
            <Skeleton className="h-20 w-full" />
          ) : !reviews || reviews.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              برای این پروژه هنوز نظری ثبت نشده است.
            </p>
          ) : (
            reviews.map((review) => (
              <ReviewItem
                key={review.id}
                rating={review.rating}
                reviewerName={review.reviewerName}
                comment={review.comment}
                createdAt={review.createdAt}
              />
            ))
          )}
        </CardContent>
      </Card>
    </div>
  );
}
