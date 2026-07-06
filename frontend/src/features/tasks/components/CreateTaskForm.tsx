import { useState } from 'react';
import { useCreateTask } from '../hooks/useTasks';
import type { TaskPriority } from '../types/task.types';
import { TASK_PRIORITIES } from '../types/task.types';
import { useAuthStore } from '@/store/authStore';

interface CreateTaskFormProps {
  onClose: () => void;
}

export function CreateTaskForm({ onClose }: CreateTaskFormProps) {
  const [title, setTitle]       = useState('');
  const [description, setDesc]  = useState('');
  const [priority, setPriority] = useState<TaskPriority>('Medium');
  const [dueDate, setDueDate]   = useState('');
  const createMutation          = useCreateTask();
  const { token }               = useAuthStore();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim() || !token) return;
    createMutation.mutate(
      {
        title: title.trim(),
        description: description.trim() || undefined,
        priority,
        dueDate: dueDate || undefined,
      },
      { onSuccess: onClose },
    );
  };

  return (
    <div className="modal-overlay" onClick={e => { if (e.target === e.currentTarget) onClose(); }}>
      <div className="modal-box">
        <h2 className="modal-title">New task</h2>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          <div className="form-group">
            <label htmlFor="task-title" className="form-label">Title *</label>
            <input
              id="task-title"
              type="text"
              className="tf-input"
              value={title}
              onChange={e => setTitle(e.target.value)}
              placeholder="What needs to be done?"
              required
              autoFocus
            />
          </div>

          <div className="form-group">
            <label htmlFor="task-desc" className="form-label">Description</label>
            <textarea
              id="task-desc"
              className="tf-input"
              value={description}
              onChange={e => setDesc(e.target.value)}
              placeholder="Add details, acceptance criteria…"
              rows={3}
              style={{ resize: 'vertical' }}
            />
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <div className="form-group">
              <label htmlFor="task-priority" className="form-label">Priority</label>
              <select
                id="task-priority"
                className="tf-input"
                value={priority}
                onChange={e => setPriority(e.target.value as TaskPriority)}
              >
                {TASK_PRIORITIES.map(p => (
                  <option key={p} value={p}>{p}</option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="task-due" className="form-label">Due date</label>
              <input
                id="task-due"
                type="date"
                className="tf-input"
                value={dueDate}
                onChange={e => setDueDate(e.target.value)}
              />
            </div>
          </div>

          <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end', marginTop: 4 }}>
            <button type="button" className="tf-btn tf-btn-ghost" onClick={onClose}>
              Cancel
            </button>
            <button
              type="submit"
              disabled={createMutation.isPending || !title.trim()}
              className="tf-btn tf-btn-primary"
            >
              {createMutation.isPending ? 'Creating…' : 'Create task'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
