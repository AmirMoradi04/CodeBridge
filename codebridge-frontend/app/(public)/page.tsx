"use client";

import { MessageSquare, Newspaper } from "lucide-react";
import { useFeed } from "@/lib/hooks/usePosts";
import { PostCard } from "@/components/posts/PostCard";
import { LiveActivityFeed } from "@/components/home/LiveActivityFeed";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { EmptyState } from "@/components/shared/EmptyState";

export default function HomePage() {
  const { data, isLoading, hasNextPage, fetchNextPage, isFetchingNextPage } = useFeed();
  const posts = data?.pages.flatMap((page) => page.items) ?? [];
  const total = data?.pages[0]?.totalCount ?? 0;

  return (
    <div className="mx-auto w-full max-w-5xl space-y-6">
      <header className="space-y-2">
        <h1 className="text-2xl font-bold">فید عمومی پست‌ها</h1>
        <p className="text-sm text-muted-foreground">
          آخرین نوشته‌های کارجویان و کارفرمایان CodeBridge
          {total > 0 ? ` — ${total.toLocaleString("fa-IR")} پست` : ""}
        </p>
      </header>

      <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_320px]">
        <div className="space-y-4">
          {isLoading ? (
            Array.from({ length: 3 }).map((_, index) => (
              <div key={index} className="space-y-3 rounded-xl border p-4">
                <div className="flex items-center gap-3">
                  <Skeleton className="h-9 w-9 rounded-full" />
                  <Skeleton className="h-4 w-40" />
                </div>
                <Skeleton className="h-4 w-full" />
                <Skeleton className="h-4 w-5/6" />
              </div>
            ))
          ) : posts.length === 0 ? (
            <EmptyState
              icon={Newspaper}
              title="هنوز پستی منتشر نشده"
              description="به‌محض انتشار اولین پست اینجا نمایش داده می‌شود."
            />
          ) : (
            posts.map((post) => <PostCard key={post.id} post={post} />)
          )}

          {hasNextPage && (
            <Button
              variant="outline"
              className="w-full"
              onClick={() => fetchNextPage()}
              disabled={isFetchingNextPage}
            >
              <MessageSquare className="h-4 w-4" />
              {isFetchingNextPage ? "در حال بارگذاری..." : "نمایش پست‌های بیشتر"}
            </Button>
          )}
        </div>

        <aside className="space-y-4 lg:sticky lg:top-20 lg:self-start">
          <LiveActivityFeed />
        </aside>
      </div>
    </div>
  );
}