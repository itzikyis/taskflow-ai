import { useState } from 'react';
import type { Team, TeamRole } from '../types/team.types';
import { TEAM_ROLES } from '../types/team.types';
import { useAddMember, useRemoveMember, useUpdateMemberRole } from '../hooks/useTeams';
import { useAuthStore } from '@/store/authStore';

const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

interface TeamMembersPanelProps {
  team: Team;
}

const ROLE_STYLE: Record<TeamRole, { color: string; bg: string }> = {
  Admin:  { color: '#7c3aed',                    bg: '#ede9fe' },
  Member: { color: 'var(--color-primary)',        bg: 'var(--color-primary-light)' },
  Viewer: { color: 'var(--text-muted)',           bg: 'var(--surface-bg)' },
};

export function TeamMembersPanel({ team }: TeamMembersPanelProps) {
  const [newUserId, setNewUserId]   = useState('');
  const [newRole, setNewRole]       = useState<TeamRole>('Member');
  const [hoveredId, setHoveredId]   = useState<string | null>(null);
  const [uuidError, setUuidError]   = useState('');

  const { token } = useAuthStore();
  const addMember        = useAddMember(team.id);
  const removeMember     = useRemoveMember(team.id);
  const updateMemberRole = useUpdateMemberRole(team.id);

  const handleAdd = (e: React.FormEvent) => {
    e.preventDefault();
    const id = newUserId.trim();
    if (!id) return;
    if (!UUID_REGEX.test(id)) {
      setUuidError('Must be a valid UUID (e.g. paste a user ID from the profile)');
      return;
    }
    setUuidError('');
    addMember.mutate(
      { userId: id, role: newRole },
      { onSuccess: () => { setNewUserId(''); setNewRole('Member'); } },
    );
  };

  return (
    <div style={{ marginTop: 12, borderTop: '1px solid var(--border-color)', paddingTop: 12 }}>
      <p style={{ fontSize: 12, fontWeight: 600, color: 'var(--text-secondary)', marginBottom: 10, textTransform: 'uppercase', letterSpacing: '0.04em' }}>
        Members ({team.members.length})
      </p>

      <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
        {team.members.map(m => {
          const style = ROLE_STYLE[m.role];
          const isHovered = hoveredId === m.userId;
          return (
            <div
              key={m.userId}
              onMouseEnter={() => setHoveredId(m.userId)}
              onMouseLeave={() => setHoveredId(null)}
              style={{ display: 'flex', alignItems: 'center', gap: 8, padding: '4px 0' }}
            >
              <div style={{
                width: 28, height: 28, borderRadius: '50%',
                background: 'var(--color-primary-light)',
                color: 'var(--color-primary)',
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                fontSize: 11, fontWeight: 700, flexShrink: 0,
              }}>
                {m.userId.slice(0, 2).toUpperCase()}
              </div>

              <span style={{ fontSize: 13, color: 'var(--text-primary)', flex: 1, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                {m.userId.length > 18 ? `${m.userId.slice(0, 18)}…` : m.userId}
              </span>

              <select
                value={m.role}
                onChange={e => updateMemberRole.mutate({ userId: m.userId, role: e.target.value as TeamRole })}
                style={{
                  fontSize: 11, fontWeight: 600,
                  color: style.color, background: style.bg,
                  border: 'none', borderRadius: 4, padding: '2px 4px', cursor: 'pointer',
                }}
              >
                {TEAM_ROLES.map(r => (
                  <option key={r} value={r}>{r}</option>
                ))}
              </select>

              <span style={{ fontSize: 11, color: 'var(--text-muted)', whiteSpace: 'nowrap' }}>
                {new Date(m.joinedAt).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })}
              </span>

              <button
                type="button"
                onClick={() => removeMember.mutate(m.userId)}
                style={{
                  background: 'none', border: 'none', cursor: 'pointer',
                  color: 'var(--color-danger)', fontSize: 14, padding: '0 2px',
                  opacity: isHovered ? 1 : 0,
                  transition: 'opacity 0.15s',
                  lineHeight: 1,
                }}
                aria-label={`Remove ${m.userId}`}
              >
                ×
              </button>
            </div>
          );
        })}
      </div>

      <form onSubmit={handleAdd} style={{ marginTop: 10 }}>
        <div style={{ display: 'flex', gap: 6 }}>
          <input
            className="tf-input"
            type="text"
            value={newUserId}
            onChange={e => { setNewUserId(e.target.value); setUuidError(''); }}
            placeholder="Paste user UUID…"
            style={{ fontSize: 13, flex: 1, borderColor: uuidError ? 'var(--color-danger)' : undefined }}
          />
          {token?.userId && (
            <button
              type="button"
              className="tf-btn tf-btn-ghost tf-btn-sm"
              title="Copy your own user ID"
              onClick={() => { setNewUserId(token.userId); setUuidError(''); }}
              style={{ flexShrink: 0, fontSize: 11 }}
            >
              My ID
            </button>
          )}
          <select
            className="tf-input"
            value={newRole}
            onChange={e => setNewRole(e.target.value as TeamRole)}
            style={{ fontSize: 13, width: 90 }}
          >
            {TEAM_ROLES.map(r => (
              <option key={r} value={r}>{r}</option>
            ))}
          </select>
          <button
            type="submit"
            className="tf-btn tf-btn-primary tf-btn-sm"
            disabled={addMember.isPending || !newUserId.trim()}
          >
            {addMember.isPending ? '…' : 'Add'}
          </button>
        </div>
        {uuidError && (
          <p style={{ fontSize: 11, color: 'var(--color-danger)', marginTop: 4 }}>{uuidError}</p>
        )}
      </form>
    </div>
  );
}
