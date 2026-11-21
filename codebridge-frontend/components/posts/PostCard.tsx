"use client";

import { useState } from "react";
import { formatDistanceToNow } from "date-fns";
import { faIR } from "date-fns/locale";
import { Heart, MessageCircle, MoreHorizontal, Send, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { cn } from "@/lib/utils";
import { getErrorMessage } from "@/lib/api/axios";
import { useAuth } from "@/lib/hooks/useAuth";
import {
  useHasLiked,
  useToggleLike
} from "@/lib/hooks/usePosts";
import { useDeletePost, usePublishPost } from "@/lib/hooks/usePosts";
import type { Post } from "@/types/post";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger
} from "@/components/ui/dropdown-menu";
import { CommentList } from "@/components/posts/CommentList";

export function PostCard({ post }: { post: Post }) {
  const { user, isAuthenticated } = useAuth();
  const hasLiked = useHasLiked(post.id, isAuthenticated);
  const toggleLike = useToggleLike(post.id);
  const deletePost = useDeletePost();
  const publishPost = usePublishPost();
  const [showComments, setShowComments] = useState(false);

  const isAuthor = user?.id === post.authorId;
  const isAdmin = user?.role === 0;
  const liked = Boolean(hasLiked.data);

  async function handleToggleLike() {
    if (!isAuthenticated) {
      toast.info("برای لایک کردن وارد حساب خود شوید.");
      return;
    }

    try {
      await toggleLike.mutateAsync();
    } catch (error) {
      toast.error(getErrorMessage(error, "لایک ثبت نشد."));
    }
  }

  async function handleDelete() {
    if (!window.confirm("این پست حذف شود؟")) return;

    try {
      await deletePost.mutateAsync(post.id);
      toast.success("پست حذف شد.");
    } catch (error) {
      toast.error(getErrorMessage(error, "حذف پست ناموفق بود."));
    }
  }

  async function handlePublish() {
    try {
      await publishPost.mutateAsync(post.id);
      toast.success("پست منتشر شد.");
    } catch (error) {
      toast.error(getErrorMessage(error, "انتشار پست ناموفق بود."));
    }
  }

  return (
    <article className="rounded-xl border bg-card p-4 shadow-sm">
      <header className="flex items-center gap-3">
        <Avatar>
          <AvatarFallback>
            {(post.authorName ?? "CB").slice(0, 2)}
          </AvatarFallback>
        </Avatar>

        <div className="min-w-0 flex-1">
          <p className="truncate text-sm font-semibold">{post.authorName ?? "کاربر"}</p>
          <p className="text-xs text-muted-foreground">
            {formatDistanceToNow(new Date(post.createdAt), {
              addSuffix: true,
              locale: faIR
            })}
          </p>
        </div>

        {!post.isPublished && <Badge variant="secondary">پیش‌نویس</Badge>}

        {(isAuthor || isAdmin) && (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon" aria-label="گزینه‌ها">
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              {isAuthor && !post.isPublished && (
                <DropdownMenuItem onClick={handlePublish} className="gap-2">
                  <Send className="h-4 w-4" />
                  انتشار
                </DropdownMenuItem>
              )}
              <DropdownMenuItem
                onClick={handleDelete}
                className="gap-2 text-destructive focus:text-destructive"
              >
                <Trash2 className="h-4 w-4" />
                حذف پست
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        )}
      </header>

      <p className="mt-3 whitespace-pre-wrap break-anywhere text-sm leading-7">
        {post.content}
      </p>

      <div className="mt-4 flex items-center gap-2 border-t pt-3">
        <Button
          variant="ghost"
          size="sm"
          onClick={handleToggleLike}
          disabled={toggleLike.isPending}
          className={cn(liked && "text-rose-500 hover:text-rose-500")}
        >
          <Heart className={cn("h-4 w-4", liked && "fill-current")} />
          {post.likesCount}
        </Button>

        <Button
          variant="ghost"
          size="sm"
          onClick={() => setShowComments((value) => !value)}
        >
          <MessageCircle className="h-4 w-4" />
          {post.commentsCount}
        </Button>
      </div>

      {showComments && <CommentList postId={post.id} />}
    </article>
  );
}