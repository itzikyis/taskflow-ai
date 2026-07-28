import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { developmentLinkService } from '@/services/developmentLinkService';
import type { CreateDevelopmentLinkPayload } from '@/services/developmentLinkService';

const DEV_LINKS_KEY = 'development-links' as const;

export function useDevelopmentLinks(taskId: string) {
  return useQuery({
    queryKey: [DEV_LINKS_KEY, taskId],
    queryFn: () => developmentLinkService.getByTask(taskId),
    enabled: Boolean(taskId),
  });
}

export function useCreateDevelopmentLink(taskId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateDevelopmentLinkPayload) =>
      developmentLinkService.create(taskId, payload),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: [DEV_LINKS_KEY, taskId] }),
  });
}

export function useRemoveDevelopmentLink(taskId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (linkId: string) =>
      developmentLinkService.remove(taskId, linkId),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: [DEV_LINKS_KEY, taskId] }),
  });
}
