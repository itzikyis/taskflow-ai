import axios from 'axios';

export interface WeeklyVelocityDto {
  weekLabel: string;
  completed: number;
}

export interface ProjectAnalyticsDto {
  totalTasks: number;
  completedTasks: number;
  inProgressTasks: number;
  openTasks: number;
  overdueTasks: number;
  weeklyVelocity: WeeklyVelocityDto[];
}

export interface WorkloadItem {
  userId: string | null;
  openTasks: number;
}

export interface CompletionPoint {
  date: string;
  completed: number;
}

export interface DashboardMetrics {
  total: number;
  todo: number;
  inProgress: number;
  inReview: number;
  done: number;
  low: number;
  medium: number;
  high: number;
  critical: number;
  workload: WorkloadItem[];
  completionTrend: CompletionPoint[];
}

export const reportingService = {
  getDashboard: async (): Promise<DashboardMetrics> => {
    const { data } = await axios.get<DashboardMetrics>('/api/reporting/dashboard');
    return data;
  },

  getProjectAnalytics: async (projectId: string): Promise<ProjectAnalyticsDto> => {
    const { data } = await axios.get<ProjectAnalyticsDto>(`/api/reporting/analytics/${projectId}`);
    return data;
  },
};
