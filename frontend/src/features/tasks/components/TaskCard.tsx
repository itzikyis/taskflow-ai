import type { Task } from '../types/task.types';

interface TaskCardProps {
  task: Task;
  onDelete: () => void;
}

export function TaskCard({ task, onDelete }: TaskCardProps) {
  return (
    <article
      style={{
        border: '1px solid #ddd',
        borderRadius: 8,
        padding: '1rem',
        marginBottom: '0.75rem',
      }}
    >
      <header style={{ display: 'flex', justifyContent: 'space-between' }}>
        <strong>{task.title}</strong>
        <span style={{ fontSize: '0.8rem', color: '#888' }}>{task.status}</span>
      </header>
      {task.description && <p style={{ margin: '0.5rem 0' }}>{task.description}</p>}
      <footer style={{ display: 'flex', gap: '0.5rem', marginTop: '0.5rem' }}>
        <span style={{ fontSize: '0.75rem' }}>Priority: {task.priority}</span>
        {task.dueDate && (
          <span style={{ fontSize: '0.75rem' }}>
            Due: {new Date(task.dueDate).toLocaleDateString()}
          </span>
        )}
        <button
          type="button"
          onClick={onDelete}
          aria-label={`Delete task ${task.title}`}
          style={{ marginLeft: 'auto', color: 'red', background: 'none', border: 'none', cursor: 'pointer' }}
        >
          Delete
        </button>
      </footer>
    </article>
  );
}
