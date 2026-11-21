"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { dailyProjectsApi } from "@/lib/api/daily-projects";
import type {
  CreateDailyProjectInput,
  StartTaskInput,
  SubmitTaskInput
} from "@/types/daily-project";

export const dailyProjectKeys = {
  all: ["daily-projects"] as const,
  active: () => ["daily-projects", "active"] as const,
  pending: () => ["daily-projects", "pending"] as const,
  byEmployer: (id: number) => ["daily-projects", "employer", id] as const,
  detail: (id: number) => ["daily-projects", "detail", id] as const,
  applications: (projectId: number) => ["daily-projects", "applications", projectId] as const,
  myApplications: (developerId: number) => ["daily-projects", "my-apps", developerId] as const,
};

export function useActiveDailyProjects() {
  return useQuery({
    queryKey: dailyProjectKeys.active(),
    queryFn: () => dailyProjectsApi.active(),
  });
}

export function usePendingDailyProjects() {
  return useQuery({
    queryKey: dailyProjectKeys.pending(),
    queryFn: () => dailyProjectsApi.pending(),
  });
}

export function useEmployerDailyProjects(employerId: number) {
  return useQuery({
    queryKey: dailyProjectKeys.byEmployer(employerId),
    queryFn: () => dailyProjectsApi.byEmployer(employerId),
    enabled: Boolean(employerId),
  });
}

export function useDailyProject(projectId?: number) {
  return useQuery({
    queryKey: dailyProjectKeys.detail(projectId ?? 0),
    queryFn: () => dailyProjectsApi.byId(projectId as number),
    enabled: Boolean(projectId),
  });
}

export function useDailyProjectApplications(projectId: number) {
  return useQuery({
    queryKey: dailyProjectKeys.applications(projectId),
    queryFn: () => dailyProjectsApi.applicationsByProject(projectId),
    enabled: projectId > 0,
  });
}

export function useDeveloperApplications(developerId: number) {
  return useQuery({
    queryKey: dailyProjectKeys.myApplications(developerId),
    queryFn: () => dailyProjectsApi.applicationsByDeveloper(developerId),
    enabled: Boolean(developerId),
  });
}

export function useCreateDailyProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (input: CreateDailyProjectInput) => dailyProjectsApi.create(input),
    onSuccess: () => void qc.invalidateQueries({ queryKey: dailyProjectKeys.all }),
  });
}

export function useApplyDailyProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (projectId: number) => dailyProjectsApi.apply(projectId),
    onSuccess: () => void qc.invalidateQueries({ queryKey: dailyProjectKeys.all }),
  });
}

export function useStartDailyTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (input: StartTaskInput) => dailyProjectsApi.startTask(input),
    onSuccess: () => void qc.invalidateQueries({ queryKey: dailyProjectKeys.all }),
  });
}

export function useSubmitDailyTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (input: SubmitTaskInput) => dailyProjectsApi.submitTask(input),
    onSuccess: () => void qc.invalidateQueries({ queryKey: dailyProjectKeys.all }),
  });
}

export function useApproveDailyProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (projectId: number) => dailyProjectsApi.approve(projectId),
    onSuccess: () => void qc.invalidateQueries({ queryKey: dailyProjectKeys.all }),
  });
}

export function useRejectDailyProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, reason }: { projectId: number; reason: string }) =>
      dailyProjectsApi.reject(projectId, reason),
    onSuccess: () => void qc.invalidateQueries({ queryKey: dailyProjectKeys.all }),
  });
}

export function useActivateDailyProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (projectId: number) => dailyProjectsApi.activate(projectId),
    onSuccess: () => void qc.invalidateQueries({ queryKey: dailyProjectKeys.all }),
  });
}

export function useApproveDailyTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (applicationId: number) => dailyProjectsApi.approveTask(applicationId),
    onSuccess: () => void qc.invalidateQueries({ queryKey: dailyProjectKeys.all }),
  });
}

export function useRejectDailyTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ applicationId, reason }: { applicationId: number; reason: string }) =>
      dailyProjectsApi.rejectTask(applicationId, reason),
    onSuccess: () => void qc.invalidateQueries({ queryKey: dailyProjectKeys.all }),
  });
}

export function useDepositFunds() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (projectId: number) => dailyProjectsApi.depositFunds(projectId),
    onSuccess: () => void qc.invalidateQueries({ queryKey: dailyProjectKeys.all }),
  });
}
