import { Role } from "@/types/auth";
import { ProjectStatus } from "@/types/project";
import { ResumeStatus } from "@/types/resume";
import { ConversationType } from "@/types/message";

export const ROLE_LABELS: Record<Role, string> = {
  [Role.Admin]: "ادمین",
  [Role.Employer]: "کارفرما",
  [Role.JobSeeker]: "کارجو"
};

export const ROLE_HOME: Record<Role, string> = {
  [Role.Admin]: "/admin",
  [Role.Employer]: "/employer",
  [Role.JobSeeker]: "/jobseeker"
};

export const ROLE_BADGE_CLASSES: Record<Role, string> = {
  [Role.Admin]: "bg-violet-100 text-violet-700 dark:bg-violet-500/15 dark:text-violet-300",
  [Role.Employer]: "bg-amber-100 text-amber-700 dark:bg-amber-500/15 dark:text-amber-300",
  [Role.JobSeeker]: "bg-emerald-100 text-emerald-700 dark:bg-emerald-500/15 dark:text-emerald-300"
};

export const PROJECT_STATUS_LABELS: Record<ProjectStatus, string> = {
  [ProjectStatus.Pending]: "در انتظار تایید",
  [ProjectStatus.Approved]: "تاییدشده",
  [ProjectStatus.Rejected]: "ردشده",
  [ProjectStatus.InProgress]: "در حال اجرا",
  [ProjectStatus.Completed]: "تکمیل‌شده"
};

export const PROJECT_STATUS_CLASSES: Record<ProjectStatus, string> = {
  [ProjectStatus.Pending]: "bg-amber-100 text-amber-800 dark:bg-amber-500/15 dark:text-amber-300",
  [ProjectStatus.Approved]: "bg-sky-100 text-sky-800 dark:bg-sky-500/15 dark:text-sky-300",
  [ProjectStatus.Rejected]: "bg-rose-100 text-rose-800 dark:bg-rose-500/15 dark:text-rose-300",
  [ProjectStatus.InProgress]: "bg-indigo-100 text-indigo-800 dark:bg-indigo-500/15 dark:text-indigo-300",
  [ProjectStatus.Completed]: "bg-emerald-100 text-emerald-800 dark:bg-emerald-500/15 dark:text-emerald-300"
};

export const RESUME_STATUS_LABELS: Record<ResumeStatus, string> = {
  [ResumeStatus.Pending]: "در انتظار بررسی",
  [ResumeStatus.Approved]: "تاییدشده",
  [ResumeStatus.Rejected]: "ردشده"
};

export const RESUME_STATUS_CLASSES: Record<ResumeStatus, string> = {
  [ResumeStatus.Pending]: "bg-amber-100 text-amber-800 dark:bg-amber-500/15 dark:text-amber-300",
  [ResumeStatus.Approved]: "bg-emerald-100 text-emerald-800 dark:bg-emerald-500/15 dark:text-emerald-300",
  [ResumeStatus.Rejected]: "bg-rose-100 text-rose-800 dark:bg-rose-500/15 dark:text-rose-300"
};

export const LEVEL_LABELS: Record<number, string> = {
  0: "Junior A",
  1: "Junior B",
  2: "Junior C",
  3: "Mid A",
  4: "Mid B",
  5: "Mid C",
  6: "Senior"
};

export const CONVERSATION_TYPE_LABELS: Record<ConversationType, string> = {
  [ConversationType.AdminUser]: "گفت‌وگو با ادمین",
  [ConversationType.ProjectGroup]: "گروه پروژه"
};

export const ROLE_OPTIONS = [
  { value: Role.JobSeeker, label: ROLE_LABELS[Role.JobSeeker] },
  { value: Role.Employer, label: ROLE_LABELS[Role.Employer] }
];

export const ROLE_FILTER_OPTIONS = [
  { value: Role.Admin, label: ROLE_LABELS[Role.Admin] },
  ...ROLE_OPTIONS
];

export const LEVEL_OPTIONS = Object.entries(LEVEL_LABELS).map(([value, label]) => ({
  value: Number(value),
  label
}));

/**
 * قانون سطح حداقلی تخصیص (هم‌خوان با بک‌اند):
 * پروژه‌های Junior (0..2) هر سطحی را می‌پذیرند؛
 * پروژه‌های Mid (3..5) فقط Mid یا Senior را می‌پذیرند؛
 * پروژه‌ی Senior (6) فقط برنامه‌نویس Senior را می‌پذیرد.
 */
export function isLevelSufficient(projectLevel: number, devLevel: number): boolean {
  if (projectLevel <= 2) return true;
  if (projectLevel <= 5) return devLevel >= 3;
  return devLevel === 6;
}

export const PROJECT_STATUS_OPTIONS: { value: ProjectStatus; label: string }[] = [
  { value: ProjectStatus.Pending, label: PROJECT_STATUS_LABELS[ProjectStatus.Pending] },
  { value: ProjectStatus.Approved, label: PROJECT_STATUS_LABELS[ProjectStatus.Approved] },
  { value: ProjectStatus.Rejected, label: PROJECT_STATUS_LABELS[ProjectStatus.Rejected] },
  { value: ProjectStatus.InProgress, label: PROJECT_STATUS_LABELS[ProjectStatus.InProgress] },
  { value: ProjectStatus.Completed, label: PROJECT_STATUS_LABELS[ProjectStatus.Completed] }
];

export function getName(parts: {
  firstName?: string | null;
  lastName?: string | null;
}): string {
  const full = `${parts.firstName ?? ""} ${parts.lastName ?? ""}`.trim();
  return full || "کاربر CodeBridge";
}

export function getInitials(parts: {
  firstName?: string | null;
  lastName?: string | null;
}): string {
  const first = parts.firstName?.trim()?.[0] ?? "";
  const last = parts.lastName?.trim()?.[0] ?? "";
  return (first + last).trim() || "CB";
}
