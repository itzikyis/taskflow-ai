import { useMutation } from '@tanstack/react-query';
import { aiService } from '@/services/aiService';

export function useSuggestDescription() {
  return useMutation({
    mutationFn: (taskTitle: string) => aiService.suggestDescription(taskTitle),
  });
}

export function useSuggestDueDate() {
  return useMutation({
    mutationFn: ({ taskTitle, taskDescription }: { taskTitle: string; taskDescription?: string }) =>
      aiService.suggestDueDate(taskTitle, taskDescription),
  });
}

export function useSummarizeComments() {
  return useMutation({
    mutationFn: (comments: string[]) => aiService.summarizeComments(comments),
  });
}
