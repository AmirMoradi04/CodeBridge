import { format } from "date-fns";
import { faIR } from "date-fns/locale";
import { Briefcase, Calendar, Coins, UserRound } from "lucide-react";
import {
  LEVEL_LABELS,
  PROJECT_STATUS_CLASSES,
  PROJECT_STATUS_LABELS
} from "@/lib/constants";
import type { Project } from "@/types/project";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";

export function ProjectCard({
  project,
  actions,
  footer
}: {
  project: Project;
  actions?: React.ReactNode;
  footer?: React.ReactNode;
}) {
  return (
    <Card className="flex h-full flex-col">
      <CardHeader className="gap-3">
        <div className="flex items-start justify-between gap-3">
          <CardTitle className="text-base leading-6">{project.title}</CardTitle>
          <Badge className={PROJECT_STATUS_CLASSES[project.status]}>
            {PROJECT_STATUS_LABELS[project.status]}
          </Badge>
        </div>

        <div className="flex flex-wrap items-center gap-3 text-xs text-muted-foreground">
          <span className="inline-flex items-center gap-1">
            <Coins className="h-3.5 w-3.5" />
            {project.budget.toLocaleString("fa-IR")} تومان
          </span>
          <span className="inline-flex items-center gap-1">
            <Briefcase className="h-3.5 w-3.5" />
            {LEVEL_LABELS[project.requiredLevel]}
          </span>
          <span className="inline-flex items-center gap-1">
            <Calendar className="h-3.5 w-3.5" />
            {format(new Date(project.createdAt), "yyyy/MM/dd", { locale: faIR })}
          </span>
        </div>
      </CardHeader>

      <CardContent className="flex flex-1 flex-col gap-4">
        <p className="line-clamp-3 text-sm text-muted-foreground">{project.description}</p>

        <div className="space-y-1 text-xs">
          {project.employerName && (
            <p className="text-muted-foreground">کارفرما: {project.employerName}</p>
          )}
          <p className="text-muted-foreground">
            برنامه‌نویس:{" "}
            {project.assignedDeveloperName ?? (
              <span className="text-amber-600 dark:text-amber-400">تخصیص نیافته</span>
            )}
          </p>
          {project.isPublic && (
            <Badge variant="outline" className="text-[10px]">
              منتشرشده
            </Badge>
          )}
        </div>

        <div className="mt-auto flex flex-wrap items-center gap-2">{actions}</div>
        {footer}
      </CardContent>
    </Card>
  );
}