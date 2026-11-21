"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { contractsApi } from "@/lib/api/contracts";
import type { CreateContractInput } from "@/types/contract";

export const contractKeys = {
  all: ["contracts"] as const,
  project: (projectId: number) => ["contracts", "project", projectId] as const,
  developer: (developerId: number) => ["contracts", "developer", developerId] as const,
  employer: (employerId: number) => ["contracts", "employer", employerId] as const
};

export function useProjectContract(projectId?: number) {
  return useQuery({
    queryKey: contractKeys.project(projectId ?? 0),
    queryFn: () => contractsApi.byProject(projectId as number),
    enabled: Boolean(projectId)
  });
}

export function useCreateContract() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: CreateContractInput) => contractsApi.create(input),
    onSuccess: (contract) => {
      void queryClient.invalidateQueries({ queryKey: contractKeys.project(contract.projectId) });
      void queryClient.invalidateQueries({ queryKey: ["projects"] });
    }
  });
}

function useInvalidateContract(projectId: number) {
  const queryClient = useQueryClient();
  return () => {
    void queryClient.invalidateQueries({ queryKey: contractKeys.project(projectId) });
    void queryClient.invalidateQueries({ queryKey: contractKeys.all });
    void queryClient.invalidateQueries({ queryKey: ["projects"] });
    void queryClient.invalidateQueries({ queryKey: ["wallets"] });
  };
}

export function useSignContract(projectId: number) {
  const invalidate = useInvalidateContract(projectId);
  return useMutation({
    mutationFn: (contractId: number) => contractsApi.sign(contractId),
    onSuccess: invalidate
  });
}

export function useSignNda(projectId: number) {
  const invalidate = useInvalidateContract(projectId);
  return useMutation({
    mutationFn: (contractId: number) => contractsApi.signNda(contractId),
    onSuccess: invalidate
  });
}

export function useCompleteContract(projectId: number) {
  const invalidate = useInvalidateContract(projectId);
  return useMutation({
    mutationFn: (contractId: number) => contractsApi.complete(contractId),
    onSuccess: invalidate
  });
}
