import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { dependencyService } from '@/services/dependencyService';

const DEP_KEY = 'dependencies' as const;

export function useTaskDependencies(taskId: string) {
  return useQuery({
    queryKey: [DEP_KEY, 'task', taskId],
    queryFn: () => dependencyService.getForTask(taskId),
    enabled: Boolean(taskId),
  });
}

export function useAllDependencies() {
  return useQuery({
    queryKey: [DEP_KEY, 'all'],
    queryFn: () => dependencyService.getAll(),
  });
}

export function useAddDependency(taskId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (blockedByTaskId: string) => dependencyService.add(taskId, blockedByTaskId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [DEP_KEY] }),
  });
}

export function useRemoveDependency() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (dependencyId: string) => dependencyService.remove(dependencyId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [DEP_KEY] }),
  });
}
