import { useState } from 'react';
import { useTasks, useDeleteTask, useTaskSearch } from '../hooks/useTasks';
import { useAllDependencies } from '../hooks/useDependencies';
import { TaskCard } from './TaskCard';
import { CreateTaskForm } from './CreateTaskForm';
import { TaskTableView, type GroupBy } from './TaskTableView';
import type { Task, TaskStatus } from '../types/task.types';
import type { TaskSearchResult } from '@/services/taskService';

type ViewMode = 'board' | 'list';

const COLUMNS: { status: TaskStatus; label: string; color: string }[] = [
  { status: 'Todo',       label: 'To Do',       color: 'var(--status-todo)'        },
  { status: 'InProgress', label: 'In Progress',  color: 'var(--status-inprogress)'  },
  { status: 'Done',       label: 'Done',         color: 'var(--status-done)'        },
];

export function TaskListPage() {
  const { data: tasks, isLoading, isError } = useTasks();
  const { data: dependencies = [] } = useAllDependencies();
  const deleteMutation = useDeleteTask();
  const search = useTaskSearch();
  const [showCreate, setShowCreate] = useState(false);
  const [filter, setFilter] = useState('');
  const [nlQuery, setNlQuery] = useState('');
  const [nlResult, setNlResult] = useState<TaskSearchResult | null>(null);
  const [viewMode, setViewMode] = useState<ViewMode>(
    () => (localStorage.getItem('taskflow-view-mode') as ViewMode) || 'board');
  const [groupBy, setGroupBy] = useState<GroupBy>(
    () => (localStorage.getItem('taskflow-group-by') as GroupBy) || 'none');

  const changeViewMode = (m: ViewMode) => { setViewMode(m); localStorage.setItem('taskflow-view-mode', m); };
  const changeGroupBy = (g: GroupBy) => { setGroupBy(g); localStorage.setItem('taskflow-group-by', g); };

  const runNlSearch = (e: React.FormEvent) => {
    e.preventDefault();
    const q = nlQuery.trim();
    if (!q) return;
    search.mutate(q, { onSuccess: setNlResult });
  };

  const clearNlSearch = () => {
    setNlResult(null);
    setNlQuery('');
  };

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

  // When an AI search is active its results drive the board; otherwise the
  // normal (optionally quick-filtered) task list is shown.
  const board: Task[] = nlResult ? nlResult.results : (filtered ?? []);

  // A task is blocked when it has a dependency whose blocker task isn't Done.
  const statusById = new Map((tasks ?? []).map(t => [t.id, t.status]));
  const blockedIds = new Set(
    dependencies
      .filter(d => statusById.get(d.blockedByTaskId) !== 'Done')
      .map(d => d.taskId),
  );

  const byStatus = (status: TaskStatus): Task[] =>
    board.filter(t => t.status === status);

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

      {/* ── Filter + AI search bar ──────────────────────────── */}
      <div style={{ marginBottom: 20, display: 'flex', flexDirection: 'column', gap: 10 }}>
        <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
          <input
            type="search"
            className="tf-input"
            value={filter}
            onChange={e => setFilter(e.target.value)}
            placeholder="🔍  Quick filter by title…"
            style={{ maxWidth: 280 }}
          />
          <form onSubmit={runNlSearch} style={{ display: 'flex', gap: 6, flex: 1, minWidth: 260 }}>
            <input
              type="text"
              className="tf-input"
              value={nlQuery}
              onChange={e => setNlQuery(e.target.value)}
              placeholder="✨  Ask… e.g. “overdue high priority tasks assigned to me”"
              style={{ flex: 1 }}
            />
            <button type="submit" className="tf-btn tf-btn-primary tf-btn-sm" disabled={search.isPending || !nlQuery.trim()}>
              {search.isPending ? '…' : 'Ask AI'}
            </button>
          </form>
        </div>

        {nlResult && (
          <div style={{
            display: 'flex', alignItems: 'center', gap: 10, flexWrap: 'wrap',
            padding: '8px 12px', border: '1px solid var(--color-primary)',
            background: 'var(--color-primary-light)', borderRadius: 8,
          }}>
            <span style={{ fontSize: 12, color: 'var(--color-primary)', fontWeight: 600 }}>
              ✨ {nlResult.interpretation}
            </span>
            <span style={{ fontSize: 12, color: 'var(--text-secondary)' }}>
              — {nlResult.results.length} result{nlResult.results.length === 1 ? '' : 's'}
            </span>
            <button type="button" className="tf-btn tf-btn-ghost tf-btn-sm" onClick={clearNlSearch} style={{ marginLeft: 'auto', fontSize: 11 }}>
              Clear
            </button>
          </div>
        )}
      </div>

      {/* ── View toggle ─────────────────────────────────────── */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 14 }}>
        <div style={{ display: 'flex', gap: 4 }}>
          <button
            type="button"
            className={`tf-btn tf-btn-sm ${viewMode === 'board' ? 'tf-btn-primary' : 'tf-btn-ghost'}`}
            onClick={() => changeViewMode('board')}
          >
            ▦ Board
          </button>
          <button
            type="button"
            className={`tf-btn tf-btn-sm ${viewMode === 'list' ? 'tf-btn-primary' : 'tf-btn-ghost'}`}
            onClick={() => changeViewMode('list')}
          >
            ☰ List
          </button>
        </div>
        {viewMode === 'list' && (
          <label style={{ fontSize: 12, color: 'var(--text-muted)', display: 'flex', alignItems: 'center', gap: 6 }}>
            Group by
            <select className="tf-input" value={groupBy} onChange={e => changeGroupBy(e.target.value as GroupBy)} style={{ fontSize: 12, padding: '2px 6px' }}>
              <option value="none">None</option>
              <option value="status">Status</option>
              <option value="priority">Priority</option>
              <option value="assignee">Assignee</option>
            </select>
          </label>
        )}
      </div>

      {/* ── Board / List ─────────────────────────────────────── */}
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
      ) : viewMode === 'list' ? (
        <TaskTableView tasks={board} groupBy={groupBy} onDelete={id => deleteMutation.mutate(id)} />
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
                      isBlocked={blockedIds.has(task.id)}
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
