"use client";

import { useMemo, useState } from "react";
import { Search, Trash2, UserPlus, Users } from "lucide-react";
import { toast } from "sonner";
import { getErrorMessage } from "@/lib/api/axios";
import { useAuth } from "@/lib/hooks/useAuth";
import {
  useAddProjectMember,
  useProjectMembers,
  useRemoveProjectMember
} from "@/lib/hooks/useProjects";
import { useUsers } from "@/lib/hooks/useUsers";
import { ROLE_BADGE_CLASSES, ROLE_LABELS } from "@/lib/constants";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger
} from "@/components/ui/dialog";
import { Skeleton } from "@/components/ui/skeleton";
import { EmptyState } from "@/components/shared/EmptyState";

export function ProjectMembers({
  projectId,
  canManage
}: {
  projectId: number;
  canManage: boolean;
}) {
  const { user } = useAuth();
  const { data: members, isLoading } = useProjectMembers(projectId);
  const removeMember = useRemoveProjectMember(projectId);
  const addMember = useAddProjectMember(projectId);

  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState("");

  // جستجوی کاربران فقط برای ادمین فراخوانی می‌شود
  const { data: users = [] } = useUsers(undefined, open && canManage);

  const results = useMemo(() => {
    const term = query.trim().toLowerCase();
    const memberIds = new Set((members ?? []).map((member) => member.userId));

    return users
      .filter((candidate) => !memberIds.has(candidate.id))
      .filter(
        (candidate) =>
          term.length === 0 ||
          candidate.email.toLowerCase().includes(term) ||
          `${candidate.firstName} ${candidate.lastName}`.toLowerCase().includes(term)
      )
      .slice(0, 8);
  }, [users, members, query]);

  async function handleAdd(userId: number) {
    try {
      await addMember.mutateAsync(userId);
      toast.success("کاربر به تیم پروژه اضافه شد.");
      setQuery("");
      setOpen(false);
    } catch (error) {
      toast.error(getErrorMessage(error, "افزودن عضو ناموفق بود."));
    }
  }

  async function handleRemove(userId: number) {
    try {
      await removeMember.mutateAsync(userId);
      toast.success("عضو از پروژه حذف شد.");
    } catch (error) {
      toast.error(getErrorMessage(error, "حذف عضو ناموفق بود."));
    }
  }

  return (
    <Card>
      <CardHeader className="flex-row items-center justify-between">
        <CardTitle className="flex items-center gap-2 text-base">
          <Users className="h-4 w-4 text-primary" />
          اعضای تیم
        </CardTitle>

        {canManage && (
          <Dialog open={open} onOpenChange={setOpen}>
            <DialogTrigger asChild>
              <Button size="sm" variant="outline">
                <UserPlus className="h-4 w-4" />
                افزودن عضو
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>افزودن عضو به تیم پروژه</DialogTitle>
                <DialogDescription>
                  کاربر را با ایمیل یا نام جستجو و انتخاب کنید.
                </DialogDescription>
              </DialogHeader>

              <div className="relative">
                <Search className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  value={query}
                  onChange={(event) => setQuery(event.target.value)}
                  placeholder="جستجوی ایمیل یا نام..."
                  className="pe-9"
                />
              </div>

              <div className="max-h-72 space-y-1 overflow-y-auto">
                {results.length === 0 ? (
                  <p className="p-4 text-center text-sm text-muted-foreground">
                    کاربری پیدا نشد.
                  </p>
                ) : (
                  results.map((candidate) => (
                    <button
                      key={candidate.id}
                      type="button"
                      onClick={() => handleAdd(candidate.id)}
                      className="flex w-full items-center justify-between gap-3 rounded-lg border p-3 text-right transition-colors hover:bg-accent"
                    >
                      <span>
                        <span className="block text-sm font-medium">
                          {candidate.firstName} {candidate.lastName}
                        </span>
                        <span className="block text-xs text-muted-foreground" dir="ltr">
                          {candidate.email}
                        </span>
                      </span>
                      <Badge className={ROLE_BADGE_CLASSES[candidate.userRole]}>
                        {ROLE_LABELS[candidate.userRole]}
                      </Badge>
                    </button>
                  ))
                )}
              </div>
            </DialogContent>
          </Dialog>
        )}
      </CardHeader>

      <CardContent className="space-y-3">
        {isLoading ? (
          <div className="space-y-2">
            <Skeleton className="h-12 w-full" />
            <Skeleton className="h-12 w-full" />
          </div>
        ) : !members || members.length === 0 ? (
          <EmptyState
            icon={Users}
            title="هنوز عضوی اضافه نشده"
            description="اعضای تیم این پروژه اینجا نمایش داده می‌شوند."
          />
        ) : (
          members.map((member) => (
            <div
              key={member.id}
              className="flex items-center justify-between gap-3 rounded-lg border p-3"
            >
              <div className="min-w-0">
                <p className="truncate text-sm font-medium">{member.userName}</p>
                <p className="truncate text-xs text-muted-foreground" dir="ltr">
                  {member.userEmail}
                </p>
              </div>

              <div className="flex items-center gap-2">
                <Badge className={ROLE_BADGE_CLASSES[member.userRole]}>
                  {ROLE_LABELS[member.userRole]}
                </Badge>

                {canManage && member.userId !== user?.id && (
                  <Button
                    size="icon"
                    variant="ghost"
                    aria-label="حذف عضو"
                    disabled={removeMember.isPending}
                    onClick={() => handleRemove(member.userId)}
                  >
                    <Trash2 className="h-4 w-4 text-destructive" />
                  </Button>
                )}
              </div>
            </div>
          ))
        )}
      </CardContent>
    </Card>
  );
}