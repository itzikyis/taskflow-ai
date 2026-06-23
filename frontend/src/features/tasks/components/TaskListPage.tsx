import { useState } from 'react';
import { useTasks, useDeleteTask } from '../hooks/useTasks';
import { TaskCard } from './TaskCard';
import { CreateTaskForm } from './CreateTaskForm';
import type { Task, TaskStatus } from '../types/task.types';

const COLUMNS: { status: TaskStatus; label: string; color: string }[] = [
  { status: 'Todo',       label: 'To Do',       color: 'var(--status-todo)'        },
  { status: 'InProgress', label: 'In Progress',  color: 'var(--status-inprogress)'  },
  { status: 'Done',       label: 'Done',         color: 'var(--status-done)'        },
];

export function TaskListPage() {
  const { data: tasks, isLoading, isError } = useTasks();
  const deleteMutation = useDeleteTask();
  const [showCreate, setShowCreate] = useState(false);
  const [filter, setFilter] = useState('');

  if (isLoading) return (
    <div className="empty-state">
      <div className="empty-state-icon">⌛</div>
      <p className="empty-state-text">Loading tasks…</p>
    </div>
  );
  if (isError) return (
    <div className="empty-state">
      <div className="empty-state-icon">⚠️</div>
      <p className="empty-state-text">Failed to load tasks. Please refresh.</p>
    </div>
  );

  const filtered = filter
    ? tasks?.filter(t => t.title.toLowerCase().includes(filter.toLowerCase()))
    : tasks;

  const byStatus = (status: TaskStatus): Task[] =>
    (filtered ?? []).filter(t => t.status === status);

  return (
    <div>
      {/* ── Page header ─────────────────────────────────────── */}
      <div className="page-header">
        <div>
          <h1 className="page-title">My Tasks</h1>
          <p className="page-subtitle">{tasks?.length ?? 0} tasks total</p>
        </div>
        <button
          type="button"
          className="tf-btn tf-btn-primary"
          onClick={() => setShowCreate(true)}
        >
          + New task
        </button>
      </div>

      {/* ── Filter bar ──────────────────────────────────────── */}
      <div style={{ marginBottom: 20 }}>
        <input
          type="search"
          className="tf-input"
          value={filter}
          onChange={e => setFilter(e.target.value)}
          placeholder="🔍  Search tasks…"
          style={{ maxWidth: 320 }}
        />
      </div>

      {/* ── Kanban board ────────────────────────────────────── */}
      {(!tasks || tasks.length === 0) ? (
        <div className="empty-state">
          <div className="empty-state-icon">✅</div>
          <p className="empty-state-text">No tasks yet. Create your first one!</p>
          <button
            type="button"
            className="tf-btn tf-btn-primary"
            onClick={() => setShowCreate(true)}
          >
            + New task
          </button>
        </div>
      ) : (
        <div className="kanban-board">
          {COLUMNS.map(col => {
            const items = byStatus(col.status);
            return (
              <div key={col.status} className="kanban-col">
                <div className="kanban-col-header">
                  <span className="kanban-col-title" style={{ color: col.color }}>
                    {col.label}
                  </span>
                  <span className="kanban-count">{items.length}</span>
                </div>

                {items.length === 0 ? (
                  <div style={{ padding: '20px 8px', textAlign: 'center', color: 'var(--text-muted)', fontSize: 12 }}>
                    No tasks
                  </div>
                ) : (
                  items.map(task => (
                    <TaskCard
                      key={task.id}
                      task={task}
                      onDelete={() => deleteMutation.mutate(task.id)}
                    />
                  ))
                )}
              </div>
            );
          })}
        </div>
      )}

      {/* ── Create modal ────────────────────────────────────── */}
      {showCreate && <CreateTaskForm onClose={() => setShowCreate(false)} />}
    </div>
  );
}
