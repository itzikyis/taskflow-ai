import { useQuery } from '@tanstack/react-query';
import { reportingService } from '@/services/reportingService';

export function useDashboardMetrics() {
  return useQuery({
    queryKey: ['reporting', 'dashboard'],
    queryFn: () => reportingService.getDashboard(),
    staleTime: 30_000,
  });
}
