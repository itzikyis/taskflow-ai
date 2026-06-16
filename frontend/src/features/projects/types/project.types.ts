export interface Project {
  id: string;
  name: string;
  description: string | null;
  ownerId: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateProjectPayload {
  name: string;
  description?: string;
  ownerId: string;
}

export interface UpdateProjectPayload {
  name: string;
  description?: string;
}
