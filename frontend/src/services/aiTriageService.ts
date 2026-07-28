import axios from 'axios';

/** A potential duplicate task returned by the AI triage endpoint. */
export interface PotentialDuplicate {
  taskId: string;
  title: string;
  similarityScore: number;
}

/** Result of POST /api/ai/triage. */
export interface TaskTriageResult {
  suggestedPriority: string;
  reasoning: string;
  potentialDuplicates: PotentialDuplicate[];
}

/** Request body for POST /api/ai/triage. */
export interface TriageByContentPayload {
  title: string;
  description?: string;
  projectId: string;
}

const BASE = '/api/ai/triage';

/** Calls the AI triage endpoint with the task's current title, description, and project. */
export async function triageTaskByContent(
  payload: TriageByContentPayload,
): Promise<TaskTriageResult> {
  const { data } = await axios.post<TaskTriageResult>(BASE, payload);
  return data;
}
