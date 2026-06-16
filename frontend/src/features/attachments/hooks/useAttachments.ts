import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { attachmentService } from '@/services/attachmentService';
import type { AddAttachmentPayload, DeleteAttachmentPayload } from '../types/attachment.types';

const KEY = 'attachments';

export function useAttachmentsByTask(taskId: string) {
  return useQuery({
    queryKey: [KEY, taskId],
    queryFn: () => attachmentService.getByTask(taskId),
    enabled: Boolean(taskId),
  });
}

export function useAddAttachment(taskId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (p: AddAttachmentPayload) => attachmentService.add(taskId, p),
    onSuccess: () => qc.invalidateQueries({ queryKey: [KEY, taskId] }),
  });
}

export function useDeleteAttachment(taskId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: DeleteAttachmentPayload }) =>
      attachmentService.remove(id, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: [KEY, taskId] }),
  });
}
