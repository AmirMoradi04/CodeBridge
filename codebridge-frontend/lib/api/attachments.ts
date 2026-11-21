import axios from "axios";
import { api, getErrorMessage } from "@/lib/api/axios";
import type { Attachment } from "@/types/user";

/** نام فایل از هدر Content-Disposition (فرمت RFC 5987) استخراج می‌شود. */
function fileNameFromDisposition(header: unknown): string | null {
  if (typeof header !== "string") return null;

  const encoded = /filename\*=UTF-8''([^;]+)/i.exec(header);
  if (encoded?.[1]) {
    try {
      return decodeURIComponent(encoded[1]);
    } catch {
      return encoded[1];
    }
  }

  const plain = /filename="?([^";]+)"?/i.exec(header);
  return plain?.[1] ?? null;
}

/**
 * پاسخ خطای دانلود به صورت Blob برمی‌گردد؛ متن آن خوانده می‌شود تا پیام
 * فارسی بک‌اند (مثلاً نبود دسترسی) به کاربر نشان داده شود.
 */
export async function attachmentErrorMessage(
  error: unknown,
  fallback = "دریافت فایل ناموفق بود."
): Promise<string> {
  if (axios.isAxiosError(error) && error.response?.data instanceof Blob) {
    try {
      const body = JSON.parse(await error.response.data.text()) as {
        message?: string;
        errors?: string[];
      };

      if (body.errors?.length) return body.errors.join("، ");
      if (body.message) return body.message;
    } catch {
      // بدنه‌ی خطا JSON نبود؛ از پیام پیش‌فرض استفاده می‌شود
    }
  }

  return getErrorMessage(error, fallback);
}

export const attachmentsApi = {
  upload: (file: File, relatedEntityType: string, relatedEntityId?: number) => {
    const formData = new FormData();
    formData.append("file", file);

    // هدر چندبخشی صریحاً تنظیم می‌شود؛ در غیر این صورت Content-Type پیش‌فرض JSON
    // روی درخواست می‌ماند و بک‌اند فایل را دریافت نمی‌کند.
    return api
      .post<Attachment>("/attachments", formData, {
        params: { relatedEntityType, relatedEntityId },
        headers: { "Content-Type": "multipart/form-data" }
      })
      .then((response) => response.data);
  },

  byId: (attachmentId: number) =>
    api
      .get<Attachment>(`/attachments/${attachmentId}`)
      .then((response) => response.data),

  // فایل با توکن خوانده می‌شود؛ آدرس عمومی برای پیوست‌ها وجود ندارد
  file: async (attachmentId: number) => {
    const response = await api.get<Blob>(`/attachments/${attachmentId}/file`, {
      responseType: "blob"
    });

    return {
      blob: response.data,
      fileName: fileNameFromDisposition(response.headers["content-disposition"])
    };
  },

  mine: () =>
    api.get<Attachment[]>("/attachments/my").then((response) => response.data),

  remove: (attachmentId: number) =>
    api.delete(`/attachments/${attachmentId}`).then((response) => response.data)
};
