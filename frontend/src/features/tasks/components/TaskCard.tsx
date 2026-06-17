import { useState } from 'react';
import type { Task } from '../types/task.types';
import { CommentThread } from '@/features/comments/components/CommentThread';
import { AttachmentList } from '@/features/attachments/components/AttachmentList';
import { AiDescriptionSuggestion } from '@/features/ai/components/AiDescriptionSuggestion';
import { useAuthStore } from '@/store/authStore';

interface TaskCardProps {
  task: Task;
  onDelete: () => void;
}

type Panel = 'comments' | 'attachments' | 'ai' | null;

const PANEL_BTN: React.CSSProperties = {
  fontSize: '0.72rem',
  padding: '0.2rem 0.5rem',
  borderRadius: 4,
  border: '1px solid #ddd',
  background: 'none',
  cursor: 'pointer',
};

export function TaskCard({ task, onDelete }: TaskCardProps) {
  const [panel, setPanel] = useState<Panel>(null);
  const { token } = useAuthStore();
  const userId = token?.userId ?? '';

  const toggle = (p: Panel) => setPanel(prev => prev === p ? null : p);

  return (
    <article style={{ border: '1px solid #ddd', borderRadius: 8, padding: '1rem', marginBottom: '0.75rem' }}>
      <header style={{ display: 'flex', justifyContent: 'space-between' }}>
        <strong>{task.title}</strong>
        <span style={{ fontSize: '0.8rem', color: '#888' }}>{task.status}</span>
      </header>

      {task.description && <p style={{ margin: '0.5rem 0', fontSize: '0.875rem' }}>{task.description}</p>}

      <footer style={{ display: 'flex', gap: '0.5rem', marginTop: '0.5rem', flexWrap: 'wrap', alignItems: 'center' }}>
        <span style={{ fontSize: '0.75rem', color: '#777' }}>Priority: {task.priority}</span>
        {task.dueDate && (
          <span style={{ fontSize: '0.75rem', color: '#777' }}>
            Due: {new Date(task.dueDate).toLocaleDateString()}
          </span>
        )}

        <div style={{ marginLeft: 'auto', display: 'flex', gap: '0.4rem' }}>
          <button
            type="button"
            style={{ ...PANEL_BTN, color: panel === 'comments' ? '#0066cc' : '#555', borderColor: panel === 'comments' ? '#0066cc' : '#ddd' }}
            onClick={() => toggle('comments')}
          >
            💬 Comments
          </button>
          <button
            type="button"
            style={{ ...PANEL_BTN, color: panel === 'attachments' ? '#0066cc' : '#555', borderColor: panel === 'attachments' ? '#0066cc' : '#ddd' }}
            onClick={() => toggle('attachments')}
          >
            📎 Attachments
          </button>
          <button
            type="button"
            style={{ ...PANEL_BTN, color: panel === 'ai' ? '#7c3aed' : '#555', borderColor: panel === 'ai' ? '#7c3aed' : '#ddd' }}
            onClick={() => toggle('ai')}
          >
            ✨ AI
          </button>
          <button
            type="button"
            onClick={onDelete}
            aria-label={`Delete task ${task.title}`}
            style={{ fontSize: '0.72rem', color: 'red', background: 'none', border: 'none', cursor: 'pointer' }}
          >
            Delete
          </button>
        </div>
      </footer>

      {panel === 'comments' && (
        <div style={{ marginTop: '1rem', paddingTop: '1rem', borderTop: '1px solid #f0f0f0' }}>
          <CommentThread taskId={task.id} currentUserId={userId} />
        </div>
      )}

      {panel === 'attachments' && (
        <div style={{ marginTop: '1rem', paddingTop: '1rem', borderTop: '1px solid #f0f0f0' }}>
          <h4 style={{ margin: '0 0 0.5rem', fontSize: '0.875rem', color: '#555' }}>Attachments</h4>
          <AttachmentList taskId={task.id} currentUserId={userId} />
        </div>
      )}

      {panel === 'ai' && (
        <div style={{ marginTop: '1rem', paddingTop: '1rem', borderTop: '1px solid #f0f0f0' }}>
          <p style={{ margin: '0 0 0.25rem', fontSize: '0.8rem', color: '#7c3aed', fontWeight: 600 }}>✨ AI Assistant</p>
          <AiDescriptionSuggestion
            taskTitle={task.title}
            onAccept={(s) => { alert('Suggestion (copy it manually):\n\n' + s); toggle('ai'); }}
          />
        </div>
      )}
    </article>
  );
}
