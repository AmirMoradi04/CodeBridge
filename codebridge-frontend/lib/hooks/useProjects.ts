"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { projectsApi } from "@/lib/api/projects";
import type { CreateProjectInput, ProjectStatus } from "@/types/project";

export const projectKeys = {
  all: ["projects"] as const,
  adminList: (status?: ProjectStatus) =>
    ["projects", "admin", status ?? "all"] as const,
  detail: (projectId: number) => ["projects", "detail", projectId] as const,
  employer: (employerId: number) => ["projects", "employer", employerId] as const,
  assigned: () => ["projects", "assigned"] as const,
  pending: () => ["projects", "pending"] as const,
  unassigned: () => ["projects", "unassigned"] as const,
  members: (projectId: number) => ["projects", "members", projectId] as const,
};

export function useAdminProjects(status?: ProjectStatus) {
  return useQuery({
    queryKey: projectKeys.adminList(status),
    queryFn: () => projectsApi.list(status)
  });
}

export function useProject(projectId?: number) {
  return useQuery({
    queryKey: projectKeys.detail(projectId ?? 0),
    queryFn: () => projectsApi.byId(projectId as number),
    enabled: Boolean(projectId)
  });
}

export function useEmployerProjects(employerId?: number) {
  return useQuery({
    queryKey: projectKeys.employer(employerId ?? 0),
    queryFn: () => projectsApi.byEmployer(employerId as number),
    enabled: Boolean(employerId)
  });
}

export function useAssignedProjects() {
  return useQuery({
    queryKey: projectKeys.assigned(),
    queryFn: () => projectsApi.assigned()
  });
}

export function usePendingProjects() {
  return useQuery({
    queryKey: projectKeys.pending(),
    queryFn: () => projectsApi.pending()
  });
}

export function useUnassignedProjects() {
  return useQuery({
    queryKey: projectKeys.unassigned(),
    queryFn: () => projectsApi.unassigned()
  });
}

export function useProjectMembers(projectId?: number) {
  return useQuery({
    queryKey: projectKeys.members(projectId ?? 0),
    queryFn: () => projectsApi.members(projectId as number),
    enabled: Boolean(projectId)
  });
}

function useInvalidateProjects() {
  const queryClient = useQueryClient();

  return () => queryClient.invalidateQueries({ queryKey: projectKeys.all });
}

export function useCreateProject() {
  const invalidate = useInvalidateProjects();

  return useMutation({
    mutationFn: (input: CreateProjectInput) => projectsApi.create(input),
    onSuccess: invalidate
  });
}

export function useApproveProject() {
  const invalidate = useInvalidateProjects();

  return useMutation({
    mutationFn: (projectId: number) => projectsApi.approve(projectId),
    onSuccess: invalidate
  });
}

export function useRejectProject() {
  const invalidate = useInvalidateProjects();

  return useMutation({
    mutationFn: ({ projectId, reason }: { projectId: number; reason: string }) =>
      projectsApi.reject(projectId, reason),
    onSuccess: invalidate
  });
}

export function usePublishProject() {
  const invalidate = useInvalidateProjects();

  return useMutation({
    mutationFn: (projectId: number) => projectsApi.publish(projectId),
    onSuccess: invalidate
  });
}

export function useDepositProject() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, amount }: { projectId: number; amount: number }) =>
      projectsApi.deposit(projectId, amount),
    onSuccess: (_data, { projectId }) => {
      void queryClient.invalidateQueries({ queryKey: ["contracts", "project", projectId] });
      void queryClient.invalidateQueries({ queryKey: projectKeys.all });
      void queryClient.invalidateQueries({ queryKey: ["wallets"] });
    }
  });
}

export function useFinalApproveProject() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (projectId: number) => projectsApi.finalApprove(projectId),
    onSuccess: (_data, projectId) => {
      void queryClient.invalidateQueries({ queryKey: ["contracts", "project", projectId] });
      void queryClient.invalidateQueries({ queryKey: projectKeys.all });
      void queryClient.invalidateQueries({ queryKey: ["wallets"] });
    }
  });
}

export function useAssignDeveloper() {
  const invalidate = useInvalidateProjects();

  return useMutation({
    mutationFn: ({
      projectId,
      developerId
    }: {
      projectId: number;
      developerId: number;
    }) => projectsApi.assign(projectId, developerId),
    onSuccess: invalidate
  });
}

export function useAddProjectMember(projectId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (userId: number) => projectsApi.addMember(projectId, userId),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: projectKeys.members(projectId)
      });
      void queryClient.invalidateQueries({ queryKey: projectKeys.all });
    }
  });
}

export function useRemoveProjectMember(projectId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (userId: number) => projectsApi.removeMember(projectId, userId),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: projectKeys.members(projectId)
      });
      void queryClient.invalidateQueries({ queryKey: projectKeys.all });
    }
  });
}
