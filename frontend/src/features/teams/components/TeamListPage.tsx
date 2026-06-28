import { useState } from 'react';
import { useTeams } from '../hooks/useTeams';
import { TeamCard } from './TeamCard';
import { CreateTeamForm } from './CreateTeamForm';

export function TeamListPage() {
  const { data: teams, isLoading } = useTeams();
  const [showCreate, setShowCreate] = useState(false);

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Teams</h1>
        <button
          type="button"
          className="tf-btn tf-btn-primary"
          onClick={() => setShowCreate(true)}
        >
          + New team
        </button>
      </div>

      {isLoading && (
        <p style={{ color: 'var(--text-muted)', fontSize: 14 }}>Loading teams…</p>
      )}

      {!isLoading && (!teams || teams.length === 0) && (
        <div className="empty-state">
          <p>No teams yet. Create one to get started.</p>
        </div>
      )}

      {teams && teams.length > 0 && (
        <div className="project-grid">
          {teams.map(team => (
            <TeamCard key={team.id} team={team} />
          ))}
        </div>
      )}

      {showCreate && <CreateTeamForm onClose={() => setShowCreate(false)} />}
    </div>
  );
}
