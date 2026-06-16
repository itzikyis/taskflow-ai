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
  const [newName, setNewName] = useState('');
  const [activeBoardId, setActiveBoardId] = useState<string | null>(null);

  if (activeBoardId) {
    return <BoardView boardId={activeBoardId} onClose={() => setActiveBoardId(null)} />;
  }

  const handleCreate = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newName.trim()) return;
    createBoard.mutate({ name: newName.trim(), projectId }, { onSuccess: () => setNewName('') });
  };

  if (isLoading) return <p>Loading boards…</p>;

  return (
    <section>
      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1rem' }}>
        <button
          type="button"
          onClick={onBack}
          style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#0066cc', fontSize: '0.875rem' }}
        >
          ← Projects
        </button>
        <h2 style={{ margin: 0 }}>Boards — {projectName}</h2>
      </div>

      <form onSubmit={handleCreate} style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
        <input
          value={newName}
          onChange={e => setNewName(e.target.value)}
          placeholder="New board name…"
          style={{ flex: 1, padding: '0.5rem' }}
        />
        <button type="submit" disabled={createBoard.isPending} style={{ padding: '0.5rem 1rem' }}>
          {createBoard.isPending ? 'Creating…' : 'Add Board'}
        </button>
      </form>

      {boards?.length === 0 && <p>No boards yet. Create one above!</p>}

      <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.75rem' }}>
        {boards?.map(board => (
          <button
            key={board.id}
            type="button"
            onClick={() => setActiveBoardId(board.id)}
            style={{
              padding: '1rem 1.5rem',
              border: '1px solid #ddd',
              borderRadius: 8,
              background: '#fff',
              cursor: 'pointer',
              textAlign: 'left',
              minWidth: 160,
            }}
          >
            <strong style={{ display: 'block' }}>{board.name}</strong>
            <span style={{ fontSize: '0.75rem', color: '#888' }}>{board.columns.length} columns</span>
          </button>
        ))}
      </div>
    </section>
  );
}
