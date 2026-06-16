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

export function AttachmentList({ taskId, currentUserId }: AttachmentListProps) {
  const { data: attachments, isLoading } = useAttachmentsByTask(taskId);
  const deleteAttachment = useDeleteAttachment(taskId);

  if (isLoading) return <p style={{ fontSize: '0.875rem', color: '#888' }}>Loading attachments…</p>;
  if (!attachments?.length) return <p style={{ fontSize: '0.875rem', color: '#aaa' }}>No attachments.</p>;

  return (
    <ul style={{ listStyle: 'none', padding: 0, margin: 0 }}>
      {attachments.map(a => (
        <li
          key={a.id}
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            padding: '0.4rem 0',
            borderBottom: '1px solid #f0f0f0',
            fontSize: '0.875rem',
          }}
        >
          <div>
            <a
              href={a.storageUrl}
              target="_blank"
              rel="noopener noreferrer"
              style={{ color: '#0066cc', textDecoration: 'none' }}
            >
              {a.fileName}
            </a>
            <span style={{ color: '#aaa', marginLeft: '0.5rem', fontSize: '0.75rem' }}>
              {formatBytes(a.fileSizeBytes)} · {new Date(a.uploadedAt).toLocaleDateString()}
            </span>
          </div>
          {a.uploadedBy === currentUserId && (
            <button
              type="button"
              onClick={() => deleteAttachment.mutate({ id: a.id, payload: { requesterId: currentUserId } })}
              disabled={deleteAttachment.isPending}
              style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#cc0000', fontSize: '0.75rem' }}
            >
              Delete
            </button>
          )}
        </li>
      ))}
    </ul>
  );
}
