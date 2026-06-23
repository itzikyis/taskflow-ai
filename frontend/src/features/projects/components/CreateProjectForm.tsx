import { useState } from 'react';
import { useCreateProject } from '../hooks/useProjects';
import { useAuthStore } from '@/store/authStore';

interface CreateProjectFormProps {
  onClose: () => void;
}

export function CreateProjectForm({ onClose }: CreateProjectFormProps) {
  const [name, setName]               = useState('');
  const [description, setDescription] = useState('');
  const createMutation                = useCreateProject();
  const { token }                     = useAuthStore();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim() || !token) return;
    createMutation.mutate(
      { name: name.trim(), description: description.trim() || undefined, ownerId: token.userId },
      { onSuccess: onClose },
    );
  };

  return (
    <div className="modal-overlay" onClick={e => { if (e.target === e.currentTarget) onClose(); }}>
      <div className="modal-box">
        <h2 className="modal-title">New project</h2>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          <div className="form-group">
            <label htmlFor="proj-name" className="form-label">Project name *</label>
            <input
              id="proj-name"
              type="text"
              className="tf-input"
              value={name}
              onChange={e => setName(e.target.value)}
              placeholder="My awesome project"
              required
              autoFocus
            />
          </div>

          <div className="form-group">
            <label htmlFor="proj-desc" className="form-label">Description</label>
            <textarea
              id="proj-desc"
              className="tf-input"
              value={description}
              onChange={e => setDescription(e.target.value)}
              placeholder="What is this project about?"
              rows={3}
              style={{ resize: 'vertical' }}
            />
          </div>

          <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end', marginTop: 4 }}>
            <button type="button" className="tf-btn tf-btn-ghost" onClick={onClose}>
              Cancel
            </button>
            <button
              type="submit"
              disabled={createMutation.isPending || !name.trim()}
              className="tf-btn tf-btn-primary"
            >
              {createMutation.isPending ? 'Creating…' : 'Create project'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
