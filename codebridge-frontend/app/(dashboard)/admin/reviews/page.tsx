"use client";

import type { ColumnDef } from "@tanstack/react-table";
import { Star } from "lucide-react";
import { useAllReviews } from "@/lib/hooks/useReviews";
import type { Review } from "@/types/review";
import { StarRating } from "@/components/shared/StarRating";
import { Badge } from "@/components/ui/badge";
import { DataTable } from "@/components/shared/DataTable";
import { EmptyState } from "@/components/shared/EmptyState";

export default function AdminReviewsPage() {
  const { data: reviews, isLoading } = useAllReviews();

  const columns: ColumnDef<Review>[] = [
    {
      accessorKey: "projectTitle",
      header: "پروژه",
      cell: ({ row }) => (
        <div className="flex flex-col">
          <span className="text-sm">{row.original.projectTitle ?? `#${row.original.projectId}`}</span>
          <span className="text-xs text-muted-foreground" dir="ltr">
            #{row.original.id}
          </span>
        </div>
      )
    },
    {
      accessorKey: "reviewerName",
      header: "از طرف",
      cell: ({ row }) => (
        <div className="flex flex-col">
          <span className="text-sm">{row.original.reviewerName ?? `#${row.original.reviewerId}`}</span>
          <Badge variant="outline" className="w-fit text-[10px]">
            کارفرما
          </Badge>
        </div>
      )
    },
    {
      accessorKey: "targetUserName",
      header: "دریافت‌کننده",
      cell: ({ row }) => (
        <span className="text-sm">{row.original.targetUserName ?? `#${row.original.targetUserId}`}</span>
      )
    },
    {
      accessorKey: "rating",
      header: "امتیاز",
      cell: ({ row }) => <StarRating value={row.original.rating} size="sm" />
    },
    {
      accessorKey: "comment",
      header: "نظر",
      cell: ({ row }) => (
        <span className="block max-w-md truncate text-xs text-muted-foreground">
          {row.original.comment ?? "—"}
        </span>
      )
    },
    {
      accessorKey: "createdAt",
      header: "تاریخ",
      cell: ({ row }) => (
        <span className="whitespace-nowrap text-xs" dir="ltr">
          {new Date(row.original.createdAt).toLocaleString("fa-IR")}
        </span>
      )
    }
  ];

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-lg font-semibold">نظرات و امتیازها</h2>
        <p className="text-sm text-muted-foreground">
          همه‌ی امتیازهای ثبت‌شده توسط کارفرمایان برای برنامه‌نویسان و منتورها
        </p>
      </div>

      {isLoading ? (
        <p className="text-sm text-muted-foreground">در حال بارگذاری...</p>
      ) : !reviews || reviews.length === 0 ? (
        <EmptyState
          icon={Star}
          title="هنوز نظری ثبت نشده"
          description="پس از تکمیل پروژه‌ها و ثبت امتیاز توسط کارفرمایان، اینجا نمایش داده می‌شود."
        />
      ) : (
        <DataTable
          columns={columns}
          data={reviews}
          emptyMessage="نظری وجود ندارد."
          pageSize={15}
        />
      )}
    </div>
  );
}
