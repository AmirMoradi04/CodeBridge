import { api } from "@/lib/api/axios";
import type { Resume, SaveResumeInput } from "@/types/resume";

export const resumesApi = {
  save: (userId: number, input: SaveResumeInput) =>
    api.put<Resume>(`/resumes/${userId}`, input).then((response) => response.data),

  byUser: (userId: number) =>
    api.get<Resume>(`/resumes/user/${userId}`).then((response) => response.data),

  pending: () =>
    api.get<Resume[]>("/resumes/pending").then((response) => response.data),

  approved: () =>
    api.get<Resume[]>("/resumes/approved").then((response) => response.data),

  approve: (resumeId: number) =>
    api.patch(`/resumes/${resumeId}/approve`).then((response) => response.data),

  reject: (resumeId: number) =>
    api.patch(`/resumes/${resumeId}/reject`).then((response) => response.data)
};
