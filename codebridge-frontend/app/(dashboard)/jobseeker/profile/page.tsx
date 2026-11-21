"use client";

import { format } from "date-fns";
import { faIR } from "date-fns/locale";
import { Star } from "lucide-react";
import { useAuth } from "@/lib/hooks/useAuth";
import { useDeveloperLevel } from "@/lib/hooks/useDeveloperLevels";
import { useUserReviews } from "@/lib/hooks/useReviews";
import { getName, LEVEL_LABELS } from "@/lib/constants";
import { StarRating } from "@/components/shared/StarRating";
import { AchievementsList } from "@/components/profile/AchievementsList";
import { ProfileSettings } from "@/components/profile/ProfileSettings";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { EmptyState } from "@/components/shared/EmptyState";

function ReviewItem({
  rating,
  reviewerName,
  projectTitle,
  comment,
  createdAt
}: {
  rating: number;
  reviewerName?: string | null;
  projectTitle?: string | null;
  comment?: string | null;
  createdAt: string;
}) {
  return (
    <div className="rounded-lg border p-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <span className="text-sm font-medium">{reviewerName ?? "کارفرما"}</span>
          {projectTitle && (
            <Badge variant="outline" className="text-[10px]">
              {projectTitle}
            </Badge>
          )}
        </div>
        <StarRating value={rating} size="sm" />
      </div>

      {comment && <p className="mt-2 text-sm text-muted-foreground">{comment}</p>}

      <p className="mt-2 text-xs text-muted-foreground" dir="ltr">
        {format(new Date(createdAt), "yyyy/MM/dd", { locale: faIR })}
      </p>
    </div>
  );
}

export default function JobSeekerProfilePage() {
  const { user } = useAuth();
  const { data: level, isLoading: levelLoading } = useDeveloperLevel(user?.id);
  const { data: reviews, isLoading: reviewsLoading } = useUserReviews(user?.id);

  const average = level?.averageRating ?? 0;
  const total = reviews?.length ?? 0;

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-lg font-semibold">{getName(user ?? {})}</h2>
        <p className="text-sm text-muted-foreground">
          اطلاعات شخصی، سطح برنامه‌نویسی و نظرات دریافتی
        </p>
      </div>

      <ProfileSettings mode="avatar" />

      <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
        <Card>
          <CardContent className="p-4">
            <p className="text-sm text-muted-foreground">میانگین امتیاز</p>
            {levelLoading ? (
              <Skeleton className="mt-2 h-6 w-32" />
            ) : (
              <div className="mt-2 flex items-center gap-2">
                <StarRating value={average} size="sm" showValue />
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-4">
            <p className="text-sm text-muted-foreground">تعداد نظرات</p>
            <p className="mt-2 text-xl font-semibold">
              {total.toLocaleString("fa-IR")}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-4">
            <p className="text-sm text-muted-foreground">سطح و امتیاز</p>
            {levelLoading ? (
              <Skeleton className="mt-2 h-6 w-32" />
            ) : (
              <div className="mt-2 flex items-center gap-2">
                <Badge variant="secondary">{LEVEL_LABELS[level?.level ?? 0]}</Badge>
                <span className="text-sm text-muted-foreground">
                  {level?.score.toLocaleString("fa-IR") ?? 0} امتیاز
                </span>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      <AchievementsList userId={user?.id} />

      <Card>
        <CardHeader>
          <CardTitle className="text-base">نظرات دریافت‌شده</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          {reviewsLoading ? (
            <Skeleton className="h-20 w-full" />
          ) : !reviews || reviews.length === 0 ? (
            <EmptyState
              icon={Star}
              title="هنوز نظری دریافت نکرده‌اید"
              description="پس از تکمیل پروژه‌ها، نظرات کارفرمایان اینجا نمایش داده می‌شود."
            />
          ) : (
            reviews.map((review) => (
              <ReviewItem
                key={review.id}
                rating={review.rating}
                reviewerName={review.reviewerName}
                projectTitle={review.projectTitle}
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
