"use client";

import Link from "next/link";
import { FolderKanban, MessageSquare, MessagesSquare } from "lucide-react";
import { useAuth } from "@/lib/hooks/useAuth";
import { useAssignedProjects } from "@/lib/hooks/useProjects";
import { ProjectCard } from "@/components/projects/ProjectCard";
import { ProjectContractPanel } from "@/components/projects/ProjectContractPanel";
import { Role } from "@/types/auth";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { EmptyState } from "@/components/shared/EmptyState";

export default function JobSeekerProjectsPage() {
  const { user } = useAuth();
  const { data: projects, isLoading } = useAssignedProjects();

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold">پروژه‌های من</h2>
          <p className="text-sm text-muted-foreground">
            پروژه‌هایی که در آنها تخصیص یافته یا عضو تیم هستید
          </p>
        </div>

        <Button asChild variant="outline">
          <Link href="/jobseeker/chat">
            <MessageSquare className="h-4 w-4" />
            گفت‌وگو با ادمین
          </Link>
        </Button>
      </div>

      {isLoading ? (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          <Skeleton className="h-56 w-full" />
          <Skeleton className="h-56 w-full" />
        </div>
      ) : !projects || projects.length === 0 ? (
        <EmptyState
          icon={FolderKanban}
          title="پروژه‌ای به شما تخصیص نیافته"
          description="وقتی ادمین شما را به پروژه‌ای اضافه کند، اینجا نمایش داده می‌شود."
        />
      ) : (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          {projects.map((project) => (
            <ProjectCard
              key={project.id}
              project={project}
              actions={
                <Button asChild variant="outline" size="sm">
                  <Link href={`/jobseeker/projects/${project.id}/chat`}>
                    <MessagesSquare className="h-4 w-4" />
                    گفت‌وگوی گروهی
                  </Link>
                </Button>
              }
              footer={<ProjectContractPanel project={project} role={Role.JobSeeker} embedded />}
            />
          ))}
        </div>
      )}
    </div>
  );
}
