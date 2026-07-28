import { useState } from 'react';
import {
  useDevelopmentLinks,
  useCreateDevelopmentLink,
  useRemoveDevelopmentLink,
} from '../hooks/useDevelopmentLinks';
import {
  DEVELOPMENT_LINK_TYPES,
  type DevelopmentLink,
  type DevelopmentLinkStatus,
  type DevelopmentLinkType,
} from '@/services/developmentLinkService';

const LINK_ICON: Record<DevelopmentLinkType, string> = {
  PullRequest: '🔀',
  Commit: '🔹',
  Branch: '🌿',
};

const STATUS_STYLE: Record<DevelopmentLinkStatus, { color: string; bg: string; label: string }> = {
  None:   { color: 'var(--text-muted)',    bg: 'var(--surface-bg)', label: '—' },
  Open:   { color: '#2563eb',              bg: '#dbeafe',           label: 'Open' },
  Draft:  { color: 'var(--text-muted)',    bg: 'var(--surface-bg)', label: 'Draft' },
  Merged: { color: '#7c3aed',              bg: '#ede9fe',           label: 'Merged' },
  Closed: { color: 'var(--color-danger)',  bg: '#fee2e2',           label: 'Closed' },
};

interface DevelopmentLinksProps {
  taskId: string;
}

export function DevelopmentLinks({ taskId }: DevelopmentLinksProps) {
  const { data: links, isLoading } = useDevelopmentLinks(taskId);
  const createMutation = useCreateDevelopmentLink(taskId);
  const removeMutation = useRemoveDevelopmentLink(taskId);

  const [showForm, setShowForm] = useState(false);
  const [linkType, setLinkType] = useState<DevelopmentLinkType>('PullRequest');
  const [url, setUrl] = useState('');
  const [title, setTitle] = useState('');
  const [formError, setFormError] = useState('');

  const resetForm = () => {
    setLinkType('PullRequest');
    setUrl('');
    setTitle('');
    setFormError('');
    setShowForm(false);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const trimmedUrl = url.trim();
    const trimmedTitle = title.trim();
    if (!trimmedUrl || !trimmedTitle) return;

    setFormError('');
    createMutation.mutate(
      { linkType, url: trimmedUrl, title: trimmedTitle },
      {
        onSuccess: resetForm,
        onError: (err: unknown) => {
          const msg =
            (err as { response?: { data?: { description?: string } } })
              ?.response?.data?.description;
          setFormError(msg ?? 'Could not add link.');
        },
      },
    );
  };

  return (
    <>
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          marginBottom: 8,
        }}
      >
        <p
          style={{
            fontSize: 12,
            fontWeight: 600,
            color: 'var(--text-secondary)',
            textTransform: 'uppercase',
            letterSpacing: '0.04em',
            margin: 0,
          }}
        >
          🔗 GitHub Links
        </p>
        <button
          type="button"
          className="tf-btn tf-btn-ghost tf-btn-sm"
          onClick={() => setShowForm(v => !v)}
          style={{ fontSize: 11 }}
        >
          {showForm ? 'Cancel' : '+ Link'}
        </button>
      </div>

      {isLoading && (
        <p style={{ fontSize: 12, color: 'var(--text-muted)' }}>Loading…</p>
      )}

      {!isLoading && (links?.length ?? 0) === 0 && !showForm && (
        <p style={{ fontSize: 12, color: 'var(--text-muted)', margin: 0 }}>
          No linked PRs or commits yet. Add one manually or reference this task
          in a commit message to link it automatically.
        </p>
      )}

      {(links?.length ?? 0) > 0 && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
          {links!.map((link: DevelopmentLink) => {
            const st = STATUS_STYLE[link.status];
            return (
              <div
                key={link.id}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 8,
                  padding: '6px 8px',
                  border: '1px solid var(--surface-border)',
                  borderRadius: 'var(--radius-md)',
                  background: 'var(--surface-bg)',
                }}
              >
                <span title={link.linkType} style={{ fontSize: 14, flexShrink: 0 }}>
                  {LINK_ICON[link.linkType]}
                </span>
                <a
                  href={link.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  style={{
                    flex: 1,
                    fontSize: 13,
                    color: 'var(--text-primary)',
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    whiteSpace: 'nowrap',
                    textDecoration: 'none',
                  }}
                  title={link.title}
                >
                  {link.title}
                </a>
                {link.status !== 'None' && (
                  <span
                    style={{
                      fontSize: 10,
                      fontWeight: 700,
                      color: st.color,
                      background: st.bg,
                      padding: '2px 6px',
                      borderRadius: 4,
                      flexShrink: 0,
                      textTransform: 'uppercase',
                    }}
                  >
                    {st.label}
                  </span>
                )}
                <button
                  type="button"
                  onClick={() => removeMutation.mutate(link.id)}
                  aria-label="Remove link"
                  style={{
                    background: 'none',
                    border: 'none',
                    cursor: 'pointer',
                    color: 'var(--color-danger)',
                    fontSize: 14,
                    lineHeight: 1,
                    flexShrink: 0,
                  }}
                >
                  ×
                </button>
              </div>
            );
          })}
        </div>
      )}

      {showForm && (
        <form
          onSubmit={handleSubmit}
          style={{ marginTop: 10, display: 'flex', flexDirection: 'column', gap: 6 }}
        >
          <select
            className="tf-input"
            value={linkType}
            onChange={e => setLinkType(e.target.value as DevelopmentLinkType)}
            style={{ fontSize: 13 }}
            aria-label="Link type"
          >
            {DEVELOPMENT_LINK_TYPES.map(t => (
              <option key={t} value={t}>
                {t === 'PullRequest' ? 'Pull Request' : t}
              </option>
            ))}
          </select>
          <input
            className="tf-input"
            value={title}
            onChange={e => setTitle(e.target.value)}
            placeholder="Title (PR title, branch name, or commit message)"
            style={{ fontSize: 13 }}
            aria-label="Link title"
          />
          <input
            className="tf-input"
            value={url}
            onChange={e => setUrl(e.target.value)}
            placeholder="https://github.com/owner/repo/pull/123"
            style={{ fontSize: 13 }}
            aria-label="GitHub URL"
          />
          <button
            type="submit"
            className="tf-btn tf-btn-primary tf-btn-sm"
            disabled={
              createMutation.isPending || !url.trim() || !title.trim()
            }
          >
            {createMutation.isPending ? 'Linking…' : 'Add link'}
          </button>
          {formError && (
            <p style={{ fontSize: 11, color: 'var(--color-danger)', margin: 0 }}>
              {formError}
            </p>
          )}
        </form>
      )}
    </>
  );
}
