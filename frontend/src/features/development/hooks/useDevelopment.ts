import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { developmentService } from '@/services/developmentService';
import type { CreateDevelopmentLinkPayload } from '../types/development.types';

const DEV_KEY = 'development' as const;

export function useDevelopmentLinks(taskId: string) {
  return useQuery({
    queryKey: [DEV_KEY, taskId],
    queryFn: () => developmentService.getByTask(taskId),
    enabled: Boolean(taskId),
  });
}

export function useLinkDevelopment(taskId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateDevelopmentLinkPayload) => developmentService.link(taskId, payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [DEV_KEY, taskId] }),
  });
}

export function useRemoveDevelopmentLink(taskId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (linkId: string) => developmentService.remove(linkId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [DEV_KEY, taskId] }),
  });
}
