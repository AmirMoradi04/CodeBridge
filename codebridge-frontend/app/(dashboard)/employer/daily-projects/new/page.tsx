"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { ArrowRight, Briefcase, Coins, Users } from "lucide-react";
import { z } from "zod";
import { useAuth } from "@/lib/hooks/useAuth";
import { useEmployerDailyProjects } from "@/lib/hooks/useDailyProjects";
import { dailyProjectsApi } from "@/lib/api/daily-projects";
import { getErrorMessage } from "@/lib/api/axios";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Badge } from "@/components/ui/badge";
import { EmptyState } from "@/components/shared/EmptyState";
import { Textarea } from "@/components/ui/textarea";
import { Input } from "@/components/ui/input";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";

const dailyProjectSchema = z.object({
  title: z.string().min(1, "عنوان الزامی است"),
  description: z.string().optional(),
  totalBudget: z.number().min(50000, "حداقل بودجه ۵۰ هزار تومان است"),
  pricePerUnit: z.number().min(5000, "حداقل قیمت هر واحد ۵ هزار تومان است"),
  capacity: z.number().min(1, "حداقل ظرفیت ۱ نفر است").max(100, "حداکثر ظرفیت ۱۰۰ نفر است"),
  deadlineSeconds: z
    .number()
    .min(300, "حداقل مهلت ۵ دقیقه است")
    .max(604800, "حداکثر مهلت ۷ روز است"),
  instructions: z.string().optional(),
});

type DailyProjectValues = z.infer<typeof dailyProjectSchema>;

export default function NewDailyProjectPage() {
  const router = useRouter();
  const [submitting, setSubmitting] = useState(false);

  const form = useForm<DailyProjectValues>({
    resolver: zodResolver(dailyProjectSchema),
    defaultValues: {
      title: "",
      description: "",
      totalBudget: undefined as unknown as number,
      pricePerUnit: undefined as unknown as number,
      capacity: undefined as unknown as number,
      deadlineSeconds: 3600,
      instructions: "",
    },
  });

  async function onSubmit(values: DailyProjectValues) {
    setSubmitting(true);
    try {
      await dailyProjectsApi.create({
        title: values.title,
        description: values.description || undefined,
        totalBudget: values.totalBudget,
        pricePerUnit: values.pricePerUnit,
        capacity: values.capacity,
        deadlineSeconds: values.deadlineSeconds,
        instructions: values.instructions || undefined,
      });
      toast.success("پروژه با موفقیت ثبت شد.");
      router.push("/employer/daily-projects");
    } catch (error) {
      toast.error(getErrorMessage(error, "ثبت پروژه ناموفق بود."));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div className="flex items-center gap-2">
        <Briefcase className="h-5 w-5 text-primary" />
        <div>
          <h2 className="text-lg font-semibold">ثبت پروژه‌ی روزانه‌ی جدید</h2>
          <p className="text-sm text-muted-foreground">
            پروژه‌ی روزانه پس از ثبت، در انتظار تایید ادمین قرار می‌گیرد.
          </p>
        </div>
      </div>

      <Card>
        <CardHeader />
        <CardContent>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
              <FormField
                control={form.control}
                name="title"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>عنوان پروژه</FormLabel>
                    <FormControl>
                      <Input placeholder="مثلاً: تست وب‌سایت فروشگاهی" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="description"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>توضیحات (اختیاری)</FormLabel>
                    <FormControl>
                      <Textarea
                        rows={4}
                        placeholder="شرح کامل پروژه و امکانات موردنیاز..."
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <FormField
                  control={form.control}
                  name="totalBudget"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>بودجه‌ی کل (تومان)</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          inputMode="numeric"
                          min={50000}
                          dir="ltr"
                          placeholder="500000"
                          onChange={(event) =>
                            field.onChange(event.target.value ? Number(event.target.value) : undefined)
                          }
                          value={field.value === undefined ? "" : String(field.value)}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="pricePerUnit"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>قیمت هر واحد (تومان)</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          inputMode="numeric"
                          min={5000}
                          dir="ltr"
                          placeholder="50000"
                          onChange={(event) =>
                            field.onChange(event.target.value ? Number(event.target.value) : undefined)
                          }
                          value={field.value === undefined ? "" : String(field.value)}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <FormField
                  control={form.control}
                  name="capacity"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>ظرفیت (نفر)</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          inputMode="numeric"
                          min={1}
                          max={100}
                          dir="ltr"
                          placeholder="10"
                          onChange={(event) =>
                            field.onChange(event.target.value ? Number(event.target.value) : undefined)
                          }
                          value={field.value === undefined ? "" : String(field.value)}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="deadlineSeconds"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>مهلت انجام (ثانیه)</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          inputMode="numeric"
                          min={300}
                          max={604800}
                          dir="ltr"
                          placeholder="3600"
                          onChange={(event) =>
                            field.onChange(event.target.value ? Number(event.target.value) : undefined)
                          }
                          value={field.value === undefined ? "" : String(field.value)}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <FormField
                control={form.control}
                name="instructions"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>دستورالعمل (اختیاری)</FormLabel>
                    <FormControl>
                      <Textarea
                        rows={3}
                        placeholder="توضیحات یا نکات خاص برای انجام‌دهنده..."
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <div className="flex gap-3 pt-2">
                <Button type="submit" disabled={submitting}>
                  {submitting ? "در حال ثبت..." : "ثبت پروژه"}
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => router.push("/employer/daily-projects")}
                >
                  انصراف
                </Button>
              </div>
            </form>
          </Form>
        </CardContent>
      </Card>
    </div>
  );
}
