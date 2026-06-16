import axios from 'axios';
import type { Attachment, AddAttachmentPayload, DeleteAttachmentPayload } from '@/features/attachments/types/attachment.types';

export const attachmentService = {
  getByTask: async (taskId: string): Promise<Attachment[]> => {
    const { data } = await axios.get<Attachment[]>(`/api/tasks/${taskId}/attachments`);
    return data;
  },
  add: async (taskId: string, payload: AddAttachmentPayload): Promise<string> => {
    const { data } = await axios.post<string>(`/api/tasks/${taskId}/attachments`, payload);
    return data;
  },
  remove: async (id: string, payload: DeleteAttachmentPayload): Promise<void> => {
    await axios.delete(`/api/attachments/${id}`, { data: payload });
  },
};
