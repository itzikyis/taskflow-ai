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

export function CommentThread({ taskId, currentUserId }: CommentThreadProps) {
  const { data: comments, isLoading } = useCommentsByTask(taskId);
  const addComment    = useAddComment(taskId);
  const editComment   = useEditComment(taskId);
  const deleteComment = useDeleteComment(taskId);
  const [newText, setNewText]     = useState('');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editText, setEditText]   = useState('');

  const handleAdd = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newText.trim()) return;
    addComment.mutate(
      { authorId: currentUserId, content: newText.trim() },
      { onSuccess: () => setNewText('') },
    );
  };

  const startEdit = (id: string, content: string) => { setEditingId(id); setEditText(content); };
  const cancelEdit = () => { setEditingId(null); setEditText(''); };

  const submitEdit = (id: string) => {
    editComment.mutate(
      { id, payload: { requesterId: currentUserId, content: editText.trim() } },
      { onSuccess: cancelEdit },
    );
  };

  if (isLoading) return (
    <p style={{ fontSize: 12, color: 'var(--text-muted)' }}>Loading comments…</p>
  );

  return (
    <div>
      <p style={{ fontSize: 12, fontWeight: 600, color: 'var(--text-secondary)', marginBottom: 8, textTransform: 'uppercase', letterSpacing: '0.04em' }}>
        Comments ({comments?.length ?? 0})
      </p>

      {comments?.map(c => (
        <div key={c.id} className="comment-item">
          <div className="comment-avatar">
            {c.authorId.slice(0, 2).toUpperCase()}
          </div>
          <div className="comment-body">
            {editingId === c.id ? (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
                <textarea
                  value={editText}
                  onChange={e => setEditText(e.target.value)}
                  rows={2}
                  className="tf-input"
                  style={{ fontSize: 13, resize: 'vertical' }}
                />
                <div style={{ display: 'flex', gap: 6 }}>
                  <button
                    type="button"
                    onClick={() => submitEdit(c.id)}
                    disabled={editComment.isPending}
                    className="tf-btn tf-btn-primary tf-btn-sm"
                  >
                    Save
                  </button>
                  <button type="button" onClick={cancelEdit} className="tf-btn tf-btn-ghost tf-btn-sm">
                    Cancel
                  </button>
                </div>
              </div>
            ) : (
              <>
                <p style={{ fontSize: 13, color: 'var(--text-primary)', lineHeight: 1.5, margin: 0 }}>
                  {c.content}
                </p>
                <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginTop: 4 }}>
                  <span className="comment-meta">
                    {new Date(c.createdAt).toLocaleString(undefined, { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })}
                    {c.updatedAt ? ' · edited' : ''}
                  </span>
                  {c.authorId === currentUserId && (
                    <>
                      <button
                        type="button"
                        onClick={() => startEdit(c.id, c.content)}
                        style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--color-primary)', fontSize: 11, padding: 0 }}
                      >
                        Edit
                      </button>
                      <button
                        type="button"
                        onClick={() => deleteComment.mutate({ id: c.id, payload: { requesterId: currentUserId } })}
                        style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--color-danger)', fontSize: 11, padding: 0 }}
                      >
                        Delete
                      </button>
                    </>
                  )}
                </div>
              </>
            )}
          </div>
        </div>
      ))}

      <form onSubmit={handleAdd} style={{ display: 'flex', flexDirection: 'column', gap: 6, marginTop: 10 }}>
        <textarea
          value={newText}
          onChange={e => setNewText(e.target.value)}
          rows={2}
          placeholder="Write a comment…"
          className="tf-input"
          style={{ fontSize: 13, resize: 'vertical' }}
        />
        <button
          type="submit"
          disabled={addComment.isPending || !newText.trim()}
          className="tf-btn tf-btn-primary tf-btn-sm"
          style={{ alignSelf: 'flex-end' }}
        >
          {addComment.isPending ? 'Posting…' : 'Post'}
        </button>
      </form>
    </div>
  );
}
