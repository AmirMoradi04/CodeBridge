import { z } from "zod";

export const postSchema = z.object({
  content: z
    .string()
    .min(5, "متن پست باید حداقل ۵ کاراکتر باشد.")
    .max(5000, "متن پست طولانی‌تر از حد مجاز است.")
});

export const commentSchema = z.object({
  content: z
    .string()
    .min(1, "متن دیدگاه الزامی است.")
    .max(2000, "متن دیدگاه طولانی‌تر از حد مجاز است.")
});

export type PostValues = z.infer<typeof postSchema>;
export type CommentValues = z.infer<typeof commentSchema>;
