"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { getErrorMessage } from "@/lib/api/axios";
import { useCreatePost } from "@/lib/hooks/usePosts";
import { postSchema, type PostValues } from "@/lib/validators/post";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage
} from "@/components/ui/form";

export function PostForm({ onSuccess }: { onSuccess: () => void }) {
  const createPost = useCreatePost();

  const form = useForm<PostValues>({
    resolver: zodResolver(postSchema),
    defaultValues: { content: "" }
  });

  async function onSubmit(values: PostValues) {
    try {
      await createPost.mutateAsync({ content: values.content });
      toast.success("پست ثبت شد؛ برای نمایش عمومی آن را منتشر کنید.");
      form.reset();
      onSuccess();
    } catch (error) {
      toast.error(getErrorMessage(error, "ثبت پست ناموفق بود."));
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="content"
          render={({ field }) => (
            <FormItem>
              <FormLabel>متن پست</FormLabel>
              <FormControl>
                <Textarea
                  rows={5}
                  placeholder="چه چیزی می‌خواهید بگویید؟"
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button type="submit" className="w-full" disabled={createPost.isPending}>
          {createPost.isPending ? "در حال ثبت..." : "ثبت پست"}
        </Button>
      </form>
    </Form>
  );
}