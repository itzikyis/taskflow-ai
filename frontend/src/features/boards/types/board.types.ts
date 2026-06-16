export interface BoardColumn {
  id: string;
  name: string;
  order: number;
  wipLimit: number | null;
}

export interface Board {
  id: string;
  name: string;
  projectId: string;
  columns: BoardColumn[];
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateBoardPayload {
  name: string;
  projectId: string;
}

export interface UpdateBoardPayload {
  name: string;
}

export interface AddColumnPayload {
  name: string;
  order: number;
  wipLimit?: number;
}
