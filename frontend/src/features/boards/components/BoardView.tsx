import { useState } from 'react';
import { useBoard, useAddColumn, useRemoveColumn, useDeleteBoard } from '../hooks/useBoards';
import { useTasks, useMoveTaskToColumn } from '@/features/tasks/hooks/useTasks';
import type { Task } from '@/features/tasks/types/task.types';
import type { BoardColumn } from '../types/board.types';

interface BoardViewProps {
  boardId: string;
  onClose: () => void;
}

const PRIORITY_DOT: Record<string, string> = {
  Low:      '#10b981',
  Medium:   '#f59e0b',
  High:     '#ef4444',
  Critical: '#7c3aed',
};

const STATUS_COLORS: Record<string, { color: string; bg: string }> = {
  Todo:       { color: 'var(--status-todo)',       bg: 'var(--status-todo-bg)'       },
  InProgress: { color: 'var(--status-inprogress)', bg: 'var(--status-inprogress-bg)' },
  Done:       { color: 'var(--status-done)',        bg: 'var(--status-done-bg)'       },
};

const COL_ACCENTS = ['#6366f1','#3b82f6','#06b6d4','#10b981','#f59e0b','#ef4444','#8b5cf6','#ec4899'];

/* ── Mini task card (board-specific) ───────────────────────────────────── */
function BoardTaskCard({
  task,
  onDragStart,
}: {
  task: Task;
  onDragStart: (e: React.DragEvent, taskId: string) => void;
}) {
  const st = STATUS_COLORS[task.status] ?? STATUS_COLORS.Todo;
  const due = task.dueDate ? new Date(task.dueDate) : null;
  const overdue = due && due < new Date() && task.status !== 'Done';

  return (
    <div
      draggable
      onDragStart={e => onDragStart(e, task.id)}
      className="tf-card"
      style={{
        padding: '10px 12px',
        cursor: 'grab',
        userSelect: 'none',
        marginBottom: 6,
      }}
    >
      <div style={{ display: 'flex', alignItems: 'flex-start', gap: 6 }}>
        <span style={{
          width: 7, height: 7, borderRadius: '50%', flexShrink: 0, marginTop: 5,
          background: PRIORITY_DOT[task.priority] ?? '#94a3b8',
        }} />
        <span style={{ fontSize: 13, fontWeight: 500, color: 'var(--text-primary)', lineHeight: 1.4, flex: 1 }}>
          {task.title}
        </span>
      </div>

      <div style={{ display: 'flex', alignItems: 'center', gap: 6, marginTop: 8, flexWrap: 'wrap' }}>
        <span
          className="tf-badge"
          style={{ color: st.color, background: st.bg, fontSize: 10 }}
        >
          {task.status === 'InProgress' ? 'In Progress' : task.status}
        </span>

        {due && (
          <span style={{ fontSize: 11, color: overdue ? 'var(--color-danger)' : 'var(--text-muted)', marginLeft: 'auto' }}>
            {overdue ? '⚠ ' : ''}{due.toLocaleDateString(undefined, { month: 'short', day: 'numeric' })}
          </span>
        )}
      </div>
    </div>
  );
}

/* ── Column drop zone ───────────────────────────────────────────────────── */
function BoardColumnView({
  col,
  tasks,
  accent,
  onDragStart,
  onDrop,
  onRemove,
}: {
  col: BoardColumn;
  tasks: Task[];
  accent: string;
  onDragStart: (e: React.DragEvent, taskId: string) => void;
  onDrop: (columnId: string) => void;
  onRemove: () => void;
}) {
  const [dragOver, setDragOver] = useState(false);

  return (
    <div
      className="kanban-col"
      style={{
        background: dragOver ? `${accent}12` : 'var(--surface-card)',
        border: dragOver ? `2px solid ${accent}` : '1px solid var(--surface-border)',
        transition: 'background 0.15s, border-color 0.15s',
        minHeight: 200,
      }}
      onDragOver={e => { e.preventDefault(); setDragOver(true); }}
      onDragLeave={() => setDragOver(false)}
      onDrop={e => { e.preventDefault(); setDragOver(false); onDrop(col.id); }}
    >
      <div className="kanban-col-header">
        <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
          <span style={{ width: 9, height: 9, borderRadius: '50%', background: accent, flexShrink: 0 }} />
          <span className="kanban-col-title" style={{ color: accent }}>{col.name}</span>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
          <span className="kanban-count">{tasks.length}</span>
          {col.wipLimit != null && (
            <span className="tf-badge" style={{ color: '#f59e0b', background: '#fef3c7', fontSize: 10 }}>
              WIP {col.wipLimit}
            </span>
          )}
          <button
            type="button"
            onClick={onRemove}
            style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--text-muted)', fontSize: 15, lineHeight: 1, padding: '1px 3px' }}
          >
            ×
          </button>
        </div>
      </div>

      {tasks.map(t => (
        <BoardTaskCard key={t.id} task={t} onDragStart={onDragStart} />
      ))}

      {tasks.length === 0 && !dragOver && (
        <div style={{
          border: '2px dashed var(--surface-border)', borderRadius: 'var(--radius-md)',
          padding: '20px 8px', textAlign: 'center', color: 'var(--text-muted)', fontSize: 12, marginTop: 4,
        }}>
          Drop tasks here
        </div>
      )}

      {dragOver && (
        <div style={{
          border: `2px dashed ${accent}`, borderRadius: 'var(--radius-md)',
          padding: '20px 8px', textAlign: 'center', color: accent, fontSize: 12, fontWeight: 600, marginTop: 4,
        }}>
          Release to drop
        </div>
      )}
    </div>
  );
}

/* ── Unassigned sidebar ─────────────────────────────────────────────────── */
function UnassignedPanel({
  tasks,
  onDragStart,
  onDrop,
}: {
  tasks: Task[];
  onDragStart: (e: React.DragEvent, taskId: string) => void;
  onDrop: () => void;
}) {
  const [dragOver, setDragOver] = useState(false);

  return (
    <div
      style={{
        width: 220, flexShrink: 0,
        background: dragOver ? '#f0f4ff' : 'var(--surface-bg)',
        border: dragOver ? '2px solid var(--color-primary)' : '1px dashed var(--surface-border)',
        borderRadius: 'var(--radius-lg)', padding: '12px',
        transition: 'background 0.15s, border-color 0.15s',
      }}
      onDragOver={e => { e.preventDefault(); setDragOver(true); }}
      onDragLeave={() => setDragOver(false)}
      onDrop={e => { e.preventDefault(); setDragOver(false); onDrop(); }}
    >
      <div style={{ fontSize: 11, fontWeight: 700, textTransform: 'uppercase', letterSpacing: '0.06em', color: 'var(--text-muted)', marginBottom: 8 }}>
        Unassigned ({tasks.length})
      </div>
      {tasks.map(t => (
        <BoardTaskCard key={t.id} task={t} onDragStart={onDragStart} />
      ))}
      {tasks.length === 0 && (
        <div style={{ fontSize: 12, color: 'var(--text-muted)', textAlign: 'center', padding: '12px 0' }}>
          {dragOver ? 'Release to unassign' : 'No unassigned tasks'}
        </div>
      )}
    </div>
  );
}

/* ── Main BoardView ─────────────────────────────────────────────────────── */
export function BoardView({ boardId, onClose }: BoardViewProps) {
  const { data: board, isLoading } = useBoard(boardId);
  const { data: allTasks = [] }    = useTasks();
  const moveToColumn               = useMoveTaskToColumn();

  const [newColName, setNewColName] = useState('');
  const [draggingId, setDraggingId] = useState<string | null>(null);
  const addCol    = useAddColumn(boardId);
  const removeCol = useRemoveColumn(boardId);
  const deleteBoard = useDeleteBoard();

  if (isLoading || !board) return (
    <div className="empty-state"><div className="empty-state-icon">⌛</div></div>
  );

  const sorted = [...board.columns].sort((a, b) => a.order - b.order);
  const nextOrder = sorted.length > 0 ? Math.max(...sorted.map(c => c.order)) + 1 : 0;

  /* Tasks that belong to this board's columns */
  const colIds = new Set(sorted.map(c => c.id));
  const boardTasks   = allTasks.filter(t => t.columnId && colIds.has(t.columnId));
  const unassigned   = allTasks.filter(t => !t.columnId || !colIds.has(t.columnId));

  const tasksByColumn = (colId: string) => boardTasks.filter(t => t.columnId === colId);

  const handleDragStart = (e: React.DragEvent, taskId: string) => {
    setDraggingId(taskId);
    e.dataTransfer.effectAllowed = 'move';
  };

  const handleDrop = (columnId: string | null) => {
    if (!draggingId) return;
    moveToColumn.mutate({ taskId: draggingId, columnId });
    setDraggingId(null);
  };

  const handleAddCol = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newColName.trim()) return;
    addCol.mutate({ name: newColName.trim(), order: nextOrder }, { onSuccess: () => setNewColName('') });
  };

  return (
    <div>
      {/* ── Header ──────────────────────────────────────────────── */}
      <div className="page-header">
        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          <button type="button" className="tf-btn tf-btn-ghost tf-btn-sm" onClick={onClose}>
            ← Boards
          </button>
          <h1 className="page-title">{board.name}</h1>
        </div>
        <button
          type="button"
          className="tf-btn tf-btn-danger tf-btn-sm"
          onClick={() => { deleteBoard.mutate(boardId); onClose(); }}
        >
          Delete board
        </button>
      </div>

      <p style={{ fontSize: 12, color: 'var(--text-muted)', marginBottom: 16 }}>
        Drag any task from the left panel into a column, or between columns.
      </p>

      <div style={{ display: 'flex', gap: 16, overflowX: 'auto', paddingBottom: 16, alignItems: 'flex-start' }}>
        {/* Unassigned tasks panel */}
        <UnassignedPanel
          tasks={unassigned}
          onDragStart={handleDragStart}
          onDrop={() => handleDrop(null)}
        />

        {/* Columns */}
        <div className="kanban-board" style={{ flex: 1, minWidth: 0 }}>
          {sorted.map((col, i) => (
            <BoardColumnView
              key={col.id}
              col={col}
              tasks={tasksByColumn(col.id)}
              accent={COL_ACCENTS[i % COL_ACCENTS.length]}
              onDragStart={handleDragStart}
              onDrop={handleDrop}
              onRemove={() => removeCol.mutate(col.id)}
            />
          ))}

          {/* Add column form */}
          <form
            onSubmit={handleAddCol}
            style={{ flexShrink: 0, width: 220, display: 'flex', flexDirection: 'column', gap: 8 }}
          >
            <input
              value={newColName}
              onChange={e => setNewColName(e.target.value)}
              placeholder="Column name…"
              className="tf-input"
            />
            <button
              type="submit"
              disabled={addCol.isPending}
              className="tf-btn tf-btn-ghost"
              style={{ justifyContent: 'center', borderStyle: 'dashed' }}
            >
              + Add column
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
