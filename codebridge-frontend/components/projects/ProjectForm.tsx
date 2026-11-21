"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { getErrorMessage } from "@/lib/api/axios";
import { useCreateProject } from "@/lib/hooks/useProjects";
import { projectSchema, type ProjectValues } from "@/lib/validators/project";
import { LEVEL_OPTIONS } from "@/lib/constants";
import type { DeveloperLevelType } from "@/types/project";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage
} from "@/components/ui/form";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from "@/components/ui/select";

export function ProjectForm({ onSuccess }: { onSuccess: () => void }) {
  const createProject = useCreateProject();
  const [submitting, setSubmitting] = useState(false);

  const form = useForm<ProjectValues>({
    resolver: zodResolver(projectSchema),
    defaultValues: {
      title: "",
      description: "",
      budget: undefined as unknown as number,
      requiredLevel: 0
    }
  });

  async function onSubmit(values: ProjectValues) {
    setSubmitting(true);

    try {
      await createProject.mutateAsync({
        title: values.title,
        description: values.description,
        budget: values.budget,
        requiredLevel: values.requiredLevel as DeveloperLevelType
      });
      toast.success("پروژه ثبت شد و در انتظار تایید ادمین است.");
      onSuccess();
    } catch (error) {
      toast.error(getErrorMessage(error, "ثبت پروژه ناموفق بود."));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="title"
          render={({ field }) => (
            <FormItem>
              <FormLabel>عنوان پروژه</FormLabel>
              <FormControl>
                <Input placeholder="مثلاً: توسعه‌ی فروشگاه آنلاین" {...field} />
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
              <FormLabel>توضیحات</FormLabel>
              <FormControl>
                <Textarea
                  rows={5}
                  placeholder="شرح کامل پروژه، امکانات موردنیاز و زمان‌بندی..."
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
            name="budget"
            render={({ field }) => (
              <FormItem>
                <FormLabel>بودجه (تومان)</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    inputMode="numeric"
                    min={1}
                    dir="ltr"
                    placeholder="15000000"
                    onChange={(event) => field.onChange(event.target.value)}
                    value={field.value === undefined ? "" : String(field.value)}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="requiredLevel"
            render={({ field }) => (
              <FormItem>
                <FormLabel>سطح موردنیاز</FormLabel>
                <Select
                  value={String(field.value)}
                  onValueChange={(value) => field.onChange(Number(value))}
                >
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="انتخاب سطح" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {LEVEL_OPTIONS.map((option) => (
                      <SelectItem key={option.value} value={String(option.value)}>
                        {option.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <Button type="submit" disabled={submitting}>
          {submitting ? "در حال ثبت..." : "ثبت پروژه"}
        </Button>
      </form>
    </Form>
  );
}