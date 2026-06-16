import { useState } from 'react';
import { useCreateProject } from '../hooks/useProjects';

// Temporary stub — replace with real auth context
const STUB_OWNER_ID = '00000000-0000-0000-0000-000000000001';

export function CreateProjectForm() {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const createMutation = useCreateProject();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;
    createMutation.mutate(
      { name: name.trim(), description: description.trim() || undefined, ownerId: STUB_OWNER_ID },
      {
        onSuccess: () => {
          setName('');
          setDescription('');
        },
      },
    );
  };

  return (
    <form onSubmit={handleSubmit} style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem', flexWrap: 'wrap' }}>
      <input
        type="text"
        value={name}
        onChange={(e) => setName(e.target.value)}
        placeholder="New project name…"
        aria-label="Project name"
        required
        style={{ flex: '1 1 200px', padding: '0.5rem' }}
      />
      <input
        type="text"
        value={description}
        onChange={(e) => setDescription(e.target.value)}
        placeholder="Description (optional)"
        aria-label="Project description"
        style={{ flex: '2 1 250px', padding: '0.5rem' }}
      />
      <button type="submit" disabled={createMutation.isPending} style={{ padding: '0.5rem 1rem' }}>
        {createMutation.isPending ? 'Creating…' : 'Add Project'}
      </button>
    </form>
  );
}
