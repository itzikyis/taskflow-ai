import { useState } from 'react';
import type { Project } from '../types/project.types';
import { useUpdateProject } from '../hooks/useProjects';

interface ProjectCardProps {
  project: Project;
  onDelete: () => void;
  onOpenBoards: () => void;
}

export function ProjectCard({ project, onDelete, onOpenBoards }: ProjectCardProps) {
  const [editing, setEditing] = useState(false);
  const [name, setName] = useState(project.name);
  const [description, setDescription] = useState(project.description ?? '');
  const updateMutation = useUpdateProject(project.id);

  const handleSave = (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;
    updateMutation.mutate(
      { name: name.trim(), description: description.trim() || undefined },
      { onSuccess: () => setEditing(false) },
    );
  };

  const cardStyle: React.CSSProperties = {
    border: '1px solid #ddd',
    borderRadius: 8,
    padding: '1rem',
    marginBottom: '0.75rem',
  };

  if (editing) {
    return (
      <article style={cardStyle}>
        <form onSubmit={handleSave} style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
          <input
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Project name"
            aria-label="Project name"
            required
            style={{ padding: '0.4rem', fontSize: '1rem', fontWeight: 600 }}
          />
          <input
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="Description (optional)"
            aria-label="Project description"
            style={{ padding: '0.4rem' }}
          />
          <div style={{ display: 'flex', gap: '0.5rem' }}>
            <button type="submit" disabled={updateMutation.isPending} style={{ padding: '0.4rem 0.8rem' }}>
              {updateMutation.isPending ? 'Saving…' : 'Save'}
            </button>
            <button type="button" onClick={() => setEditing(false)} style={{ padding: '0.4rem 0.8rem' }}>
              Cancel
            </button>
          </div>
        </form>
      </article>
    );
  }

  return (
    <article style={cardStyle}>
      <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <strong style={{ fontSize: '1rem' }}>{project.name}</strong>
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          <button
            type="button"
            onClick={onOpenBoards}
            style={{ background: '#0066cc', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', padding: '0.2rem 0.6rem', fontSize: '0.75rem' }}
          >
            Boards
          </button>
          <button
            type="button"
            onClick={() => setEditing(true)}
            aria-label={`Edit project ${project.name}`}
            style={{ background: 'none', border: '1px solid #aaa', borderRadius: 4, cursor: 'pointer', padding: '0.2rem 0.5rem', fontSize: '0.75rem' }}
          >
            Edit
          </button>
          <button
            type="button"
            onClick={onDelete}
            aria-label={`Delete project ${project.name}`}
            style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'red', fontSize: '0.75rem' }}
          >
            Delete
          </button>
        </div>
      </header>
      {project.description && (
        <p style={{ margin: '0.4rem 0 0', color: '#555', fontSize: '0.875rem' }}>{project.description}</p>
      )}
      <footer style={{ marginTop: '0.5rem', fontSize: '0.7rem', color: '#999' }}>
        Created {new Date(project.createdAt).toLocaleDateString()}
        {project.updatedAt && ` · Updated ${new Date(project.updatedAt).toLocaleDateString()}`}
      </footer>
    </article>
  );
}
