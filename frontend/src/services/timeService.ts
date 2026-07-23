import axios from 'axios';

export interface TimeEntry {
  id: string;
  taskId: string;
  userId: string;
  minutes: number;
  note: string | null;
  loggedAt: string;
}

export interface TaskTimeSummary {
  taskId: string;
  totalMinutes: number;
  entries: TimeEntry[];
}

export const timeService = {
  getSummary: async (taskId: string): Promise<TaskTimeSummary> => {
    const { data } = await axios.get<TaskTimeSummary>(`/api/tasks/${taskId}/time-entries`);
    return data;
  },

  log: async (taskId: string, minutes: number, note?: string): Promise<string> => {
    const { data } = await axios.post<string>(`/api/tasks/${taskId}/time-entries`, { minutes, note });
    return data;
  },

  remove: async (entryId: string): Promise<void> => {
    await axios.delete(`/api/time-entries/${entryId}`);
  },
};

// ── Timesheet ─────────────────────────────────────────────────────────────────

export interface TimesheetRowDto {
  taskId: string;
  taskTitle: string;
  hoursByDay: number[];
}

export interface TimesheetDto {
  weekStart: string;
  rows: TimesheetRowDto[];
  totalByDay: number[];
  grandTotal: number;
}

export const timesheetService = {
  getTimesheet: async (userId: string, weekStart: string): Promise<TimesheetDto> => {
    const { data } = await axios.get<TimesheetDto>('/api/time-entries/timesheet', {
      params: { userId, weekStart },
    });
    return data;
  },
};

/** Formats minutes as "2h 30m" / "45m". */
export function formatMinutes(total: number): string {
  const h = Math.floor(total / 60);
  const m = total % 60;
  if (h === 0) return `${m}m`;
  if (m === 0) return `${h}h`;
  return `${h}h ${m}m`;
}
