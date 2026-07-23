import axios from 'axios';

export interface ReleaseNotes {
  version: string;
  summary: string;
  features: string[];
  bugFixes: string[];
  improvements: string[];
  markdownContent: string;
}

export interface AiSuggestion {
  suggestion: string;
}

export interface StoryPointEstimate {
  points: number;
  reasoning: string;
}

export interface SubtaskSuggestion {
  title: string;
  description: string | null;
}

export interface RetroTaskInput {
  title: string;
  description?: string;
  priority: string;
}

export interface SprintRetrospective {
  summary: string;
  wentWell: string[];
  issues: string[];
  estimateAccuracyNotes: string[];
  actionItems: string[];
}

export const aiService = {
  suggestDescription: async (taskTitle: string): Promise<AiSuggestion> => {
    const { data } = await axios.post<AiSuggestion>('/api/ai/suggest-description', { taskTitle });
    return data;
  },
  suggestDueDate: async (taskTitle: string, taskDescription?: string): Promise<AiSuggestion> => {
    const { data } = await axios.post<AiSuggestion>('/api/ai/suggest-due-date', { taskTitle, taskDescription });
    return data;
  },
  summarizeComments: async (comments: string[]): Promise<AiSuggestion> => {
    const { data } = await axios.post<AiSuggestion>('/api/ai/summarize-comments', { comments });
    return data;
  },
  estimateStoryPoints: async (title: string, description?: string): Promise<StoryPointEstimate> => {
    const { data } = await axios.post<StoryPointEstimate>('/api/ai/story-points', { title, description });
    return data;
  },
  taskBreakdown: async (title: string, description?: string): Promise<SubtaskSuggestion[]> => {
    const { data } = await axios.post<SubtaskSuggestion[]>('/api/ai/task-breakdown', { title, description });
    return data;
  },
  generateRetrospective: async (
    completed: RetroTaskInput[],
    incomplete: RetroTaskInput[],
  ): Promise<SprintRetrospective> => {
    const { data } = await axios.post<SprintRetrospective>('/api/ai/retrospective', { completed, incomplete });
    return data;
  },
  generateReleaseNotes: async (
    version: string,
    completedTasks: Array<{ title: string; description?: string; priority: string }>,
  ): Promise<ReleaseNotes> => {
    const { data } = await axios.post<ReleaseNotes>('/api/ai/release-notes', { version, completedTasks });
    return data;
  },
  assessRisk: async (tasks: RiskTaskInput[]): Promise<SprintRiskAssessment> => {
    const { data } = await axios.post<SprintRiskAssessment>('/api/ai/risk-assessment', { tasks });
    return data;
  },
  analyzeMeetingNotes: async (transcript: string, participants: string[]): Promise<MeetingNotesResult> => {
    const { data } = await axios.post<MeetingNotesResult>('/api/ai/meeting-notes', { transcript, participants });
    return data;
  },
  askCopilot: async (
    question: string,
    tasks: CopilotTaskContext[],
    conversationHistory: string[],
  ): Promise<CopilotAnswer> => {
    const { data } = await axios.post<CopilotAnswer>('/api/ai/copilot', { question, tasks, conversationHistory });
    return data;
  },
  suggestSprintPlan: async (
    backlog: Array<{ id: string; title: string; description?: string; priority: string; status: string }>,
    sprintCapacity = 40,
  ): Promise<SprintPlan> => {
    const { data } = await axios.post<SprintPlan>('/api/ai/sprint-plan', { backlog, sprintCapacity });
    return data;
  },
  getDashboardInsights: async (projectId: string): Promise<DashboardInsightsDto> => {
    const { data } = await axios.get<DashboardInsightsDto>(`/api/ai/dashboard-insights/${projectId}`);
    return data;
  },
  getStatusDigest: async (projectId: string, periodDays = 7): Promise<StatusDigestDto> => {
    const { data } = await axios.get<StatusDigestDto>(
      `/api/ai/status-digest/${projectId}`,
      { params: { periodDays } },
    );
    return data;
  },
};

export interface SprintTaskSuggestion {
  taskId: string;
  title: string;
  estimatedPoints: number;
  justification: string;
}

export interface SprintPlan {
  sprintGoal: string;
  suggestedTasks: SprintTaskSuggestion[];
  reasoning: string;
}

export type RiskLevel = 'OnTrack' | 'AtRisk' | 'Blocked';

export interface TaskRiskScore {
  taskId: string;
  title: string;
  level: RiskLevel;
  reason: string;
}

export interface SprintRiskAssessment {
  tasks: TaskRiskScore[];
  onTrackCount: number;
  atRiskCount: number;
  blockedCount: number;
  summary: string;
  recommendations: string[];
}

export interface MeetingActionItem {
  title: string;
  description: string;
  priority: string;
  suggestedAssignee: string | null;
  suggestedDueDate: string | null;
}

export interface MeetingNotesResult {
  summary: string;
  keyDecisions: string[];
  actionItems: MeetingActionItem[];
}

export interface CopilotTaskContext {
  id: string;
  title: string;
  description?: string | null;
  status: string;
  priority: string;
  dueDate?: string | null;
  openBlockerCount: number;
  recentComments: string[];
}

export interface CopilotAnswer {
  answer: string;
  referencedTaskIds: string[];
}

export interface RiskTaskInput {
  id: string;
  title: string;
  status: string;
  priority: string;
  createdAt: string;
  dueDate?: string | null;
  updatedAt?: string | null;
  openBlockerCount: number;
}

export interface DashboardInsightsDto {
  narrative: string;
  highlights: string[];
  healthStatus: string;
}

export interface StatusDigestDto {
  periodLabel: string;
  completed: string[];
  inProgress: string[];
  blockers: string[];
  aiNarrative: string;
  healthStatus: string;
}
