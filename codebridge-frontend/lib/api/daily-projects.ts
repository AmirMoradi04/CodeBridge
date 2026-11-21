import { api } from "@/lib/api/axios";
import type {
  CreateDailyProjectInput,
  DailyApplicationDto,
  DailyProjectDto,
  SubmitTaskInput,
  StartTaskInput
} from "@/types/daily-project";

export const dailyProjectsApi = {
  active: () =>
    api.get<DailyProjectDto[]>("/dailyprojects/active").then((r) => r.data),

  pending: () =>
    api.get<DailyProjectDto[]>("/dailyprojects/pending").then((r) => r.data),

  approved: () =>
    api.get<DailyProjectDto[]>("/dailyprojects/approved").then((r) => r.data),

  byEmployer: (_employerId?: number) =>
    api.get<DailyProjectDto[]>("/dailyprojects/employer/me").then((r) => r.data),

  byId: (projectId: number) =>
    api.get<DailyProjectDto>(`/dailyprojects/${projectId}`).then((r) => r.data),

  applicationsByDeveloper: (developerId: number) =>
    api
      .get<DailyApplicationDto[]>(`/dailyprojects/developer/${developerId}/applications`)
      .then((r) => r.data),

  applicationsByProject: (projectId: number) =>
    api
      .get<DailyApplicationDto[]>(`/dailyprojects/${projectId}/applications`)
      .then((r) => r.data),

  create: (input: CreateDailyProjectInput) =>
    api.post<DailyProjectDto>("/dailyprojects", input).then((r) => r.data),

  depositFunds: (projectId: number) =>
    api.post(`/dailyprojects/${projectId}/deposit-funds`).then((r) => r.data),

  apply: (projectId: number) =>
    api.post<DailyApplicationDto>(`/dailyprojects/${projectId}/apply`).then((r) => r.data),

  startTask: (input: StartTaskInput) =>
    api.post("/dailyprojects/start", input).then((r) => r.data),

  submitTask: (input: SubmitTaskInput) =>
    api.post("/dailyprojects/submit", input).then((r) => r.data),

  approve: (projectId: number) =>
    api.patch(`/dailyprojects/${projectId}/approve`).then((r) => r.data),

  reject: (projectId: number, note: string) =>
    api.patch(`/dailyprojects/${projectId}/reject`, null, { params: { note } }).then((r) => r.data),

  activate: (projectId: number) =>
    api.patch(`/dailyprojects/${projectId}/activate`).then((r) => r.data),

  approveTask: (applicationId: number) =>
    api.patch(`/dailyprojects/tasks/${applicationId}/approve`).then((r) => r.data),

  rejectTask: (applicationId: number, note: string) =>
    api.patch(`/dailyprojects/tasks/${applicationId}/reject`, null, { params: { note } }).then((r) => r.data),

  expireTask: (applicationId: number) =>
    api.patch(`/dailyprojects/tasks/${applicationId}/expire`).then((r) => r.data)
};
