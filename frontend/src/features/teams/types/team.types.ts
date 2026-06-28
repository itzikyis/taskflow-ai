export const TEAM_ROLES = ['Viewer', 'Member', 'Admin'] as const;
export type TeamRole = (typeof TEAM_ROLES)[number];

export interface TeamMember {
  userId: string;
  role: TeamRole;
  joinedAt: string;
}

export interface Team {
  id: string;
  name: string;
  description: string | null;
  createdAt: string;
  members: TeamMember[];
}

export interface CreateTeamPayload {
  name: string;
  description?: string;
}

export interface AddMemberPayload {
  userId: string;
  role: TeamRole;
}
