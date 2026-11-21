"use client";

import { useState } from "react";
import { formatDistanceToNow } from "date-fns";
import { faIR } from "date-fns/locale";
import { Send, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { getErrorMessage } from "@/lib/api/axios";
import { useAuth } from "@/lib/hooks/useAuth";
import { useAddComment, useComments, useDeleteComment } from "@/lib/hooks/usePosts";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";

export function CommentList({ postId }: { postId: number }) {
  const { user, isAuthenticated } = useAuth();
  const { data: comments, isLoading } = useComments(postId);
  const addComment = useAddComment(postId);
  const deleteComment = useDeleteComment(postId);
  const [text, setText] = useState("");

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();

    const content = text.trim();
    if (!content) return;

    try {
      await addComment.mutateAsync({ postId, content });
      setText("");
    } catch (error) {
      toast.error(getErrorMessage(error, "ثبت دیدگاه ناموفق بود."));
    }
  }

  async function handleDelete(commentId: number) {
    if (!window.confirm("این دیدگاه حذف شود؟")) return;

    try {
      await deleteComment.mutateAsync(commentId);
      toast.success("دیدگاه حذف شد.");
    } catch (error) {
      toast.error(getErrorMessage(error, "حذف دیدگاه ناموفق بود."));
    }
  }

  return (
    <section className="mt-3 space-y-3 border-t pt-3">
      {isLoading ? (
        <div className="space-y-2">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-4/5" />
        </div>
      ) : comments && comments.length > 0 ? (
        comments.map((comment) => (
          <div key={comment.id} className="flex items-start gap-2">
            <div className="min-w-0 flex-1 rounded-lg bg-muted px-3 py-2">
              <p className="text-xs font-semibold">
                {comment.authorName ?? "کاربر"}
                <span className="ms-2 font-normal text-muted-foreground">
                  {formatDistanceToNow(new Date(comment.createdAt), {
                    addSuffix: true,
                    locale: faIR
                  })}
                </span>
              </p>
              <p className="mt-1 break-anywhere text-sm">{comment.content}</p>
            </div>

            {(user?.id === comment.authorId || user?.role === 0) && (
              <Button
                variant="ghost"
                size="icon"
                aria-label="حذف دیدگاه"
                onClick={() => handleDelete(comment.id)}
              >
                <Trash2 className="h-4 w-4 text-destructive" />
              </Button>
            )}
          </div>
        ))
      ) : (
        <p className="text-sm text-muted-foreground">هنوز دیدگاهی ثبت نشده است.</p>
      )}

      {isAuthenticated ? (
        <form onSubmit={handleSubmit} className="flex items-center gap-2">
          <Input
            value={text}
            onChange={(event) => setText(event.target.value)}
            placeholder="دیدگاه خود را بنویسید..."
            maxLength={2000}
          />
          <Button
            type="submit"
            size="icon"
            aria-label="ارسال دیدگاه"
            disabled={addComment.isPending || text.trim().length === 0}
          >
            <Send className="h-4 w-4" />
          </Button>
        </form>
      ) : (
        <p className="text-xs text-muted-foreground">
          برای نوشتن دیدگاه وارد حساب خود شوید.
        </p>
      )}
    </section>
  );
}