import axios from 'axios';
import type {
  Comment,
  AddCommentPayload,
  EditCommentPayload,
  DeleteCommentPayload,
} from '@/features/comments/types/comment.types';

export const commentService = {
  getByTask: async (taskId: string): Promise<Comment[]> => {
    const { data } = await axios.get<Comment[]>(`/api/tasks/${taskId}/comments`);
    return data;
  },

  add: async (taskId: string, payload: AddCommentPayload): Promise<string> => {
    const { data } = await axios.post<string>(`/api/tasks/${taskId}/comments`, payload);
    return data;
  },

  edit: async (id: string, payload: EditCommentPayload): Promise<void> => {
    await axios.put(`/api/comments/${id}`, payload);
  },

  remove: async (id: string, payload: DeleteCommentPayload): Promise<void> => {
    await axios.delete(`/api/comments/${id}`, { data: payload });
  },
};
