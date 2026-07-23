import { useQuery } from '@tanstack/react-query';
import { timesheetService } from '@/services/timeService';

export function useTimesheet(userId: string, weekStart: string) {
  return useQuery({
    queryKey: ['timesheet', userId, weekStart],
    queryFn: () => timesheetService.getTimesheet(userId, weekStart),
    enabled: Boolean(userId) && Boolean(weekStart),
    staleTime: 30_000,
  });
}
