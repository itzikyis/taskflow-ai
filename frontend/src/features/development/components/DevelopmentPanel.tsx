import { useState } from 'react';
import {
  useDevelopmentLinks,
  useLinkDevelopment,
  useRemoveDevelopmentLink,
} from '../hooks/useDevelopment';
import type {
  DevelopmentLink,
  DevelopmentLinkStatus,
  DevelopmentRefType,
} from '../types/development.types';
import { DEV_REF_TYPES } from '../types/development.types';

const REF_ICON: Record<DevelopmentRefType, string> = {
  Branch: '🌿',
  Commit: '🔹',
  PullRequest: '🔀',
};

const STATUS_STYLE: Record<DevelopmentLinkStatus, { color: string; bg: string; label: string }> = {
  None:   { color: 'var(--text-muted)', bg: 'var(--surface-bg)', label: '—' },
  Open:   { color: '#2563eb',           bg: '#dbeafe',           label: 'Open' },
  Draft:  { color: 'var(--text-muted)', bg: 'var(--surface-bg)', label: 'Draft' },
  Merged: { color: '#7c3aed',           bg: '#ede9fe',           label: 'Merged' },
  Closed: { color: 'var(--color-danger)', bg: '#fee2e2',         label: 'Closed' },
};

interface DevelopmentPanelProps {
  taskId: string;
}

export function DevelopmentPanel({ taskId }: DevelopmentPanelProps) {
  const { data: links, isLoading } = useDevelopmentLinks(taskId);
  const linkMutation = useLinkDevelopment(taskId);
  const removeMutation = useRemoveDevelopmentLink(taskId);

  const [showForm, setShowForm] = useState(false);
  const [repository, setRepository] = useState('');
  const [refType, setRefType] = useState<DevelopmentRefType>('PullRequest');
  const [title, setTitle] = useState('');
  const [url, setUrl] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!repository.trim() || !title.trim() || !url.trim()) return;
    linkMutation.mutate(
      { repository: repository.trim(), refType, title: title.trim(), url: url.trim() },
      {
        onSuccess: () => {
          setRepository(''); setTitle(''); setUrl(''); setRefType('PullRequest'); setShowForm(false);
        },
      },
    );
  };

  return (
    <>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 8 }}>
        <p style={{ fontSize: 12, fontWeight: 600, color: 'var(--text-secondary)', textTransform: 'uppercase', letterSpacing: '0.04em', margin: 0 }}>
          🔗 Development
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
        <p style={{ fontSize: 12, color: 'var(--text-muted)' }}>
          No linked branches or pull requests yet. Reference this task's id in a commit
          message or PR to link it automatically, or add one manually.
        </p>
      )}

      <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
        {links?.map((link: DevelopmentLink) => {
          const st = STATUS_STYLE[link.status];
          return (
            <div
              key={link.id}
              style={{
                display: 'flex', alignItems: 'center', gap: 8,
                padding: '6px 8px', border: '1px solid var(--border-color)',
                borderRadius: 6, background: 'var(--surface-bg)',
              }}
            >
              <span title={link.refType} style={{ fontSize: 14, flexShrink: 0 }}>
                {REF_ICON[link.refType]}
              </span>
              <a
                href={link.url}
                target="_blank"
                rel="noopener noreferrer"
                style={{
                  flex: 1, fontSize: 13, color: 'var(--text-primary)',
                  overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap',
                  textDecoration: 'none',
                }}
                title={`${link.repository} — ${link.title}`}
              >
                {link.title}
              </a>
              {link.status !== 'None' && (
                <span style={{
                  fontSize: 10, fontWeight: 700, color: st.color, background: st.bg,
                  padding: '2px 6px', borderRadius: 4, flexShrink: 0, textTransform: 'uppercase',
                }}>
                  {st.label}
                </span>
              )}
              <button
                type="button"
                onClick={() => removeMutation.mutate(link.id)}
                aria-label="Remove link"
                style={{
                  background: 'none', border: 'none', cursor: 'pointer',
                  color: 'var(--color-danger)', fontSize: 14, lineHeight: 1, flexShrink: 0,
                }}
              >
                ×
              </button>
            </div>
          );
        })}
      </div>

      {showForm && (
        <form onSubmit={handleSubmit} style={{ marginTop: 10, display: 'flex', flexDirection: 'column', gap: 6 }}>
          <div style={{ display: 'flex', gap: 6 }}>
            <select
              className="tf-input"
              value={refType}
              onChange={e => setRefType(e.target.value as DevelopmentRefType)}
              style={{ fontSize: 13, width: 120 }}
            >
              {DEV_REF_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
            </select>
            <input
              className="tf-input"
              value={repository}
              onChange={e => setRepository(e.target.value)}
              placeholder="owner/repo"
              style={{ fontSize: 13, flex: 1 }}
            />
          </div>
          <input
            className="tf-input"
            value={title}
            onChange={e => setTitle(e.target.value)}
            placeholder="Title (branch / PR / commit message)"
            style={{ fontSize: 13 }}
          />
          <input
            className="tf-input"
            value={url}
            onChange={e => setUrl(e.target.value)}
            placeholder="https://github.com/…"
            style={{ fontSize: 13 }}
          />
          <button
            type="submit"
            className="tf-btn tf-btn-primary tf-btn-sm"
            disabled={linkMutation.isPending || !repository.trim() || !title.trim() || !url.trim()}
          >
            {linkMutation.isPending ? 'Linking…' : 'Link reference'}
          </button>
        </form>
      )}
    </>
  );
}
