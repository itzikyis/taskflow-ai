import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { teamService } from '@/services/teamService';
import type { CreateTeamPayload, AddMemberPayload, TeamRole } from '../types/team.types';

const TEAMS_KEY = 'teams' as const;

export function useTeams() {
  return useQuery({
    queryKey: [TEAMS_KEY],
    queryFn: () => teamService.getAll(),
  });
}

export function useTeam(id: string) {
  return useQuery({
    queryKey: [TEAMS_KEY, id],
    queryFn: () => teamService.getById(id),
    enabled: Boolean(id),
  });
}

export function useCreateTeam() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateTeamPayload) => teamService.create(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [TEAMS_KEY] }),
  });
}

export function useDeleteTeam() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => teamService.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [TEAMS_KEY] }),
  });
}

export function useRenameTeam() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ teamId, name }: { teamId: string; name: string }) =>
      teamService.rename(teamId, name),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [TEAMS_KEY] }),
  });
}

export function useAddMember(teamId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: AddMemberPayload) => teamService.addMember(teamId, payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [TEAMS_KEY] }),
  });
}

export function useRemoveMember(teamId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (userId: string) => teamService.removeMember(teamId, userId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [TEAMS_KEY] }),
  });
}

export function useUpdateMemberRole(teamId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: TeamRole }) =>
      teamService.updateMemberRole(teamId, userId, role),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [TEAMS_KEY] }),
  });
}
