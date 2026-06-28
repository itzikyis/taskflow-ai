import { useState } from 'react';
import type { Team } from '../types/team.types';
import { useDeleteTeam } from '../hooks/useTeams';
import { TeamMembersPanel } from './TeamMembersPanel';

interface TeamCardProps {
  team: Team;
}

function teamColor(name: string): string {
  return `hsl(${(name.charCodeAt(0) * 37) % 360}, 55%, 50%)`;
}

export function TeamCard({ team }: TeamCardProps) {
  const [expanded, setExpanded]   = useState(false);
  const [confirming, setConfirming] = useState(false);
  const deleteTeam                 = useDeleteTeam();

  const handleDelete = () => {
    if (!confirming) { setConfirming(true); return; }
    deleteTeam.mutate(team.id);
  };

  const color = teamColor(team.name);

  return (
    <div
      className="tf-card"
      style={{ padding: 0, overflow: 'hidden', display: 'flex', flexDirection: 'column' }}
    >
      <div style={{ height: 4, background: color, flexShrink: 0 }} />

      <div style={{ padding: '14px 16px', display: 'flex', flexDirection: 'column', gap: 6, flex: 1 }}>
        <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', gap: 8 }}>
          <span style={{ fontSize: 15, fontWeight: 700, color: 'var(--text-primary)', lineHeight: 1.3 }}>
            {team.name}
          </span>
          <div style={{ display: 'flex', gap: 4, flexShrink: 0 }}>
            <button
              type="button"
              onClick={() => setExpanded(v => !v)}
              className="tf-btn tf-btn-ghost tf-btn-sm"
              aria-label={expanded ? 'Collapse members' : 'Expand members'}
            >
              {expanded ? '▲' : '▼'}
            </button>
            {confirming ? (
              <>
                <button
                  type="button"
                  onClick={handleDelete}
                  className="tf-btn tf-btn-danger tf-btn-sm"
                  disabled={deleteTeam.isPending}
                >
                  Confirm
                </button>
                <button
                  type="button"
                  onClick={() => setConfirming(false)}
                  className="tf-btn tf-btn-ghost tf-btn-sm"
                >
                  Cancel
                </button>
              </>
            ) : (
              <button
                type="button"
                onClick={handleDelete}
                className="tf-btn tf-btn-danger tf-btn-sm"
              >
                Delete
              </button>
            )}
          </div>
        </div>

        {team.description && (
          <p style={{ fontSize: 13, color: 'var(--text-secondary)', margin: 0, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
            {team.description}
          </p>
        )}

        <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginTop: 2 }}>
          <span
            className="tf-badge"
            style={{ background: `${color}22`, color, border: `1px solid ${color}44` }}
          >
            {team.members.length} {team.members.length === 1 ? 'member' : 'members'}
          </span>
          <span style={{ fontSize: 11, color: 'var(--text-muted)' }}>
            Created {new Date(team.createdAt).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })}
          </span>
        </div>

        {expanded && <TeamMembersPanel team={team} />}
      </div>
    </div>
  );
}
