import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import {
  GetFamilyMembersResponse,
  AddFamilyMemberCommand,
  UpdateFamilyMemberProfileCommand,
  UpdateFamilyMemberClaimedUserCommand,
  ApiResponse,
} from './types';

export function useRosterMembers(treeId: string) {
  return useQuery({
    queryKey: ['roster', treeId],
    queryFn: async () => {
      const res = await apiClient<ApiResponse<GetFamilyMembersResponse>>(`trees/${treeId}/members`);
      return res.value;
    },
    enabled: !!treeId,
  });
}

export function useAddMember() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (command: AddFamilyMemberCommand) => {
      const res = await apiClient<ApiResponse<{ familyMemberId: string }>>(`trees/${command.treeId}/members`, {
        method: 'POST',
        body: JSON.stringify(command),
      });
      return res;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['roster', variables] });
    },
  });
}

export function useUpdateMemberProfile() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (command: UpdateFamilyMemberProfileCommand) => {
      const res = await apiClient<ApiResponse<void>>(`trees/${command.treeId}/members/${command.familyMemberId}/profile`, {
        method: 'PUT',
        body: JSON.stringify(command),
      });
      return res;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['roster', variables] });
      queryClient.invalidateQueries({ queryKey: ['canvas', variables] });
    },
  });
}

export function useUpdateMemberClaim() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (command: UpdateFamilyMemberClaimedUserCommand) => {
      const res = await apiClient<ApiResponse<void>>(`trees/${command.treeId}/members/${command.familyMemberId}/user`, {
        method: 'PUT',
        body: JSON.stringify(command),
      });
      return res.value;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['roster', variables.treeId] });
    },
  });
}

export function useDeleteMember() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async ({ treeId, memberId }: { treeId: string; memberId: string }) => {
      const res = await apiClient<ApiResponse<void>>(`trees/${treeId}/members/${memberId}`, {
        method: 'DELETE',
      });
      return res.value;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['roster', variables.treeId] });
      queryClient.invalidateQueries({ queryKey: ['canvas', variables.treeId] });
    },
  });
}
