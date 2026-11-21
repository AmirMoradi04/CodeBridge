"use client";

import { useState } from "react";
import { Star } from "lucide-react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { getErrorMessage } from "@/lib/api/axios";
import { useSubmitReview } from "@/lib/hooks/useReviews";
import { StarRating } from "@/components/shared/StarRating";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger
} from "@/components/ui/dialog";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage
} from "@/components/ui/form";
import { Textarea } from "@/components/ui/textarea";

const reviewSchema = z.object({
  rating: z.coerce.number().min(1, "امتیاز ۱ تا ۵ را انتخاب کنید.").max(5),
  comment: z.string().max(2000, "نظر نباید بیش از ۲۰۰۰ کاراکتر باشد.").optional()
});

export type ReviewValues = z.infer<typeof reviewSchema>;

/** دیالوگ ثبت امتیاز کارفرما برای برنامه‌نویس/منتور پروژه‌ی تکمیل‌شده. */
export function ReviewForm({
  projectId,
  targetUserId,
  targetName,
  disabled = false
}: {
  projectId: number;
  targetUserId: number;
  targetName?: string | null;
  disabled?: boolean;
}) {
  const [open, setOpen] = useState(false);
  const submitReview = useSubmitReview();

  const form = useForm<ReviewValues>({
    resolver: zodResolver(reviewSchema),
    defaultValues: { rating: 0, comment: "" }
  });

  async function onSubmit(values: ReviewValues) {
    try {
      await submitReview.mutateAsync({
        projectId,
        targetUserId,
        rating: values.rating,
        comment: values.comment?.trim() || null
      });

      toast.success("امتیاز شما ثبت شد.");
      form.reset({ rating: 0, comment: "" });
      setOpen(false);
    } catch (error) {
      toast.error(getErrorMessage(error, "ثبت امتیاز انجام نشد."));
    }
  }

  if (disabled) return null;

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button>
          <Star className="h-4 w-4" />
          ثبت امتیاز و نظر
        </Button>
      </DialogTrigger>

      <DialogContent>
        <DialogHeader>
          <DialogTitle>امتیاز به {targetName ?? "برنامه‌نویس پروژه"}</DialogTitle>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-5">
            <FormField
              control={form.control}
              name="rating"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>امتیاز شما</FormLabel>
                  <FormControl>
                    <StarRating
                      value={field.value}
                      onChange={field.onChange}
                      size="md"
                      showValue
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="comment"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>نظر شما (اختیاری)</FormLabel>
                  <FormControl>
                    <Textarea
                      rows={4}
                      placeholder="کیفیت همکاری، تعهد به زمان و ارتباط را بنویسید…"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <Button
              type="submit"
              className="w-full"
              disabled={submitReview.isPending || form.watch("rating") < 1}
            >
              {submitReview.isPending ? "در حال ثبت..." : "ثبت امتیاز"}
            </Button>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
