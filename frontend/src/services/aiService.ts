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
  generateReleaseNotes: async (
    version: string,
    completedTasks: Array<{ title: string; description?: string; priority: string }>,
  ): Promise<ReleaseNotes> => {
    const { data } = await axios.post<ReleaseNotes>('/api/ai/release-notes', { version, completedTasks });
    return data;
  },
  suggestSprintPlan: async (
    backlog: Array<{ id: string; title: string; description?: string; priority: string; status: string }>,
    sprintCapacity = 40,
  ): Promise<SprintPlan> => {
    const { data } = await axios.post<SprintPlan>('/api/ai/sprint-plan', { backlog, sprintCapacity });
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
