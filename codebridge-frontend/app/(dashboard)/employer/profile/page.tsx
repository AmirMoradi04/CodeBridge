"use client";

import { Building2 } from "lucide-react";
import { ProfileSettings } from "@/components/profile/ProfileSettings";
import { Card, CardContent } from "@/components/ui/card";

export default function EmployerProfilePage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2">
        <Building2 className="h-5 w-5 text-primary" />
        <div>
          <h2 className="text-lg font-semibold">پروفایل شرکت</h2>
          <p className="text-sm text-muted-foreground">
            لوگوی شرکت و اطلاعات نمایش داده‌شده در پروژه‌ها
          </p>
        </div>
      </div>

      <ProfileSettings mode="logo" />

      <Card>
        <CardContent className="p-4 text-sm text-muted-foreground">
          لوگو پس از ذخیره در پروژه‌های شما و گفت‌وگوها نمایش داده می‌شود.
        </CardContent>
      </Card>
    </div>
  );
}
