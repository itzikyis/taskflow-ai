import { useState } from 'react';
import {
  useCommentsByTask,
  useAddComment,
  useEditComment,
  useDeleteComment,
} from '../hooks/useComments';

interface CommentThreadProps {
  taskId: string;
  currentUserId: string;
}

/** Displays a threaded list of comments for a task with add/edit/delete support. */
export function CommentThread({ taskId, currentUserId }: CommentThreadProps) {
  const { data: comments, isLoading } = useCommentsByTask(taskId);
  const addComment = useAddComment(taskId);
  const editComment = useEditComment(taskId);
  const deleteComment = useDeleteComment(taskId);
  const [newText, setNewText] = useState('');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editText, setEditText] = useState('');

  const handleAdd = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newText.trim()) return;
    addComment.mutate(
      { authorId: currentUserId, content: newText.trim() },
      { onSuccess: () => setNewText('') },
    );
  };

  const startEdit = (id: string, content: string) => {
    setEditingId(id);
    setEditText(content);
  };

  const cancelEdit = () => {
    setEditingId(null);
    setEditText('');
  };

  const submitEdit = (id: string) => {
    editComment.mutate(
      { id, payload: { requesterId: currentUserId, content: editText.trim() } },
      { onSuccess: () => cancelEdit() },
    );
  };

  if (isLoading) return <p style={{ fontSize: '0.875rem', color: '#888' }}>Loading comments…</p>;

  return (
    <div style={{ marginTop: '1rem' }}>
      <h4 style={{ margin: '0 0 0.5rem', fontSize: '0.875rem', color: '#555' }}>
        Comments ({comments?.length ?? 0})
      </h4>

      {comments?.map(c => (
        <div
          key={c.id}
          style={{ borderLeft: '2px solid #eee', paddingLeft: '0.75rem', marginBottom: '0.75rem' }}
        >
          {editingId === c.id ? (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem' }}>
              <textarea
                value={editText}
                onChange={e => setEditText(e.target.value)}
                rows={3}
                style={{
                  padding: '0.4rem',
                  fontSize: '0.875rem',
                  resize: 'vertical',
                  borderRadius: 4,
                  border: '1px solid #ddd',
                }}
              />
              <div style={{ display: 'flex', gap: '0.4rem' }}>
                <button
                  type="button"
                  onClick={() => submitEdit(c.id)}
                  disabled={editComment.isPending}
                  style={{
                    padding: '0.25rem 0.6rem',
                    fontSize: '0.8rem',
                    borderRadius: 4,
                    border: 'none',
                    background: '#0066cc',
                    color: '#fff',
                    cursor: 'pointer',
                  }}
                >
                  Save
                </button>
                <button
                  type="button"
                  onClick={cancelEdit}
                  style={{
                    padding: '0.25rem 0.6rem',
                    fontSize: '0.8rem',
                    borderRadius: 4,
                    border: '1px solid #ccc',
                    background: 'none',
                    cursor: 'pointer',
                  }}
                >
                  Cancel
                </button>
              </div>
            </div>
          ) : (
            <>
              <p style={{ margin: '0 0 0.25rem', fontSize: '0.875rem' }}>{c.content}</p>
              <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'center' }}>
                <span style={{ fontSize: '0.7rem', color: '#aaa' }}>
                  {new Date(c.createdAt).toLocaleString()}
                  {c.updatedAt ? ' (edited)' : ''}
                </span>
                {c.authorId === currentUserId && (
                  <>
                    <button
                      type="button"
                      onClick={() => startEdit(c.id, c.content)}
                      style={{
                        fontSize: '0.7rem',
                        color: '#0066cc',
                        background: 'none',
                        border: 'none',
                        cursor: 'pointer',
                        padding: 0,
                      }}
                    >
                      Edit
                    </button>
                    <button
                      type="button"
                      onClick={() =>
                        deleteComment.mutate({ id: c.id, payload: { requesterId: currentUserId } })
                      }
                      style={{
                        fontSize: '0.7rem',
                        color: '#cc0000',
                        background: 'none',
                        border: 'none',
                        cursor: 'pointer',
                        padding: 0,
                      }}
                    >
                      Delete
                    </button>
                  </>
                )}
              </div>
            </>
          )}
        </div>
      ))}

      <form
        onSubmit={handleAdd}
        style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem', marginTop: '0.5rem' }}
      >
        <textarea
          value={newText}
          onChange={e => setNewText(e.target.value)}
          rows={2}
          placeholder="Add a comment…"
          style={{
            padding: '0.4rem',
            fontSize: '0.875rem',
            resize: 'vertical',
            borderRadius: 4,
            border: '1px solid #ddd',
          }}
        />
        <button
          type="submit"
          disabled={addComment.isPending || !newText.trim()}
          style={{
            alignSelf: 'flex-end',
            padding: '0.3rem 0.75rem',
            fontSize: '0.8rem',
            borderRadius: 4,
            border: 'none',
            background: '#0066cc',
            color: '#fff',
            cursor: 'pointer',
          }}
        >
          {addComment.isPending ? 'Posting…' : 'Post'}
        </button>
      </form>
    </div>
  );
}
