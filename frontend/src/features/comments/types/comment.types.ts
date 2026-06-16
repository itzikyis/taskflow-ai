export interface Comment {
  id: string;
  taskId: string;
  authorId: string;
  content: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface AddCommentPayload {
  authorId: string;
  content: string;
}

export interface EditCommentPayload {
  requesterId: string;
  content: string;
}

export interface DeleteCommentPayload {
  requesterId: string;
}
