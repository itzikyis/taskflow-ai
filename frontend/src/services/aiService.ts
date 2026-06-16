import axios from 'axios';

export interface AiSuggestion {
  suggestion: string;
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
};
