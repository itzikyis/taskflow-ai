import { useQuery } from '@tanstack/react-query';
import { auditService } from '@/services/auditService';

export function useRecentAudit(page: number) {
  return useQuery({
    queryKey: ['audit', 'recent', page],
    queryFn: () => auditService.getRecent(page),
  });
}

export function useEntityAudit(entityType: string, entityId: string) {
  return useQuery({
    queryKey: ['audit', 'entity', entityType, entityId],
    queryFn: () => auditService.getByEntity(entityType, entityId),
    enabled: Boolean(entityType && entityId),
  });
}

export function useActorAudit(actorId: string, page: number) {
  return useQuery({
    queryKey: ['audit', 'actor', actorId, page],
    queryFn: () => auditService.getByActor(actorId, page),
    enabled: Boolean(actorId),
  });
}
