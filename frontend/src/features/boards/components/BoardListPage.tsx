import { useState } from 'react';
import { useBoardsByProject, useCreateBoard } from '../hooks/useBoards';
import { BoardView } from './BoardView';

interface BoardListPageProps {
  projectId: string;
  projectName: string;
  onBack: () => void;
}

export function BoardListPage({ projectId, projectName, onBack }: BoardListPageProps) {
  const { data: boards, isLoading } = useBoardsByProject(projectId);
  const createBoard = useCreateBoard();
  const [newName, setNewName]         = useState('');
  const [activeBoardId, setActiveId]  = useState<string | null>(null);
  const [showForm, setShowForm]       = useState(false);

  if (activeBoardId) {
    return <BoardView boardId={activeBoardId} onClose={() => setActiveId(null)} />;
  }

  const handleCreate = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newName.trim()) return;
    createBoard.mutate(
      { name: newName.trim(), projectId },
      { onSuccess: () => { setNewName(''); setShowForm(false); } },
    );
  };

  if (isLoading) return (
    <div className="empty-state">
      <div className="empty-state-icon">⌛</div>
      <p className="empty-state-text">Loading boards…</p>
    </div>
  );

  return (
    <div>
      {/* ── Header ──────────────────────────────────────────── */}
      <div className="page-header">
        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          <button
            type="button"
            className="tf-btn tf-btn-ghost tf-btn-sm"
            onClick={onBack}
            style={{ padding: '6px 10px' }}
          >
            ← Back
          </button>
          <div>
            <h1 className="page-title">Boards</h1>
            <p className="page-subtitle">{projectName}</p>
          </div>
        </div>
        <button
          type="button"
          className="tf-btn tf-btn-primary"
          onClick={() => setShowForm(p => !p)}
        >
          + New board
        </button>
      </div>

      {/* ── Create inline ────────────────────────────────────── */}
      {showForm && (
        <form
          onSubmit={handleCreate}
          style={{
            display: 'flex', gap: 8, marginBottom: 20,
            background: 'var(--surface-card)',
            border: '1px solid var(--surface-border)',
            borderRadius: 'var(--radius-md)',
            padding: '12px 14px',
          }}
        >
          <input
            value={newName}
            onChange={e => setNewName(e.target.value)}
            placeholder="Board name…"
            className="tf-input"
            style={{ flex: 1 }}
            autoFocus
          />
          <button type="submit" disabled={createBoard.isPending} className="tf-btn tf-btn-primary">
            {createBoard.isPending ? 'Creating…' : 'Create'}
          </button>
          <button type="button" className="tf-btn tf-btn-ghost" onClick={() => setShowForm(false)}>
            Cancel
          </button>
        </form>
      )}

      {/* ── Board grid ───────────────────────────────────────── */}
      {(!boards || boards.length === 0) ? (
        <div className="empty-state">
          <div className="empty-state-icon">⬡</div>
          <p className="empty-state-text">No boards yet. Create your first board!</p>
        </div>
      ) : (
        <div className="board-grid">
          {boards.map(board => (
            <button
              key={board.id}
              type="button"
              onClick={() => setActiveId(board.id)}
              className="tf-card"
              style={{
                cursor: 'pointer', padding: '16px', textAlign: 'left',
                border: 'none', width: '100%',
                background: 'var(--surface-card)',
                display: 'flex', flexDirection: 'column', gap: 6,
              }}
            >
              <div style={{ fontSize: 24 }}>⬡</div>
              <div style={{ fontWeight: 600, fontSize: 14, color: 'var(--text-primary)' }}>
                {board.name}
              </div>
              <div style={{ fontSize: 11, color: 'var(--text-muted)' }}>
                {board.columns.length} column{board.columns.length !== 1 ? 's' : ''}
              </div>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
