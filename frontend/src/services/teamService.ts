import axios from 'axios';
import type { Team, CreateTeamPayload, AddMemberPayload, TeamRole } from '@/features/teams/types/team.types';

const BASE = '/api/teams';

export const teamService = {
  getAll: async (): Promise<Team[]> => {
    const { data } = await axios.get<Team[]>(BASE);
    return data;
  },

  getById: async (id: string): Promise<Team> => {
    const { data } = await axios.get<Team>(`${BASE}/${id}`);
    return data;
  },

  create: async (payload: CreateTeamPayload): Promise<string> => {
    const { data } = await axios.post<string>(BASE, payload);
    return data;
  },

  remove: async (id: string): Promise<void> => {
    await axios.delete(`${BASE}/${id}`);
  },

  addMember: async (teamId: string, payload: AddMemberPayload): Promise<void> => {
    await axios.post(`${BASE}/${teamId}/members`, payload);
  },

  removeMember: async (teamId: string, userId: string): Promise<void> => {
    await axios.delete(`${BASE}/${teamId}/members/${userId}`);
  },

  updateMemberRole: async (teamId: string, userId: string, role: TeamRole): Promise<void> => {
    await axios.put(`${BASE}/${teamId}/members/${userId}/role`, { role });
  },

  rename: async (teamId: string, name: string): Promise<void> => {
    await axios.patch(`${BASE}/${teamId}/name`, { name });
  },
};
