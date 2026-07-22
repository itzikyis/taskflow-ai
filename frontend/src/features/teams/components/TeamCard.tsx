import { useRef, useState } from 'react';
import type { Team } from '../types/team.types';
import { useDeleteTeam, useRenameTeam } from '../hooks/useTeams';
import { TeamMembersPanel } from './TeamMembersPanel';

interface TeamCardProps {
  team: Team;
}

function teamColor(name: string): string {
  return `hsl(${(name.charCodeAt(0) * 37) % 360}, 55%, 50%)`;
}

const INPUT_MAX_LENGTH = 100;

export function TeamCard({ team }: TeamCardProps) {
  const [expanded, setExpanded]     = useState(false);
  const [confirming, setConfirming] = useState(false);
  const [renaming, setRenaming]     = useState(false);
  const [draftName, setDraftName]   = useState('');
  const [renameError, setRenameError] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  const deleteTeam = useDeleteTeam();
  const renameTeam = useRenameTeam();

  const handleDelete = () => {
    if (!confirming) { setConfirming(true); return; }
    deleteTeam.mutate(team.id);
  };

  const startRename = () => {
    setDraftName(team.name);
    setRenameError(null);
    setRenaming(true);
    // focus after next paint
    setTimeout(() => inputRef.current?.select(), 0);
  };

  const cancelRename = () => {
    setRenaming(false);
    setRenameError(null);
  };

  const commitRename = () => {
    const trimmed = draftName.trim();
    if (!trimmed) return;
    setRenameError(null);
    renameTeam.mutate(
      { teamId: team.id, name: trimmed },
      {
        onSuccess: () => setRenaming(false),
        onError: () => setRenameError('Failed to rename team. Please try again.'),
      },
    );
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') { e.preventDefault(); commitRename(); }
    if (e.key === 'Escape') { e.preventDefault(); cancelRename(); }
  };

  const color = teamColor(team.name);
  const isSaving = renameTeam.isPending;
  const canSave = draftName.trim().length > 0;

  return (
    <div
      className="tf-card"
      style={{ padding: 0, overflow: 'hidden', display: 'flex', flexDirection: 'column' }}
    >
      <div style={{ height: 4, background: color, flexShrink: 0 }} />

      <div style={{ padding: '14px 16px', display: 'flex', flexDirection: 'column', gap: 6, flex: 1 }}>
        <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', gap: 8 }}>

          {renaming ? (
            <div style={{ display: 'flex', flexDirection: 'column', gap: 4, flex: 1, minWidth: 0 }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
                <input
                  ref={inputRef}
                  type="text"
                  value={draftName}
                  onChange={e => setDraftName(e.target.value)}
                  onKeyDown={handleKeyDown}
                  maxLength={INPUT_MAX_LENGTH}
                  disabled={isSaving}
                  aria-label="Team name"
                  className="tf-input"
                  style={{
                    flex: 1,
                    fontSize: 15,
                    fontWeight: 700,
                    padding: '2px 6px',
                    opacity: isSaving ? 0.6 : 1,
                  }}
                  autoFocus
                />
                <button
                  type="button"
                  onClick={commitRename}
                  disabled={!canSave || isSaving}
                  className="tf-btn tf-btn-primary tf-btn-sm"
                  aria-label="Save new name"
                  style={{ flexShrink: 0 }}
                >
                  {isSaving ? '…' : '✓'}
                </button>
                <button
                  type="button"
                  onClick={cancelRename}
                  disabled={isSaving}
                  className="tf-btn tf-btn-ghost tf-btn-sm"
                  aria-label="Cancel rename"
                  style={{ flexShrink: 0 }}
                >
                  ✗
                </button>
              </div>
              {renameError && (
                <span style={{ fontSize: 12, color: 'var(--danger, #e53e3e)' }}>
                  {renameError}
                </span>
              )}
            </div>
          ) : (
            <div style={{ display: 'flex', alignItems: 'center', gap: 4, flex: 1, minWidth: 0 }}>
              <span
                style={{
                  fontSize: 15,
                  fontWeight: 700,
                  color: 'var(--text-primary)',
                  lineHeight: 1.3,
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                  whiteSpace: 'nowrap',
                }}
              >
                {team.name}
              </span>
              <button
                type="button"
                onClick={startRename}
                className="tf-btn tf-btn-ghost tf-btn-sm"
                aria-label={`Rename team ${team.name}`}
                style={{ flexShrink: 0, fontSize: 12 }}
              >
                ✏️
              </button>
            </div>
          )}

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
