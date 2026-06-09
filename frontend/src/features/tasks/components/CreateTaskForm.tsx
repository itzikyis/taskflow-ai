import { useState } from 'react';
import { useCreateTask } from '../hooks/useTasks';
import type { TaskPriority } from '../types/task.types';
import { TASK_PRIORITIES } from '../types/task.types';

// Temporary stub — replace with real auth context
const STUB_USER_ID = '00000000-0000-0000-0000-000000000001';

export function CreateTaskForm() {
  const [title, setTitle] = useState('');
  const [priority, setPriority] = useState<TaskPriority>('Medium');
  const createMutation = useCreateTask();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) return;
    createMutation.mutate(
      { title: title.trim(), priority, createdByUserId: STUB_USER_ID },
      { onSuccess: () => setTitle('') },
    );
  };

  return (
    <form onSubmit={handleSubmit} style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
      <input
        type="text"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        placeholder="New task title…"
        aria-label="Task title"
        required
        style={{ flex: 1, padding: '0.5rem' }}
      />
      <select
        value={priority}
        onChange={(e) => setPriority(e.target.value as TaskPriority)}
        aria-label="Priority"
        style={{ padding: '0.5rem' }}
      >
        {TASK_PRIORITIES.map((p) => (
          <option key={p} value={p}>
            {p}
          </option>
        ))}
      </select>
      <button type="submit" disabled={createMutation.isPending} style={{ padding: '0.5rem 1rem' }}>
        {createMutation.isPending ? 'Adding…' : 'Add Task'}
      </button>
    </form>
  );
}
