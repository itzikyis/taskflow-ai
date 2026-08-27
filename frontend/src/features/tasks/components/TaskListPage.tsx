import { useState, useCallback, useRef } from 'react';
import { useTasks, useDeleteTask, useTaskSearch } from '../hooks/useTasks';
import { useAllDependencies } from '../hooks/useDependencies';
import { useSavedViews } from '../hooks/useSavedViews';
import { CreateTaskForm } from './CreateTaskForm';
import { TaskTableView, type GroupBy } from './TaskTableView';
import { BulkActionToolbar } from './BulkActionToolbar';
import type { Task } from '../types/task.types';
import type { TaskFilter } from '../types/savedView.types';
import type { TaskSearchResult } from '@/services/taskService';

type ViewMode = 'list';


const PANEL_STYLE: React.CSSProperties = {
  position: 'absolute',
  top: '100%',
  right: 0,
  zIndex: 100,
  marginTop: 4,
  minWidth: 240,
  background: 'var(--surface-card)',
  border: '1px solid var(--surface-border)',
  borderRadius: 'var(--radius-md)',
  boxShadow: '0 4px 16px rgba(0,0,0,0.15)',
  padding: '8px 0',
};

export function TaskListPage() {
  const { data: tasks, isLoading, isError } = useTasks();
  const { data: dependencies = [] } = useAllDependencies();
  const deleteMutation = useDeleteTask();
  const search = useTaskSearch();
  const { savedViews, saveView, deleteView, pinView } = useSavedViews();

  const [showCreate, setShowCreate] = useState(false);
  const [filter, setFilter] = useState('');
  const [nlQuery, setNlQuery] = useState('');
  const [nlResult, setNlResult] = useState<TaskSearchResult | null>(null);
  const [viewMode] = useState<ViewMode>('list');
  const [groupBy, setGroupBy] = useState<GroupBy>(
    () => (localStorage.getItem('taskflow-group-by') as GroupBy) || 'none');
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  // Saved-views UI state
  const [saveViewName, setSaveViewName] = useState('');
  const [showSaveInput, setShowSaveInput] = useState(false);
  const [showViewsPanel, setShowViewsPanel] = useState(false);
  const viewsPanelRef = useRef<HTMLDivElement>(null);

  const handleToggleSelect = useCallback((id: string) => {
    setSelectedIds(prev => {
      const next = new Set(prev);
      if (next.has(id)) { next.delete(id); } else { next.add(id); }
      return next;
    });
  }, []);

  const handleToggleAll = useCallback((ids: string[]) => {
    setSelectedIds(prev => {
      const allSelected = ids.every(id => prev.has(id));
      return allSelected ? new Set() : new Set(ids);
    });
  }, []);

  const handleClearSelection = useCallback(() => setSelectedIds(new Set()), []);

  const changeGroupBy = (g: GroupBy) => {
    setGroupBy(g);
    localStorage.setItem('taskflow-group-by', g);
    setSelectedIds(new Set());
  };

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

  const currentFilter: TaskFilter = { search: filter };

  const handleSaveView = (e: React.FormEvent) => {
    e.preventDefault();
    const name = saveViewName.trim();
    if (!name) return;
    saveView(name, currentFilter);
    setSaveViewName('');
    setShowSaveInput(false);
  };

  const applyView = (f: TaskFilter) => {
    setFilter(f.search ?? '');
    setShowViewsPanel(false);
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

  const pinnedViews = savedViews.filter(v => v.isPinned);

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

      {/* ── Pinned view chips ────────────────────────────────── */}
      {pinnedViews.length > 0 && (
        <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap', marginBottom: 12 }}>
          {pinnedViews.map(v => (
            <button
              key={v.id}
              type="button"
              onClick={() => applyView(v.filter)}
              style={{
                display: 'inline-flex',
                alignItems: 'center',
                gap: 4,
                padding: '3px 10px',
                fontSize: 12,
                fontWeight: 500,
                borderRadius: 999,
                border: '1px solid var(--color-primary)',
                background: 'var(--color-primary-light, rgba(99,102,241,0.1))',
                color: 'var(--color-primary)',
                cursor: 'pointer',
                fontFamily: 'inherit',
              }}
            >
              📌 {v.name}
            </button>
          ))}
        </div>
      )}

      {/* ── Filter + AI search bar ──────────────────────────── */}
      <div style={{ marginBottom: 20, display: 'flex', flexDirection: 'column', gap: 10 }}>
        <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap', alignItems: 'center' }}>
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
              placeholder={'✨  Ask… e.g. "overdue high priority tasks assigned to me"'}
              style={{ flex: 1 }}
            />
            <button type="submit" className="tf-btn tf-btn-primary tf-btn-sm" disabled={search.isPending || !nlQuery.trim()}>
              {search.isPending ? '…' : 'Ask AI'}
            </button>
          </form>

          {/* ── Save current view ─── */}
          {showSaveInput ? (
            <form onSubmit={handleSaveView} style={{ display: 'flex', gap: 4, alignItems: 'center' }}>
              <input
                type="text"
                className="tf-input"
                value={saveViewName}
                onChange={e => setSaveViewName(e.target.value)}
                placeholder="View name…"
                autoFocus
                style={{ width: 140, fontSize: 13 }}
              />
              <button type="submit" className="tf-btn tf-btn-primary tf-btn-sm" disabled={!saveViewName.trim()}>
                Save
              </button>
              <button
                type="button"
                className="tf-btn tf-btn-ghost tf-btn-sm"
                onClick={() => { setShowSaveInput(false); setSaveViewName(''); }}
              >
                Cancel
              </button>
            </form>
          ) : (
            <button
              type="button"
              className="tf-btn tf-btn-ghost tf-btn-sm"
              onClick={() => setShowSaveInput(true)}
              title="Save current filters as a named view"
            >
              + Save view
            </button>
          )}

          {/* ── Saved views dropdown ─── */}
          <div ref={viewsPanelRef} style={{ position: 'relative' }}>
            <button
              type="button"
              className="tf-btn tf-btn-ghost tf-btn-sm"
              onClick={() => setShowViewsPanel(p => !p)}
              aria-expanded={showViewsPanel}
              aria-haspopup="true"
            >
              Views {savedViews.length > 0 ? `(${savedViews.length})` : ''} ▾
            </button>

            {showViewsPanel && (
              <div style={PANEL_STYLE} role="menu">
                {savedViews.length === 0 ? (
                  <p style={{ padding: '8px 16px', fontSize: 12, color: 'var(--text-muted, #888)', margin: 0 }}>
                    No saved views yet.
                  </p>
                ) : (
                  savedViews.map(v => (
                    <div
                      key={v.id}
                      style={{
                        display: 'flex',
                        alignItems: 'center',
                        gap: 4,
                        padding: '6px 12px',
                        fontSize: 13,
                        color: 'var(--text-primary)',
                      }}
                    >
                      <button
                        type="button"
                        onClick={() => applyView(v.filter)}
                        style={{
                          flex: 1,
                          textAlign: 'left',
                          background: 'none',
                          border: 'none',
                          padding: 0,
                          cursor: 'pointer',
                          color: 'inherit',
                          fontSize: 'inherit',
                          fontFamily: 'inherit',
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                          whiteSpace: 'nowrap',
                        }}
                        role="menuitem"
                      >
                        {v.name}
                      </button>
                      <button
                        type="button"
                        onClick={() => pinView(v.id)}
                        title={v.isPinned ? 'Unpin' : 'Pin'}
                        style={{
                          background: 'none',
                          border: 'none',
                          cursor: 'pointer',
                          padding: '0 2px',
                          fontSize: 13,
                          opacity: v.isPinned ? 1 : 0.4,
                        }}
                        aria-label={v.isPinned ? `Unpin ${v.name}` : `Pin ${v.name}`}
                      >
                        📌
                      </button>
                      <button
                        type="button"
                        onClick={() => deleteView(v.id)}
                        title="Delete view"
                        style={{
                          background: 'none',
                          border: 'none',
                          cursor: 'pointer',
                          padding: '0 2px',
                          fontSize: 13,
                          color: '#f87171',
                        }}
                        aria-label={`Delete ${v.name}`}
                      >
                        ✕
                      </button>
                    </div>
                  ))
                )}
              </div>
            )}
          </div>
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

      {/* ── View controls ───────────────────────────────────── */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 14 }}>
        <label style={{ fontSize: 12, color: 'var(--text-muted)', display: 'flex', alignItems: 'center', gap: 6 }}>
          Group by
          <select className="tf-input" value={groupBy} onChange={e => changeGroupBy(e.target.value as GroupBy)} style={{ fontSize: 12, padding: '2px 6px' }}>
            <option value="none">None</option>
            <option value="status">Status</option>
            <option value="priority">Priority</option>
            <option value="assignee">Assignee</option>
          </select>
        </label>
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
      ) : (
        <TaskTableView
          tasks={board}
          groupBy={groupBy}
          onDelete={id => deleteMutation.mutate(id)}
          selection={{
            selectedIds,
            onToggleSelect: handleToggleSelect,
            onToggleAll: handleToggleAll,
          }}
        />
      )}

      {/* ── Bulk action toolbar ─────────────────────────────── */}
      <BulkActionToolbar selectedIds={selectedIds} onClearSelection={handleClearSelection} />

      {/* ── Create modal ────────────────────────────────────── */}
      {showCreate && <CreateTaskForm onClose={() => setShowCreate(false)} />}
    </div>
  );
}
