import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { timeService } from '@/services/timeService';

const TIME_KEY = 'time-entries' as const;

export function useTaskTime(taskId: string) {
  return useQuery({
    queryKey: [TIME_KEY, taskId],
    queryFn: () => timeService.getSummary(taskId),
    enabled: Boolean(taskId),
  });
}

export function useLogTime(taskId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ minutes, note }: { minutes: number; note?: string }) =>
      timeService.log(taskId, minutes, note),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [TIME_KEY, taskId] }),
  });
}

export function useDeleteTimeEntry(taskId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (entryId: string) => timeService.remove(entryId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [TIME_KEY, taskId] }),
  });
}
