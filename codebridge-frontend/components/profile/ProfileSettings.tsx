"use client";

import { useRef, useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { ImagePlus, Loader2, Upload } from "lucide-react";
import { z } from "zod";

import { attachmentsApi } from "@/lib/api/attachments";
import { getErrorMessage } from "@/lib/api/axios";
import { useAuth } from "@/lib/hooks/useAuth";
import { useUpdateProfile, useUserProfile } from "@/lib/hooks/useUsers";
import { Role } from "@/types/auth";
import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

const profileSchema = z.object({
  firstName: z.string().min(2, "نام الزامی است"),
  lastName: z.string().min(2, "نام خانوادگی الزامی است"),
  bio: z.string().max(1000, "حداکثر ۱۰۰۰ کاراکتر").optional()
});

type ProfileValues = z.infer<typeof profileSchema>;

const MAX_IMAGE_SIZE = 10 * 1024 * 1024;
const IMAGE_TYPES = new Set(["image/png", "image/jpeg"]);

/**
 * تنظیمات پروفایل: اطلاعات پایه + آواتار (کارجو) یا لوگوی شرکت (کارفرما).
 * فایل ابتدا با API پیوست‌ها آپلود و سپس شناسه‌ی آن در پروفایل ذخیره می‌شود.
 */
export function ProfileSettings({ mode }: { mode: "avatar" | "logo" }) {
  const { user } = useAuth();
  const userId = user?.id;
  const isAvatar = mode === "avatar";

  const { data: profile, isLoading } = useUserProfile(userId);
  const updateProfile = useUpdateProfile(userId ?? 0);

  const [uploading, setUploading] = useState(false);
  const [pendingAttachmentId, setPendingAttachmentId] = useState<number | undefined>();

  const fileInputRef = useRef<HTMLInputElement>(null);

  const form = useForm<ProfileValues>({
    resolver: zodResolver(profileSchema),
    values: {
      firstName: profile?.firstName ?? "",
      lastName: profile?.lastName ?? "",
      bio: profile?.bio ?? ""
    }
  });

  const imageUrl = isAvatar
    ? profile?.profilePictureUrl
    : profile?.companyLogoUrl;

  async function handleFileSelected(file: File | null) {
    if (!file || !userId || uploading) return;

    if (!IMAGE_TYPES.has(file.type)) {
      toast.error("فقط تصویر PNG یا JPG مجاز است.");
      return;
    }

    if (file.size > MAX_IMAGE_SIZE) {
      toast.error("حجم تصویر نباید بیشتر از ۱۰ مگابایت باشد.");
      return;
    }

    setUploading(true);

    try {
      const attachment = await attachmentsApi.upload(
        file,
        isAvatar ? "ProfilePicture" : "CompanyLogo"
      );

      setPendingAttachmentId(attachment.id);
      toast.success(isAvatar ? "آواتار آپلود شد؛ ذخیره کنید." : "لوگو آپلود شد؛ ذخیره کنید.");
    } catch (error) {
      toast.error(getErrorMessage(error, "آپلود تصویر ناموفق بود."));
    } finally {
      setUploading(false);
    }
  }

  async function onSubmit(values: ProfileValues) {
    if (!userId) return;

    try {
      await updateProfile.mutateAsync({
        firstName: values.firstName,
        lastName: values.lastName,
        bio: values.bio || null,
        ...(isAvatar
          ? { profilePictureAttachmentId: pendingAttachmentId }
          : { companyLogoAttachmentId: pendingAttachmentId })
      });

      toast.success("پروفایل ذخیره شد.");
      setPendingAttachmentId(undefined);
    } catch (error) {
      toast.error(getErrorMessage(error, "ذخیره‌ی پروفایل ناموفق بود."));
    }
  }

  if (isLoading) {
    return (
      <Card>
        <CardContent className="space-y-3 p-5">
          <Skeleton className="h-24 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">
          {isAvatar ? "آواتار و اطلاعات شخصی" : "لوگوی شرکت و اطلاعات"}
        </CardTitle>
      </CardHeader>

      <CardContent className="space-y-5">
        <div className="flex flex-wrap items-center gap-4">
          <div className="relative">
            {imageUrl ? (
              // eslint-disable-next-line @next/next/no-img-element
              <img
                src={imageUrl}
                alt={isAvatar ? "آواتار" : "لوگو"}
                className="h-20 w-20 rounded-xl border object-cover"
              />
            ) : (
              <div className="flex h-20 w-20 items-center justify-center rounded-xl border bg-muted text-muted-foreground">
                <ImagePlus className="h-7 w-7" />
              </div>
            )}

            {uploading && (
              <div className="absolute inset-0 flex items-center justify-center rounded-xl bg-background/70">
                <Loader2 className="h-6 w-6 animate-spin text-primary" />
              </div>
            )}
          </div>

          <div className="space-y-2">
            <input
              ref={fileInputRef}
              type="file"
              accept="image/png,image/jpeg"
              className="hidden"
              onChange={(event) => handleFileSelected(event.target.files?.[0] ?? null)}
            />
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={uploading}
              onClick={() => fileInputRef.current?.click()}
            >
              <Upload className="h-4 w-4" />
              {uploading ? "در حال آپلود..." : `انتخاب ${isAvatar ? "آواتار" : "لوگو"}`}
            </Button>
            <p className="text-xs text-muted-foreground">
              فقط PNG یا JPG تا ۱۰ مگابایت
            </p>
          </div>
        </div>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <FormField
                control={form.control}
                name="firstName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>نام</FormLabel>
                    <FormControl>
                      <Input placeholder="نام" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="lastName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>نام خانوادگی</FormLabel>
                    <FormControl>
                      <Input placeholder="نام خانوادگی" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <FormField
              control={form.control}
              name="bio"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>درباره من</FormLabel>
                  <FormControl>
                    <Textarea rows={4} placeholder="توضیح مختصر درباره شما / شرکت" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <Button type="submit" disabled={updateProfile.isPending}>
              {updateProfile.isPending && <Loader2 className="h-4 w-4 animate-spin" />}
              ذخیره‌ی پروفایل
            </Button>
          </form>
        </Form>
      </CardContent>
    </Card>
  );
}
