import { useState } from 'react';
import type { Project } from '../types/project.types';
import { useUpdateProject } from '../hooks/useProjects';

interface ProjectCardProps {
  project: Project;
  onDelete: () => void;
  onOpenBoards: () => void;
}

export function ProjectCard({ project, onDelete, onOpenBoards }: ProjectCardProps) {
  const [editing, setEditing]         = useState(false);
  const [name, setName]               = useState(project.name);
  const [description, setDescription] = useState(project.description ?? '');
  const updateMutation                = useUpdateProject(project.id);

  const handleSave = (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;
    updateMutation.mutate(
      { name: name.trim(), description: description.trim() || undefined },
      { onSuccess: () => setEditing(false) },
    );
  };

  const initials = project.name.slice(0, 2).toUpperCase();
  const hue = (project.name.charCodeAt(0) * 37 + project.name.charCodeAt(1 % project.name.length) * 13) % 360;

  if (editing) {
    return (
      <div className="tf-card" style={{ padding: 16 }}>
        <form onSubmit={handleSave} style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          <input
            className="tf-input"
            value={name}
            onChange={e => setName(e.target.value)}
            placeholder="Project name"
            required
            style={{ fontWeight: 600 }}
          />
          <input
            className="tf-input"
            value={description}
            onChange={e => setDescription(e.target.value)}
            placeholder="Description (optional)"
          />
          <div style={{ display: 'flex', gap: 8 }}>
            <button type="submit" disabled={updateMutation.isPending} className="tf-btn tf-btn-primary tf-btn-sm">
              {updateMutation.isPending ? 'Saving…' : 'Save'}
            </button>
            <button type="button" className="tf-btn tf-btn-ghost tf-btn-sm" onClick={() => setEditing(false)}>
              Cancel
            </button>
          </div>
        </form>
      </div>
    );
  }

  return (
    <div className="tf-card" style={{ padding: 0, overflow: 'hidden' }}>
      {/* Color banner */}
      <div style={{
        height: 6,
        background: `hsl(${hue}, 60%, 55%)`,
      }} />

      <div style={{ padding: '14px 16px' }}>
        <div style={{ display: 'flex', alignItems: 'flex-start', gap: 12 }}>
          {/* Project avatar */}
          <div style={{
            width: 36, height: 36, borderRadius: 8,
            background: `hsl(${hue}, 60%, 55%)`,
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            fontSize: 13, fontWeight: 700, color: '#fff', flexShrink: 0,
          }}>
            {initials}
          </div>

          <div style={{ flex: 1, overflow: 'hidden' }}>
            <div style={{ fontWeight: 600, fontSize: 14, color: 'var(--text-primary)' }}>
              {project.name}
            </div>
            {project.description && (
              <div style={{
                fontSize: 12, color: 'var(--text-secondary)', marginTop: 2,
                overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap',
              }}>
                {project.description}
              </div>
            )}
            <div style={{ fontSize: 11, color: 'var(--text-muted)', marginTop: 4 }}>
              Created {new Date(project.createdAt).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })}
            </div>
          </div>
        </div>

        <div style={{ display: 'flex', gap: 6, marginTop: 14, paddingTop: 12, borderTop: '1px solid var(--surface-border-light)' }}>
          <button
            type="button"
            className="tf-btn tf-btn-primary tf-btn-sm"
            onClick={onOpenBoards}
            style={{ flex: 1, justifyContent: 'center' }}
          >
            ⬡ Boards
          </button>
          <button
            type="button"
            className="tf-btn tf-btn-ghost tf-btn-sm"
            onClick={() => setEditing(true)}
            aria-label={`Edit ${project.name}`}
          >
            ✎
          </button>
          <button
            type="button"
            className="tf-btn tf-btn-danger tf-btn-sm"
            onClick={onDelete}
            aria-label={`Delete ${project.name}`}
          >
            ✕
          </button>
        </div>
      </div>
    </div>
  );
}
