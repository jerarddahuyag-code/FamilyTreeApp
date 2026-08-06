import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import {
  GetCanvasQueryResponse,
  UpdateCanvasCommand,
  AddTreeNodeCommand,
  AddTreeEdgeCommand,
  ApiResponse,
} from './types';

export function useCanvas(treeId: string) {
  return useQuery({
    queryKey: ['canvas', treeId],
    queryFn: async () => {
      const res = await apiClient<ApiResponse<GetCanvasQueryResponse>>(`trees/${treeId}/canvas`);
      return res.value;
    },
    enabled: !!treeId,
  });
}

export function useUpdateCanvas() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (command: UpdateCanvasCommand) => {
      const res = await apiClient<ApiResponse<void>>(`trees/${command.treeId}/canvas`, {
        method: 'PUT',
        body: JSON.stringify(command),
      });
      return res.value;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['canvas', variables.treeId] });
    },
  });
}

export function useAddTreeNode() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (command: AddTreeNodeCommand) => {
      const res = await apiClient<ApiResponse<{ nodeId: string }>>(`trees/${command.treeId}/canvas/nodes`, {
        method: 'POST',
        body: JSON.stringify(command),
      });
      return res;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['canvas', variables] });
    },
  });
}

export function useAddTreeEdge() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (command: AddTreeEdgeCommand) => {
      const res = await apiClient<ApiResponse<{ edgeId: string }>>(`trees/${command.treeId}/canvas/edges`, {
        method: 'POST',
        body: JSON.stringify(command),
      });
      return res.value;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['canvas', variables.treeId] });
    },
  });
}

export function useRemoveTreeNode() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async ({ treeId, nodeId }: { treeId: string; nodeId: string }) => {
      const res = await apiClient<ApiResponse<void>>(`trees/${treeId}/canvas/nodes/${nodeId}`, {
        method: 'DELETE',
      });
      return res.value;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['canvas', variables.treeId] });
    },
  });
}

export function useRemoveTreeEdge() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async ({ treeId, edgeId }: { treeId: string; edgeId: string }) => {
      const res = await apiClient<ApiResponse<void>>(`trees/${treeId}/canvas/edges/${edgeId}`, {
        method: 'DELETE',
      });
      return res.value;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['canvas', variables.treeId] });
    },
  });
}
