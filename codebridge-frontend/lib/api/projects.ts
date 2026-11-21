import { api } from "@/lib/api/axios";
import type {
  CreateProjectInput,
  Project,
  ProjectMember,
  ProjectStatus
} from "@/types/project";


export const projectsApi = {
  // فهرست کامل پروژه‌ها مخصوص ادمین
  list: (status?: ProjectStatus) =>
    api
      .get<Project[]>("/projects", { params: status === undefined ? {} : { status } })
      .then((response) => response.data),

  byId: (projectId: number) =>
    api.get<Project>(`/projects/${projectId}`).then((response) => response.data),

  pending: () => api.get<Project[]>("/projects/pending").then((response) => response.data),

  unassigned: () =>
    api.get<Project[]>("/projects/unassigned").then((response) => response.data),

  byEmployer: (employerId: number) =>
    api
      .get<Project[]>(`/projects/employer/${employerId}`)
      .then((response) => response.data),

  byDeveloper: (developerId: number) =>
    api
      .get<Project[]>(`/projects/developer/${developerId}`)
      .then((response) => response.data),

  assigned: () =>
    api.get<Project[]>("/projects/assigned").then((response) => response.data),

  create: (input: CreateProjectInput) =>
    api.post<Project>("/projects", input).then((response) => response.data),

  approve: (projectId: number) =>
    api.patch(`/projects/${projectId}/approve`).then((response) => response.data),

  reject: (projectId: number, reason: string) =>
    api
      .patch(`/projects/${projectId}/reject`, null, { params: { reason } })
      .then((response) => response.data),

  assign: (projectId: number, developerId: number) =>
    api
      .post(`/projects/${projectId}/assign`, { developerId })
      .then((response) => response.data),

  publish: (projectId: number) =>
    api.post(`/projects/${projectId}/publish`).then((response) => response.data),

  deposit: (projectId: number, amount: number) =>
    api.post(`/projects/${projectId}/deposit`, null, { params: { amount } }).then((response) => response.data),

  finalApprove: (projectId: number) =>
    api.post(`/projects/${projectId}/final-approve`).then((response) => response.data),

  changeStatus: (projectId: number, status: ProjectStatus) =>
    api
      .patch(`/projects/${projectId}/status`, null, { params: { status } })
      .then((response) => response.data),

  members: (projectId: number) =>
    api
      .get<ProjectMember[]>(`/projects/${projectId}/members`)
      .then((response) => response.data),

  addMember: (projectId: number, userId: number) =>
    api
      .post<ProjectMember>(`/projects/${projectId}/members`, { projectId, userId })
      .then((response) => response.data),

  removeMember: (projectId: number, userId: number) =>
    api
      .delete(`/projects/${projectId}/members/${userId}`)
      .then((response) => response.data)
};
