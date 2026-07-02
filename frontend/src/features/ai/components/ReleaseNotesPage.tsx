import { useState } from 'react';
import type { CSSProperties } from 'react';
import { useAllTasks } from '@/features/tasks/hooks/useTasks';
import { useGenerateReleaseNotes } from '../hooks/useAi';
import type { ReleaseNotes } from '@/services/aiService';

const DEFAULT_VERSION = '1.0.0';

const styles = {
  layout: {
    display: 'grid',
    gridTemplateColumns: '280px 1fr',
    gap: 24,
    alignItems: 'start',
  } satisfies CSSProperties,

  panel: {
    background: 'var(--surface-bg)',
    border: '1px solid var(--border-color)',
    borderRadius: 'var(--radius-md)',
    padding: 20,
  } satisfies CSSProperties,

  label: {
    display: 'block',
    fontSize: 13,
    fontWeight: 600,
    color: 'var(--text-secondary)',
    marginBottom: 6,
    textTransform: 'uppercase' as const,
    letterSpacing: '0.04em',
  } satisfies CSSProperties,

  summaryBox: {
    background: 'var(--color-primary-light, #ede9fe)',
    border: '1px solid var(--color-primary)',
    borderRadius: 'var(--radius-md)',
    padding: '14px 18px',
    marginBottom: 20,
  } satisfies CSSProperties,

  sectionTitle: {
    fontSize: 15,
    fontWeight: 700,
    color: 'var(--text-primary)',
    marginBottom: 10,
    marginTop: 0,
    display: 'flex',
    alignItems: 'center',
    gap: 6,
  } satisfies CSSProperties,

  listItem: {
    fontSize: 14,
    color: 'var(--text-primary)',
    lineHeight: 1.6,
    padding: '6px 0',
    borderBottom: '1px solid var(--border-color)',
  } satisfies CSSProperties,

  spinnerWrap: {
    display: 'flex',
    flexDirection: 'column' as const,
    alignItems: 'center',
    gap: 12,
    padding: '48px 0',
    color: 'var(--text-muted)',
    fontSize: 14,
  } satisfies CSSProperties,

  pre: {
    background: 'var(--surface-raised, #f8f8f8)',
    border: '1px solid var(--border-color)',
    borderRadius: 'var(--radius-md)',
    padding: '14px 16px',
    fontSize: 13,
    lineHeight: 1.6,
    overflowX: 'auto' as const,
    whiteSpace: 'pre-wrap' as const,
    wordBreak: 'break-word' as const,
    color: 'var(--text-secondary)',
    margin: 0,
  } satisfies CSSProperties,
};

export function ReleaseNotesPage() {
  const { data: allTasks = [] } = useAllTasks();
  const { mutate: generate, isPending, data: notes, reset } = useGenerateReleaseNotes();

  const [version, setVersion] = useState(DEFAULT_VERSION);
  const [rawOpen, setRawOpen] = useState(false);
  const [copied, setCopied] = useState(false);

  const completedTasks = allTasks.filter(t => t.status === 'Done');

  function handleGenerate() {
    generate({
      version,
      completedTasks: completedTasks.map(t => ({
        title: t.title,
        description: t.description ?? undefined,
        priority: t.priority,
      })),
    });
  }

  function handleCopy() {
    if (!notes) return;
    void navigator.clipboard.writeText(notes.markdownContent).then(() => {
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    });
  }

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">AI Release Notes Generator 📋</h1>
      </div>

      <div style={styles.layout}>
        {/* ── Controls panel ────────────────────────────────────── */}
        <aside style={styles.panel}>
          <div style={{ marginBottom: 20 }}>
            <label htmlFor="release-version" style={styles.label}>
              Version
            </label>
            <input
              id="release-version"
              type="text"
              className="tf-input"
              placeholder="e.g. 1.0.0"
              value={version}
              onChange={e => {
                setVersion(e.target.value);
                reset();
              }}
              style={{ width: '100%' }}
            />
          </div>

          <div style={{ marginBottom: 20, fontSize: 12, color: 'var(--text-muted)' }}>
            {completedTasks.length} completed task{completedTasks.length !== 1 ? 's' : ''} will be included
          </div>

          <button
            type="button"
            className="tf-btn tf-btn-primary"
            style={{ width: '100%' }}
            disabled={isPending || completedTasks.length === 0 || version.trim() === ''}
            onClick={handleGenerate}
          >
            {isPending ? 'Generating...' : 'Generate Release Notes'}
          </button>
        </aside>

        {/* ── Results panel ─────────────────────────────────────── */}
        <section style={styles.panel}>
          {isPending && <LoadingState />}

          {!isPending && !notes && (
            <EmptyOrReadyState hasCompleted={completedTasks.length > 0} />
          )}

          {!isPending && notes && (
            <NotesResults
              notes={notes}
              copied={copied}
              rawOpen={rawOpen}
              onCopy={handleCopy}
              onToggleRaw={() => setRawOpen(p => !p)}
            />
          )}
        </section>
      </div>
    </div>
  );
}

function LoadingState() {
  return (
    <div style={styles.spinnerWrap}>
      <Spinner />
      <span>Generating release notes...</span>
    </div>
  );
}

function EmptyOrReadyState({ hasCompleted }: { hasCompleted: boolean }) {
  if (!hasCompleted) {
    return (
      <div className="empty-state">
        <div className="empty-state-icon">📋</div>
        <div className="empty-state-text">
          No completed tasks found. Mark tasks as Done first.
        </div>
      </div>
    );
  }
  return (
    <div className="empty-state">
      <div className="empty-state-icon">📝</div>
      <div className="empty-state-text">
        Enter a version number and click Generate Release Notes to create AI-powered release notes.
      </div>
    </div>
  );
}

function NotesResults({
  notes,
  copied,
  rawOpen,
  onCopy,
  onToggleRaw,
}: {
  notes: ReleaseNotes;
  copied: boolean;
  rawOpen: boolean;
  onCopy: () => void;
  onToggleRaw: () => void;
}) {
  return (
    <>
      {/* Header row with copy button */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <h2 style={{ margin: 0, fontSize: 22, fontWeight: 800, color: 'var(--text-primary)' }}>
          v{notes.version}
        </h2>
        <button type="button" className="tf-btn tf-btn-ghost" onClick={onCopy}>
          {copied ? '✅ Copied!' : '📋 Copy Markdown'}
        </button>
      </div>

      {/* Summary */}
      <div style={styles.summaryBox}>
        <div style={{ fontSize: 11, fontWeight: 700, textTransform: 'uppercase', letterSpacing: '0.06em', color: 'var(--color-primary)', marginBottom: 6 }}>
          Summary
        </div>
        <p style={{ margin: 0, fontSize: 14, color: 'var(--text-primary)', lineHeight: 1.6 }}>
          {notes.summary}
        </p>
      </div>

      {/* Sections */}
      {notes.features.length > 0 && (
        <NoteSection icon="✨" title="New Features" items={notes.features} />
      )}
      {notes.bugFixes.length > 0 && (
        <NoteSection icon="🐛" title="Bug Fixes" items={notes.bugFixes} />
      )}
      {notes.improvements.length > 0 && (
        <NoteSection icon="🔧" title="Improvements" items={notes.improvements} />
      )}

      {/* Collapsible raw markdown */}
      <div style={{ marginTop: 24 }}>
        <button
          type="button"
          className="tf-btn tf-btn-ghost tf-btn-sm"
          onClick={onToggleRaw}
          style={{ marginBottom: rawOpen ? 10 : 0 }}
        >
          {rawOpen ? '▼' : '▶'} Raw Markdown
        </button>
        {rawOpen && (
          <pre style={styles.pre}>{notes.markdownContent}</pre>
        )}
      </div>
    </>
  );
}

function NoteSection({ icon, title, items }: { icon: string; title: string; items: string[] }) {
  return (
    <div style={{ marginBottom: 20 }}>
      <p style={styles.sectionTitle}>
        <span>{icon}</span>
        {title}
      </p>
      <ul style={{ margin: 0, padding: 0, listStyle: 'none' }}>
        {items.map((item, i) => (
          <li key={i} style={styles.listItem}>
            <span style={{ marginRight: 8, color: 'var(--text-muted)' }}>–</span>
            {item}
          </li>
        ))}
      </ul>
    </div>
  );
}

function Spinner() {
  return (
    <>
      <div
        style={{
          width: 32,
          height: 32,
          border: '3px solid var(--border-color)',
          borderTop: '3px solid var(--color-primary)',
          borderRadius: '50%',
          animation: 'spin 0.8s linear infinite',
        }}
      />
      <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
    </>
  );
}
