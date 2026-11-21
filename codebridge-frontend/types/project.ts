import type { Role } from "@/types/auth";

// هم‌راستا با enum ProjectStatus در بک‌اند
export const ProjectStatus = {
  Pending: 0,
  Approved: 1,
  Rejected: 2,
  InProgress: 3,
  Completed: 4
} as const;

export type ProjectStatus = (typeof ProjectStatus)[keyof typeof ProjectStatus];

// هم‌راستا با enum DeveloperLevelType در بک‌اند
export const DeveloperLevelType = {
  JuniorA: 0,
  JuniorB: 1,
  JuniorC: 2,
  MidA: 3,
  MidB: 4,
  MidC: 5,
  Senior: 6
} as const;

export type DeveloperLevelType =
  (typeof DeveloperLevelType)[keyof typeof DeveloperLevelType];

export interface Project {
  id: number;
  title: string;
  description: string;
  budget: number;
  employerId: number;
  employerName?: string | null;
  assignedDeveloperId?: number | null;
  assignedDeveloperName?: string | null;
  requiredLevel: DeveloperLevelType;
  status: ProjectStatus;
  adminNote?: string | null;
  isPublic: boolean;
  publishedAt?: string | null;
  isFinalApproved: boolean;
  finalApprovedAt?: string | null;
  createdAt: string;
}

export interface CreateProjectInput {
  title: string;
  description: string;
  budget: number;
  requiredLevel: DeveloperLevelType;
}

export interface ProjectMember {
  id: number;
  projectId: number;
  userId: number;
  userName: string;
  userEmail: string;
  userRole: Role;
  isApproved: boolean;
  joinedAt: string;
}

export interface AddProjectMemberInput {
  projectId: number;
  userId: number;
}
