"use client";

import { useEffect, useState } from "react";
import { Download, ExternalLink, Eye, Loader2 } from "lucide-react";
import { toast } from "sonner";
import {
  attachmentErrorMessage,
  attachmentsApi
} from "@/lib/api/attachments";
import { useAttachment } from "@/lib/hooks/useAttachments";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle
} from "@/components/ui/dialog";

/**
 * مشاهده/دانلود فایل رزومه؛ فقط صاحب رزومه و ادمین به آن دسترسی دارند.
 * فایل با توکن احراز هویت دریافت می‌شود، نه با لینک عمومی.
 */
export function ResumeAttachmentLink({ attachmentId }: { attachmentId: number }) {
  const { data, isLoading } = useAttachment(attachmentId);
  const [isOpen, setIsOpen] = useState(false);
  const [isFetching, setIsFetching] = useState(false);
  const [preview, setPreview] = useState<{ url: string; fileName: string } | null>(
    null
  );

  // آدرس موقت فایل هنگام بستن یا جایگزینی آزاد می‌شود
  useEffect(
    () => () => {
      if (preview) URL.revokeObjectURL(preview.url);
    },
    [preview]
  );

  if (isLoading) return <Skeleton className="h-7 w-32" />;

  if (!data) {
    return (
      <span className="text-xs text-muted-foreground">
        دسترسی به فایل رزومه مجاز نیست.
      </span>
    );
  }

  const fallbackFileName = data.fileName;

  async function handleOpen() {
    setIsOpen(true);
    setIsFetching(true);

    try {
      const { blob, fileName } = await attachmentsApi.file(attachmentId);

      setPreview({
        url: URL.createObjectURL(blob),
        fileName: fileName ?? fallbackFileName
      });
    } catch (error) {
      setIsOpen(false);
      toast.error(await attachmentErrorMessage(error));
    } finally {
      setIsFetching(false);
    }
  }

  return (
    <>
      <Button
        type="button"
        variant="outline"
        size="sm"
        className="h-7 max-w-full gap-1.5 px-2.5 text-xs"
        onClick={handleOpen}
        title={`مشاهده‌ی ${fallbackFileName}`}
      >
        <Eye className="h-3.5 w-3.5 shrink-0" />
        <span>مشاهده رزومه</span>
        <span className="max-w-[12rem] truncate text-muted-foreground">
          ({fallbackFileName})
        </span>
      </Button>

      <Dialog open={isOpen} onOpenChange={setIsOpen}>
        <DialogContent className="max-w-4xl gap-3">
          <DialogHeader>
            <DialogTitle className="truncate text-sm">
              {preview?.fileName ?? fallbackFileName}
            </DialogTitle>
          </DialogHeader>

          {isFetching || !preview ? (
            <div className="flex h-[65vh] items-center justify-center rounded-lg border">
              <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
            </div>
          ) : (
            <>
              <iframe
                src={preview.url}
                title={preview.fileName}
                className="h-[65vh] w-full rounded-lg border"
              />
              <div className="flex flex-wrap justify-end gap-2">
                {/* اگر مرورگر پیش‌نمایش PDF نداشت، فایل در تب جدید باز می‌شود */}
                <Button asChild variant="ghost" size="sm">
                  <a href={preview.url} target="_blank" rel="noreferrer">
                    <ExternalLink className="h-4 w-4" />
                    باز کردن در تب جدید
                  </a>
                </Button>
                <Button asChild variant="outline" size="sm">
                  <a href={preview.url} download={preview.fileName}>
                    <Download className="h-4 w-4" />
                    دانلود فایل
                  </a>
                </Button>
              </div>
            </>
          )}
        </DialogContent>
      </Dialog>
    </>
  );
}
