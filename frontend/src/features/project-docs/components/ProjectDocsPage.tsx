import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useProjects } from '@/features/projects/hooks/useProjects';
import { useAuthStore } from '@/store/authStore';
import { projectDocService, type ProjectDocumentDto } from '@/services/projectDocService';

// ── Simple markdown renderer ──────────────────────────────────────────────────
// Renders headings, bold, italic, inline code, code blocks, lists, and links.

function renderMarkdown(text: string): string {
  return text
    // Code blocks
    .replace(/```[\s\S]*?```/g, m => {
      const code = m.slice(3, -3).replace(/^[^\n]*\n/, '');
      return `<pre style="background:var(--bg-surface,#f9fafb);border:1px solid var(--border-default);border-radius:6px;padding:12px;overflow-x:auto;font-size:13px;"><code>${escHtml(code)}</code></pre>`;
    })
    // Headings
    .replace(/^### (.+)$/gm, '<h3 style="font-size:15px;margin:16px 0 6px;font-weight:700;">$1</h3>')
    .replace(/^## (.+)$/gm, '<h2 style="font-size:17px;margin:20px 0 8px;font-weight:700;">$1</h2>')
    .replace(/^# (.+)$/gm, '<h1 style="font-size:20px;margin:24px 0 10px;font-weight:800;">$1</h1>')
    // Bold / italic
    .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
    .replace(/\*(.+?)\*/g, '<em>$1</em>')
    // Inline code
    .replace(/`([^`]+)`/g, '<code style="background:var(--bg-surface,#f9fafb);padding:2px 5px;border-radius:4px;font-size:12px;">$1</code>')
    // Unordered lists
    .replace(/^[-*] (.+)$/gm, '<li style="margin:3px 0;">$1</li>')
    .replace(/(<li[^>]*>.*<\/li>\n?)+/g, '<ul style="padding-left:20px;margin:8px 0;">$&</ul>')
    // Links
    .replace(/\[(.+?)\]\((.+?)\)/g, '<a href="$2" style="color:var(--color-primary,#6366f1);text-decoration:underline;" target="_blank" rel="noopener">$1</a>')
    // Paragraphs (double newline)
    .replace(/\n\n/g, '</p><p style="margin:0 0 10px;">')
    .replace(/^(.+)$/, '<p style="margin:0 0 10px;">$1</p>');
}

function escHtml(s: string) {
  return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

// ── Doc list item ──────────────────────────────────────────────────────────────

interface DocItemProps {
  doc: ProjectDocumentDto;
  isSelected: boolean;
  onSelect: () => void;
}

function DocItem({ doc, isSelected, onSelect }: DocItemProps) {
  return (
    <button
      type="button"
      onClick={onSelect}
      style={{
        display: 'block', width: '100%', textAlign: 'left',
        padding: '10px 14px', border: 'none', borderRadius: 8, cursor: 'pointer',
        background: isSelected ? 'var(--color-primary, #6366f1)' : 'transparent',
        color: isSelected ? '#fff' : 'var(--text-primary)',
        marginBottom: 2,
      }}
    >
      <div style={{ fontWeight: 600, fontSize: 13, marginBottom: 2 }}>{doc.title}</div>
      <div style={{ fontSize: 11, opacity: 0.7 }}>
        {new Date(doc.updatedAt).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })}
      </div>
    </button>
  );
}

// ── Editor ────────────────────────────────────────────────────────────────────

interface EditorProps {
  doc: ProjectDocumentDto | null;
  projectId: string;
  authorId: string;
  onSaved: () => void;
  onDeleted: () => void;
  onNewDoc: () => void;
}

function DocEditor({ doc, projectId, authorId, onSaved, onDeleted, onNewDoc }: EditorProps) {
  const isNew = doc === null;
  const [title, setTitle]     = useState(doc?.title ?? '');
  const [body, setBody]       = useState(doc?.body ?? '');
  const [preview, setPreview] = useState(false);
  const [dirty, setDirty]     = useState(false);
  const [error, setError]     = useState<string | null>(null);
  const qc = useQueryClient();

  const KEY = ['project-docs', projectId];

  const createMutation = useMutation({
    mutationFn: () => projectDocService.create({ projectId, title, body, authorId }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEY }); setDirty(false); onSaved(); },
  });

  const updateMutation = useMutation({
    mutationFn: () => projectDocService.update(doc!.id, title, body),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEY }); setDirty(false); onSaved(); },
  });

  const deleteMutation = useMutation({
    mutationFn: () => projectDocService.delete(doc!.id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEY }); onDeleted(); },
  });

  async function handleSave() {
    setError(null);
    if (!title.trim()) { setError('Title is required.'); return; }
    try {
      if (isNew) await createMutation.mutateAsync();
      else await updateMutation.mutateAsync();
    } catch {
      setError('Failed to save document.');
    }
  }

  const saving = createMutation.isPending || updateMutation.isPending;

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      {/* Toolbar */}
      <div style={{
        display: 'flex', alignItems: 'center', gap: 10, padding: '12px 20px',
        borderBottom: '1px solid var(--border-default)',
        background: 'var(--bg-card)',
      }}>
        <input
          value={title}
          onChange={e => { setTitle(e.target.value); setDirty(true); }}
          placeholder="Document title…"
          style={{
            flex: 1, fontSize: 16, fontWeight: 700,
            border: 'none', outline: 'none', background: 'transparent',
            color: 'var(--text-primary)',
          }}
        />
        <button
          type="button"
          onClick={() => setPreview(v => !v)}
          style={{
            fontSize: 12, padding: '4px 12px', borderRadius: 6,
            border: '1px solid var(--border-default)',
            background: preview ? 'var(--color-primary,#6366f1)' : 'transparent',
            color: preview ? '#fff' : 'var(--text-secondary)',
            cursor: 'pointer',
          }}
        >
          {preview ? 'Edit' : 'Preview'}
        </button>
        <button
          type="button"
          onClick={handleSave}
          disabled={saving || !dirty}
          className="btn btn-primary"
          style={{ fontSize: 12, padding: '4px 14px' }}
        >
          {saving ? 'Saving…' : 'Save'}
        </button>
        {!isNew && (
          <button
            type="button"
            onClick={() => deleteMutation.mutate()}
            title="Delete document"
            style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--text-muted)', fontSize: 15 }}
            onMouseEnter={e => (e.currentTarget.style.color = '#ef4444')}
            onMouseLeave={e => (e.currentTarget.style.color = 'var(--text-muted)')}
          >🗑</button>
        )}
        <button
          type="button"
          onClick={onNewDoc}
          title="New document"
          style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--text-secondary)', fontSize: 18, lineHeight: 1 }}
        >+</button>
      </div>

      {error && (
        <div style={{ padding: '8px 20px', background: '#fef2f2', color: '#ef4444', fontSize: 13 }}>
          {error}
        </div>
      )}

      {/* Body */}
      <div style={{ flex: 1, overflow: 'auto' }}>
        {preview ? (
          <div
            style={{ padding: '24px 32px', lineHeight: 1.7, fontSize: 14, color: 'var(--text-primary)' }}
            dangerouslySetInnerHTML={{ __html: renderMarkdown(body) }}
          />
        ) : (
          <textarea
            value={body}
            onChange={e => { setBody(e.target.value); setDirty(true); }}
            placeholder={'# Document Title\n\nWrite your spec, decision notes, or documentation here.\n\nMarkdown is supported — **bold**, *italic*, `code`, lists, headings, and code blocks.\n'}
            style={{
              width: '100%', height: '100%', border: 'none', outline: 'none',
              resize: 'none', padding: '24px 32px', fontSize: 14, lineHeight: 1.7,
              fontFamily: 'var(--font-mono, monospace)',
              background: 'var(--bg-main, #fff)', color: 'var(--text-primary)',
              boxSizing: 'border-box',
            }}
          />
        )}
      </div>
    </div>
  );
}

// ── Main page ─────────────────────────────────────────────────────────────────

export function ProjectDocsPage() {
  const { data: projects = [], isLoading: projectsLoading } = useProjects();
  const [projectId, setProjectId] = useState('');
  const [selectedDocId, setSelectedDocId] = useState<string | 'new' | null>(null);
  const { token } = useAuthStore();

  const effectiveProjectId = projectId || (projects[0]?.id ?? '');

  const { data: docs = [], isLoading: docsLoading } = useQuery({
    queryKey: ['project-docs', effectiveProjectId],
    queryFn: () => projectDocService.getByProject(effectiveProjectId),
    enabled: Boolean(effectiveProjectId),
  });

  const selectedDoc = selectedDocId && selectedDocId !== 'new'
    ? docs.find(d => d.id === selectedDocId) ?? null
    : null;

  const showEditor = selectedDocId !== null;

  return (
    <div style={{ display: 'flex', height: '100%', overflow: 'hidden' }}>
      {/* Sidebar: project + doc list */}
      <div style={{
        width: 240, flexShrink: 0,
        borderRight: '1px solid var(--border-default)',
        display: 'flex', flexDirection: 'column',
        background: 'var(--bg-surface, #f9fafb)',
      }}>
        {/* Project selector */}
        <div style={{ padding: '16px 14px 10px', borderBottom: '1px solid var(--border-default)' }}>
          <div style={{ fontSize: 11, fontWeight: 700, color: 'var(--text-muted)', marginBottom: 6, textTransform: 'uppercase', letterSpacing: 0.5 }}>
            Project
          </div>
          {projectsLoading ? (
            <div style={{ fontSize: 13, color: 'var(--text-secondary)' }}>Loading…</div>
          ) : (
            <select
              value={effectiveProjectId}
              onChange={e => { setProjectId(e.target.value); setSelectedDocId(null); }}
              style={{ width: '100%', fontSize: 13, padding: '4px 8px', borderRadius: 6, border: '1px solid var(--border-default)' }}
            >
              {projects.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
            </select>
          )}
        </div>

        {/* Doc list */}
        <div style={{ flex: 1, overflow: 'auto', padding: '10px 8px' }}>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '0 6px', marginBottom: 8 }}>
            <span style={{ fontSize: 11, fontWeight: 700, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: 0.5 }}>
              Documents
            </span>
            <button
              type="button"
              onClick={() => setSelectedDocId('new')}
              title="New document"
              style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--color-primary,#6366f1)', fontSize: 18, lineHeight: 1, padding: 0 }}
            >+</button>
          </div>

          {docsLoading ? (
            <div style={{ fontSize: 12, color: 'var(--text-secondary)', padding: '0 6px' }}>Loading…</div>
          ) : docs.length === 0 ? (
            <div style={{ fontSize: 12, color: 'var(--text-secondary)', padding: '0 6px', lineHeight: 1.5 }}>
              No documents yet.{' '}
              <button
                type="button"
                onClick={() => setSelectedDocId('new')}
                style={{ background: 'none', border: 'none', padding: 0, color: 'var(--color-primary,#6366f1)', cursor: 'pointer', fontSize: 12 }}
              >Create one</button>
            </div>
          ) : (
            docs.map(doc => (
              <DocItem
                key={doc.id}
                doc={doc}
                isSelected={selectedDocId === doc.id}
                onSelect={() => setSelectedDocId(doc.id)}
              />
            ))
          )}
        </div>
      </div>

      {/* Editor / welcome */}
      <div style={{ flex: 1, overflow: 'hidden', display: 'flex', flexDirection: 'column' }}>
        {showEditor && effectiveProjectId ? (
          <DocEditor
            key={selectedDocId ?? 'new'}
            doc={selectedDocId === 'new' ? null : selectedDoc}
            projectId={effectiveProjectId}
            authorId={token?.userId ?? ''}
            onSaved={() => { /* stay on saved doc, list will refresh */ }}
            onDeleted={() => setSelectedDocId(null)}
            onNewDoc={() => setSelectedDocId('new')}
          />
        ) : (
          <div style={{
            flex: 1, display: 'flex', flexDirection: 'column',
            alignItems: 'center', justifyContent: 'center',
            color: 'var(--text-secondary)', gap: 12,
          }}>
            <div style={{ fontSize: 48 }}>📄</div>
            <div style={{ fontWeight: 700, fontSize: 16, color: 'var(--text-primary)' }}>Project Docs</div>
            <div style={{ fontSize: 13, maxWidth: 300, textAlign: 'center', lineHeight: 1.6 }}>
              Keep specs, decisions, and notes alongside your tasks. Select a doc from the list or create a new one.
            </div>
            <button
              type="button"
              className="btn btn-primary"
              onClick={() => setSelectedDocId('new')}
              style={{ fontSize: 13, marginTop: 4 }}
            >
              + New Document
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
