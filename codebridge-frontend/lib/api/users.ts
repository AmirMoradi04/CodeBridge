import { api } from "@/lib/api/axios";
import type { Role } from "@/types/auth";
import type { DeveloperLevelType } from "@/types/project";
import type { UpdateProfileInput, User, UserProfile } from "@/types/user";

export const usersApi = {
  getAll: (role?: Role) =>
    api
      .get<User[]>("/users", { params: role === undefined ? {} : { role } })
      .then((response) => response.data),

  getPending: () =>
    api.get<User[]>("/users/pending").then((response) => response.data),

  getProfile: (userId: number) =>
    api
      .get<UserProfile>(`/users/${userId}/profile`)
      .then((response) => response.data),

  updateProfile: (userId: number, input: UpdateProfileInput) =>
    api
      .put(`/users/${userId}/profile`, input)
      .then((response) => response.data),

  approve: (userId: number) =>
    api.patch(`/users/${userId}/approve`).then((response) => response.data),

  reject: (userId: number) =>
    api.patch(`/users/${userId}/reject`).then((response) => response.data),

  jobSeekersByLevel: (level: DeveloperLevelType) =>
    api
      .get<User[]>(`/users/job-seekers/${level}`)
      .then((response) => response.data),

  countByRole: (role: Role) =>
    api
      .get<number>(`/users/count/${role}`)
      .then((response) => response.data)
};
