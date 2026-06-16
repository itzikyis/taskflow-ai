import { useState } from 'react';
import { useBoard, useAddColumn, useRemoveColumn, useDeleteBoard } from '../hooks/useBoards';

interface BoardViewProps {
  boardId: string;
  onClose: () => void;
}

export function BoardView({ boardId, onClose }: BoardViewProps) {
  const { data: board, isLoading } = useBoard(boardId);
  const [newColName, setNewColName] = useState('');
  const addCol = useAddColumn(boardId);
  const removeCol = useRemoveColumn(boardId);
  const deleteBoard = useDeleteBoard();

  if (isLoading || !board) return <p>Loading board…</p>;

  const nextOrder = board.columns.length > 0 ? Math.max(...board.columns.map(c => c.order)) + 1 : 0;

  const handleAddCol = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newColName.trim()) return;
    addCol.mutate({ name: newColName.trim(), order: nextOrder }, { onSuccess: () => setNewColName('') });
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
        <h3 style={{ margin: 0 }}>{board.name}</h3>
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          <button
            type="button"
            onClick={() => { deleteBoard.mutate(boardId); onClose(); }}
            style={{ color: 'red', background: 'none', border: 'none', cursor: 'pointer', fontSize: '0.8rem' }}
          >
            Delete board
          </button>
          <button
            type="button"
            onClick={onClose}
            style={{ background: 'none', border: '1px solid #ccc', borderRadius: 4, cursor: 'pointer', padding: '0.2rem 0.5rem', fontSize: '0.8rem' }}
          >
            ← Back
          </button>
        </div>
      </div>

      <div style={{ display: 'flex', gap: '1rem', overflowX: 'auto', paddingBottom: '1rem', minHeight: 200 }}>
        {[...board.columns].sort((a, b) => a.order - b.order).map(col => (
          <div
            key={col.id}
            style={{ minWidth: 220, background: '#f5f5f5', borderRadius: 8, padding: '0.75rem', flexShrink: 0 }}
          >
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
              <strong style={{ fontSize: '0.875rem' }}>{col.name}</strong>
              <button
                type="button"
                onClick={() => removeCol.mutate(col.id)}
                style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#bbb', fontSize: '0.75rem' }}
              >
                ✕
              </button>
            </div>
            {col.wipLimit != null && (
              <p style={{ fontSize: '0.7rem', color: '#aaa', margin: 0 }}>WIP limit: {col.wipLimit}</p>
            )}
          </div>
        ))}

        <form
          onSubmit={handleAddCol}
          style={{ minWidth: 200, flexShrink: 0, display: 'flex', flexDirection: 'column', gap: '0.4rem' }}
        >
          <input
            value={newColName}
            onChange={e => setNewColName(e.target.value)}
            placeholder="Column name…"
            style={{ padding: '0.4rem', borderRadius: 4, border: '1px solid #ddd', fontSize: '0.875rem' }}
          />
          <button
            type="submit"
            disabled={addCol.isPending}
            style={{ padding: '0.35rem', borderRadius: 4, border: '1px dashed #aaa', background: 'none', cursor: 'pointer', fontSize: '0.8rem' }}
          >
            + Add column
          </button>
        </form>
      </div>
    </div>
  );
}
