import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { commentService } from '@/services/commentService';
import type {
  AddCommentPayload,
  EditCommentPayload,
  DeleteCommentPayload,
} from '../types/comment.types';

const KEY = 'comments';

/** Returns all comments for the given task. */
export function useCommentsByTask(taskId: string) {
  return useQuery({
    queryKey: [KEY, taskId],
    queryFn: () => commentService.getByTask(taskId),
    enabled: Boolean(taskId),
  });
}

/** Mutation to add a comment to the given task. */
export function useAddComment(taskId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (p: AddCommentPayload) => commentService.add(taskId, p),
    onSuccess: () => qc.invalidateQueries({ queryKey: [KEY, taskId] }),
  });
}

/** Mutation to edit an existing comment. */
export function useEditComment(taskId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: EditCommentPayload }) =>
      commentService.edit(id, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: [KEY, taskId] }),
  });
}

/** Mutation to delete a comment. */
export function useDeleteComment(taskId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: DeleteCommentPayload }) =>
      commentService.remove(id, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: [KEY, taskId] }),
  });
}
