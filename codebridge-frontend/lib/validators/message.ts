import { z } from "zod";

export const messageSchema = z.object({
  content: z
    .string()
    .trim()
    .min(1, "متن پیام الزامی است.")
    .max(2000, "متن پیام طولانی‌تر از حد مجاز است.")
});

export type MessageValues = z.infer<typeof messageSchema>;
