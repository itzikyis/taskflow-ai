import axios from 'axios';
import type { Board, CreateBoardPayload, UpdateBoardPayload, AddColumnPayload } from '@/features/boards/types/board.types';

export const boardService = {
  getByProject: async (projectId: string): Promise<Board[]> => {
    const { data } = await axios.get<Board[]>(`/api/projects/${projectId}/boards`);
    return data;
  },
  getById: async (id: string): Promise<Board> => {
    const { data } = await axios.get<Board>(`/api/boards/${id}`);
    return data;
  },
  create: async (payload: CreateBoardPayload): Promise<string> => {
    const { data } = await axios.post<string>('/api/boards', payload);
    return data;
  },
  update: async (id: string, payload: UpdateBoardPayload): Promise<void> => {
    await axios.put(`/api/boards/${id}`, payload);
  },
  remove: async (id: string): Promise<void> => {
    await axios.delete(`/api/boards/${id}`);
  },
  addColumn: async (boardId: string, payload: AddColumnPayload): Promise<string> => {
    const { data } = await axios.post<string>(`/api/boards/${boardId}/columns`, payload);
    return data;
  },
  removeColumn: async (boardId: string, columnId: string): Promise<void> => {
    await axios.delete(`/api/boards/${boardId}/columns/${columnId}`);
  },
};
