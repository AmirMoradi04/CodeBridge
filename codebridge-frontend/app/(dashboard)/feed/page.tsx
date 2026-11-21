"use client";

import { Fragment, useState } from "react";
import { Loader2, Newspaper, Plus } from "lucide-react";
import { useFeed } from "@/lib/hooks/usePosts";
import { PostCard } from "@/components/posts/PostCard";
import { PostForm } from "@/components/posts/PostForm";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger
} from "@/components/ui/dialog";
import { EmptyState } from "@/components/shared/EmptyState";
import { useAuth } from "@/lib/hooks/useAuth";
import { Role } from "@/types/auth";

export default function FeedPage() {
  const { user, isAuthenticated } = useAuth();
  const [open, setOpen] = useState(false);
  const { data, fetchNextPage, hasNextPage, isFetchingNextPage, isLoading } = useFeed();

  const posts = data?.pages.flatMap((page) => page.items) ?? [];
  const canPost = isAuthenticated && (user?.role === Role.JobSeeker || user?.role === Role.Employer);

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold">صفحه اصلی و پست‌ها</h2>
          <p className="text-sm text-muted-foreground">
            پست‌های روزمره کاربران CodeBridge
          </p>
        </div>

        {canPost && (
          <Dialog open={open} onOpenChange={setOpen}>
            <DialogTrigger asChild>
              <Button>
                <Plus className="h-4 w-4" />
                ساخت پست
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>پست جدید</DialogTitle>
                <DialogDescription>
                  پست پس از ثبت به‌صورت پیش‌نویس می‌ماند و باید منتشر شود.
                </DialogDescription>
              </DialogHeader>
              <PostForm onSuccess={() => setOpen(false)} />
            </DialogContent>
          </Dialog>
        )}
      </div>

      <section className="space-y-4">
        {isLoading ? (
          <div className="space-y-3">
            <Skeleton className="h-40 w-full" />
            <Skeleton className="h-40 w-full" />
            <Skeleton className="h-40 w-full" />
          </div>
        ) : posts.length === 0 ? (
          <EmptyState
            icon={Newspaper}
            title="هنوز پستی منتشر نشده است"
            description="اولین نفری باشید که پست می‌گذارد!"
          />
        ) : (
          <>
            {posts.map((post) => (
              <PostCard key={post.id} post={post} />
            ))}

            {hasNextPage && (
              <div className="flex justify-center py-4">
                <Button
                  variant="outline"
                  onClick={() => fetchNextPage()}
                  disabled={isFetchingNextPage}
                >
                  {isFetchingNextPage ? (
                    <>
                      <Loader2 className="h-4 w-4 animate-spin" />
                      در حال بارگذاری...
                    </>
                  ) : (
                    "بارگذاری بیشتر"
                  )}
                </Button>
              </div>
            )}
          </>
        )}
      </section>
    </div>
  );
}
