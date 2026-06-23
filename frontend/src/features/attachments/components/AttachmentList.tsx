import { useAttachmentsByTask, useDeleteAttachment } from '../hooks/useAttachments';

interface AttachmentListProps {
  taskId: string;
  currentUserId: string;
}

function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

function fileIcon(name: string): string {
  const ext = name.split('.').pop()?.toLowerCase();
  if (['png', 'jpg', 'jpeg', 'gif', 'svg', 'webp'].includes(ext ?? '')) return '🖼';
  if (['pdf'].includes(ext ?? '')) return '📄';
  if (['zip', 'gz', 'tar'].includes(ext ?? '')) return '📦';
  if (['mp4', 'mov', 'avi'].includes(ext ?? '')) return '🎥';
  return '📎';
}

export function AttachmentList({ taskId, currentUserId }: AttachmentListProps) {
  const { data: attachments, isLoading } = useAttachmentsByTask(taskId);
  const deleteAttachment = useDeleteAttachment(taskId);

  if (isLoading) return <p style={{ fontSize: 12, color: 'var(--text-muted)' }}>Loading…</p>;

  if (!attachments?.length) return (
    <p style={{ fontSize: 12, color: 'var(--text-muted)', padding: '8px 0' }}>No attachments yet.</p>
  );

  return (
    <div>
      {attachments.map(a => (
        <div key={a.id} className="attachment-item">
          <span className="attachment-icon">{fileIcon(a.fileName)}</span>
          <div className="attachment-info">
            <a
              href={a.storageUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="attachment-name"
            >
              {a.fileName}
            </a>
            <div className="attachment-meta">
              {formatBytes(a.fileSizeBytes)} · {new Date(a.uploadedAt).toLocaleDateString()}
            </div>
          </div>
          {a.uploadedBy === currentUserId && (
            <button
              type="button"
              onClick={() => deleteAttachment.mutate({ id: a.id, payload: { requesterId: currentUserId } })}
              disabled={deleteAttachment.isPending}
              className="tf-btn tf-btn-danger tf-btn-sm"
              style={{ border: 'none', background: 'none', color: 'var(--text-muted)', padding: '2px 6px' }}
              title="Delete attachment"
            >
              ×
            </button>
          )}
        </div>
      ))}
    </div>
  );
}
