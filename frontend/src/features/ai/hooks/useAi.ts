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

export function useEstimateStoryPoints() {
  return useMutation({
    mutationFn: ({ title, description }: { title: string; description?: string }) =>
      aiService.estimateStoryPoints(title, description),
  });
}

export function useSuggestSprintPlan() {
  return useMutation({
    mutationFn: ({
      backlog,
      sprintCapacity,
    }: {
      backlog: Array<{ id: string; title: string; description?: string; priority: string; status: string }>;
      sprintCapacity?: number;
    }) => aiService.suggestSprintPlan(backlog, sprintCapacity),
  });
}
