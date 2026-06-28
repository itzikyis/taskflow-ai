import { useState } from 'react';
import { useCreateTeam } from '../hooks/useTeams';

interface CreateTeamFormProps {
  onClose: () => void;
}

export function CreateTeamForm({ onClose }: CreateTeamFormProps) {
  const [name, setName]               = useState('');
  const [description, setDescription] = useState('');
  const createTeam                    = useCreateTeam();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;
    createTeam.mutate(
      { name: name.trim(), description: description.trim() || undefined },
      { onSuccess: onClose },
    );
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-box" onClick={e => e.stopPropagation()}>
        <h2 style={{ margin: '0 0 20px', fontSize: 18, fontWeight: 700, color: 'var(--text-primary)' }}>
          New Team
        </h2>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
            <label style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-secondary)' }}>
              Name <span style={{ color: 'var(--color-danger)' }}>*</span>
            </label>
            <input
              className="tf-input"
              type="text"
              value={name}
              onChange={e => setName(e.target.value)}
              placeholder="e.g. Frontend Guild"
              required
              autoFocus
            />
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
            <label style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-secondary)' }}>
              Description
            </label>
            <textarea
              className="tf-input"
              value={description}
              onChange={e => setDescription(e.target.value)}
              placeholder="What does this team work on?"
              rows={3}
              style={{ resize: 'vertical' }}
            />
          </div>

          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 8, marginTop: 4 }}>
            <button type="button" onClick={onClose} className="tf-btn tf-btn-ghost">
              Cancel
            </button>
            <button
              type="submit"
              className="tf-btn tf-btn-primary"
              disabled={createTeam.isPending || !name.trim()}
            >
              {createTeam.isPending ? 'Creating…' : 'Create'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
