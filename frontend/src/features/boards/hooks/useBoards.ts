import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { boardService } from '@/services/boardService';
import type { CreateBoardPayload, UpdateBoardPayload, AddColumnPayload } from '../types/board.types';

const KEY = 'boards';

export function useBoardsByProject(projectId: string) {
  return useQuery({
    queryKey: [KEY, 'project', projectId],
    queryFn: () => boardService.getByProject(projectId),
    enabled: Boolean(projectId),
  });
}

export function useBoard(id: string) {
  return useQuery({
    queryKey: [KEY, id],
    queryFn: () => boardService.getById(id),
    enabled: Boolean(id),
  });
}

export function useCreateBoard() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: boardService.create,
    onSuccess: () => qc.invalidateQueries({ queryKey: [KEY] }),
  });
}

export function useUpdateBoard(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (p: UpdateBoardPayload) => boardService.update(id, p),
    onSuccess: () => qc.invalidateQueries({ queryKey: [KEY] }),
  });
}

export function useDeleteBoard() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: boardService.remove,
    onSuccess: () => qc.invalidateQueries({ queryKey: [KEY] }),
  });
}

export function useAddColumn(boardId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (p: AddColumnPayload) => boardService.addColumn(boardId, p),
    onSuccess: () => qc.invalidateQueries({ queryKey: [KEY] }),
  });
}

export function useRemoveColumn(boardId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (colId: string) => boardService.removeColumn(boardId, colId),
    onSuccess: () => qc.invalidateQueries({ queryKey: [KEY] }),
  });
}
