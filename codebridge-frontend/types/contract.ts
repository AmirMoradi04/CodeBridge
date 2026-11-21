import type { Project } from "@/types/project";

export const ContractStatus = {
  Draft: 0,
  Signed: 1,
  Active: 2,
  Completed: 3,
  Cancelled: 4
} as const;

export type ContractStatus = (typeof ContractStatus)[keyof typeof ContractStatus];

export interface Contract {
  id: number;
  projectId: number;
  developerId: number;
  employerId: number;
  adminId: number;
  totalAmount: number;
  platformFee: number;
  developerShare: number;
  mentorShare?: number | null;
  depositPaid: number;
  isFullyPaid: boolean;
  status: ContractStatus;
  signedAt?: string | null;
  isNdaSigned: boolean;
  ndaSignedAt?: string | null;
  createdAt: string;
}

export interface CreateContractInput {
  projectId: number;
  totalAmount: number;
  platformFee: number;
  developerShare: number;
  mentorShare?: number | null;
  mentorId?: number | null;
}

export interface ProjectPaymentSummary {
  project: Project;
  contract: Contract | null;
}
