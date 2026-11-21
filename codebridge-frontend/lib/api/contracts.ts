import axios from "axios";
import { api } from "@/lib/api/axios";
import type { Contract, CreateContractInput } from "@/types/contract";

export const contractsApi = {
  create: (input: CreateContractInput) =>
    api.post<Contract>("/contracts", input).then((response) => response.data),

  byId: (contractId: number) =>
    api.get<Contract>(`/contracts/${contractId}`).then((response) => response.data),

  byProject: async (projectId: number) => {
    try {
      const response = await api.get<Contract>(`/contracts/project/${projectId}`);
      return response.data;
    } catch (error) {
      const message = axios.isAxiosError<{ message?: string }>(error)
        ? error.response?.data?.message
        : undefined;
      if (
        axios.isAxiosError(error) &&
        error.response?.status === 400 &&
        message?.includes("قراردادی برای این پروژه یافت نشد")
      ) {
        return null;
      }
      throw error;
    }
  },

  byDeveloper: (developerId: number) =>
    api.get<Contract[]>(`/contracts/developer/${developerId}`).then((response) => response.data),

  byEmployer: (employerId: number) =>
    api.get<Contract[]>(`/contracts/employer/${employerId}`).then((response) => response.data),

  sign: (contractId: number) =>
    api.patch(`/contracts/${contractId}/sign`).then((response) => response.data),

  signNda: (contractId: number) =>
    api.post(`/contracts/${contractId}/sign-nda`).then((response) => response.data),

  complete: (contractId: number) =>
    api.patch(`/contracts/${contractId}/complete`).then((response) => response.data)
};
