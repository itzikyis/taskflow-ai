import axios from 'axios';

/** A single task returned by the natural-language search endpoint. */
export interface TaskSummaryDto {
  id: string;
  title: string;
  description: string | null;
  status: string;
  priority: string;
  dueDate: string | null;
  assignedToUserId: string | null;
}

/** Response from POST /api/ai/search. */
export interface NaturalLanguageSearchResult {
  tasks: TaskSummaryDto[];
  interpretation: string;
}

/**
 * Sends a natural-language search query to the backend and returns matching
 * tasks together with a human-readable interpretation of the query.
 */
export async function search(
  query: string,
  projectId?: string,
): Promise<NaturalLanguageSearchResult> {
  const { data } = await axios.post<NaturalLanguageSearchResult>('/api/ai/search', {
    query,
    projectId: projectId ?? null,
  });
  return data;
}
