"use client";

import { useState } from "react";
import Link from "next/link";
import { FileText, Heart, MessageCircle, MessageSquare, Plus, Newspaper } from "lucide-react";
import { useAuth } from "@/lib/hooks/useAuth";
import { useAuthorPosts } from "@/lib/hooks/usePosts";
import { PostCard } from "@/components/posts/PostCard";
import { PostForm } from "@/components/posts/PostForm";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
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

function StatCard({
  icon: Icon,
  label,
  value
}: {
  icon: React.ElementType;
  label: string;
  value: number;
}) {
  return (
    <Card>
      <CardContent className="flex items-center gap-3 p-4">
        <span className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10 text-primary">
          <Icon className="h-5 w-5" />
        </span>
        <div>
          <p className="text-xl font-bold">{value.toLocaleString("fa-IR")}</p>
          <p className="text-xs text-muted-foreground">{label}</p>
        </div>
      </CardContent>
    </Card>
  );
}

export default function JobSeekerDashboardPage() {
  const { user } = useAuth();
  const { data: posts, isLoading } = useAuthorPosts(user?.id);
  const [open, setOpen] = useState(false);

  // آمار از خود پست‌ها محاسبه می‌شود
  const totalLikes = (posts ?? []).reduce((sum, post) => sum + post.likesCount, 0);
  const totalComments = (posts ?? []).reduce((sum, post) => sum + post.commentsCount, 0);

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold">
            سلام {user?.firstName ?? user?.email}
          </h2>
          <p className="text-sm text-muted-foreground">پنل کارجوی CodeBridge</p>
        </div>

        <div className="flex gap-2">
          <Button asChild variant="outline">
            <Link href="/jobseeker/resume">
              <FileText className="h-4 w-4" />
              رزومه‌ی من
            </Link>
          </Button>
          <Button asChild variant="outline">
            <Link href="/jobseeker/chat">
              <MessageSquare className="h-4 w-4" />
              گفت‌وگو با ادمین
            </Link>
          </Button>
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
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <StatCard icon={Newspaper} label="پست‌های من" value={posts?.length ?? 0} />
        <StatCard icon={Heart} label="لایک دریافتی" value={totalLikes} />
        <StatCard icon={MessageCircle} label="دیدگاه دریافتی" value={totalComments} />
      </div>

      <section className="space-y-4">
        <h3 className="font-semibold">پست‌های من</h3>

        {isLoading ? (
          <div className="space-y-3">
            <Skeleton className="h-32 w-full" />
            <Skeleton className="h-32 w-full" />
          </div>
        ) : !posts || posts.length === 0 ? (
          <EmptyState
            icon={Newspaper}
            title="هنوز پستی نساخته‌اید"
            description="با دکمه‌ی «ساخت پست» اولین نوشته‌ی خود را ثبت کنید."
          />
        ) : (
          posts.map((post) => <PostCard key={post.id} post={post} />)
        )}
      </section>
    </div>
  );
}