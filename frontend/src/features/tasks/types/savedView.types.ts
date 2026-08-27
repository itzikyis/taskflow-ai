export interface TaskFilter {
  status?: string;
  priority?: string;
  assigneeId?: string;
  search?: string;
}

export interface SavedView {
  id: string;
  name: string;
  filter: TaskFilter;
  isPinned: boolean;
  createdAt: string;
}
