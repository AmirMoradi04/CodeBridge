import { z } from "zod";

export const resumeSchema = z.object({
  title: z
    .string()
    .min(3, "عنوان رزومه باید حداقل ۳ کاراکتر باشد.")
    .max(200, "عنوان رزومه طولانی‌تر از حد مجاز است."),
  summary: z
    .string()
    .min(10, "خلاصه‌ی رزومه باید حداقل ۱۰ کاراکتر باشد.")
    .max(2000, "خلاصه‌ی رزومه طولانی‌تر از حد مجاز است."),
  skills: z
    .string()
    .min(2, "حداقل یک مهارت وارد کنید.")
    .max(1000, "فهرست مهارت‌ها طولانی‌تر از حد مجاز است.")
});

export type ResumeValues = z.infer<typeof resumeSchema>;
