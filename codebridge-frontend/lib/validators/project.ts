import { z } from "zod";

export const projectSchema = z.object({
  title: z
    .string()
    .min(3, "عنوان پروژه باید حداقل ۳ کاراکتر باشد.")
    .max(200, "عنوان پروژه طولانی‌تر از حد مجاز است."),
  description: z
    .string()
    .min(10, "توضیحات باید حداقل ۱۰ کاراکتر باشد.")
    .max(5000, "توضیحات طولانی‌تر از حد مجاز است."),
  budget: z.coerce
    .number({ invalid_type_error: "بودجه باید عدد باشد." })
    .positive("بودجه باید بزرگ‌تر از صفر باشد."),
  requiredLevel: z.coerce
    .number()
    .int()
    .min(0, "سطح موردنیاز را انتخاب کنید.")
    .max(6, "سطح موردنیاز معتبر نیست.")
});

export const rejectProjectSchema = z.object({
  reason: z.string().min(3, "دلیل رد پروژه الزامی است.")
});

export type ProjectValues = z.infer<typeof projectSchema>;
export type RejectProjectValues = z.infer<typeof rejectProjectSchema>;
