"use client";

import Link from "next/link";
import { FolderKanban, MessageSquare, Plus, Settings2 } from "lucide-react";
import { useAuth } from "@/lib/hooks/useAuth";
import { useEmployerProjects } from "@/lib/hooks/useProjects";
import { ProjectCard } from "@/components/projects/ProjectCard";
import { ProjectContractPanel } from "@/components/projects/ProjectContractPanel";
import { Role } from "@/types/auth";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { EmptyState } from "@/components/shared/EmptyState";

export default function EmployerDashboardPage() {
  const { user } = useAuth();
  const { data: projects, isLoading } = useEmployerProjects(user?.id);

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold">پروژه‌های من</h2>
          <p className="text-sm text-muted-foreground">
            پروژه‌های ثبت‌شده و وضعیت بررسی آنها
          </p>
        </div>

        <div className="flex gap-2">
          <Button asChild variant="outline">
            <Link href="/employer/chat">
              <MessageSquare className="h-4 w-4" />
              گفت‌وگو با ادمین
            </Link>
          </Button>
          <Button asChild>
            <Link href="/employer/projects/new">
              <Plus className="h-4 w-4" />
              ثبت پروژه‌ی جدید
            </Link>
          </Button>
        </div>
      </div>

      {isLoading ? (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          <Skeleton className="h-56 w-full" />
          <Skeleton className="h-56 w-full" />
        </div>
      ) : !projects || projects.length === 0 ? (
        <EmptyState
          icon={FolderKanban}
          title="هنوز پروژه‌ای ثبت نکرده‌اید"
          description="اولین پروژه‌ی خود را بسازید تا پس از تایید ادمین به برنامه‌نویسان برسد."
          action={
            <Button asChild>
              <Link href="/employer/projects/new">ثبت پروژه</Link>
            </Button>
          }
        />
      ) : (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          {projects.map((project) => (
            <ProjectCard
              key={project.id}
              project={project}
              actions={
                <Button asChild variant="outline" size="sm">
                  <Link href={`/employer/projects/${project.id}`}>
                    <Settings2 className="h-4 w-4" />
                    جزئیات پروژه
                  </Link>
                </Button>
              }
              footer={<ProjectContractPanel project={project} role={Role.Employer} embedded />}
            />
          ))}
        </div>
      )}
    </div>
  );
}