import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { activityService } from '@/services/activityService';
import type { LogActivityPayload } from '@/services/activityService';

const ACTIVITY_KEY = 'activity' as const;

export function useRecentActivity(page = 1) {
  return useQuery({
    queryKey: [ACTIVITY_KEY, 'recent', page],
    queryFn: () => activityService.getRecent(page),
    staleTime: 30_000,
  });
}

export function useEntityActivity(entityType: string, entityId: string, page = 1) {
  return useQuery({
    queryKey: [ACTIVITY_KEY, 'entity', entityType, entityId, page],
    queryFn: () => activityService.getByEntity(entityType, entityId, page),
    enabled: Boolean(entityType) && Boolean(entityId),
  });
}

export function useProjectActivity(projectId: string, page = 1) {
  return useQuery({
    queryKey: [ACTIVITY_KEY, 'project', projectId, page],
    queryFn: () => activityService.getByProject(projectId, page),
    enabled: Boolean(projectId),
  });
}

export function useActorActivity(actorId: string, page = 1) {
  return useQuery({
    queryKey: [ACTIVITY_KEY, 'actor', actorId, page],
    queryFn: () => activityService.getByActor(actorId, page),
    enabled: Boolean(actorId),
  });
}

export function useLogActivity() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: LogActivityPayload) => activityService.log(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [ACTIVITY_KEY] }),
  });
}
