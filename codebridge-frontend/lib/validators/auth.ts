import { z } from "zod";
import { Role } from "@/types/auth";

export const loginSchema = z.object({
  email: z
    .string()
    .min(1, "ایمیل الزامی است.")
    .email("ایمیل معتبر نیست."),
  password: z.string().min(6, "رمز عبور باید حداقل ۶ کاراکتر باشد.")
});

export const registerSchema = z
  .object({
    firstName: z.string().min(2, "نام باید حداقل ۲ کاراکتر باشد."),
    lastName: z.string().min(2, "نام خانوادگی باید حداقل ۲ کاراکتر باشد."),
    email: z
      .string()
      .min(1, "ایمیل الزامی است.")
      .email("ایمیل معتبر نیست."),
    password: z.string().min(6, "رمز عبور باید حداقل ۶ کاراکتر باشد."),
    confirmPassword: z.string().min(6, "تکرار رمز عبور الزامی است."),
    // فقط این دو نقش حق ثبت‌نام دارند؛ برنامه‌نویس توسط ادمین مدیریت می‌شود
    userRole: z.coerce
      .number()
      .refine(
        (value) => value === Role.JobSeeker || value === Role.Employer,
        "نقش خود را انتخاب کنید."
      )
  })
  .refine((values) => values.password === values.confirmPassword, {
    message: "رمز عبور و تکرار آن یکسان نیستند.",
    path: ["confirmPassword"]
  });

export type LoginValues = z.infer<typeof loginSchema>;
export type RegisterValues = z.infer<typeof registerSchema>;
