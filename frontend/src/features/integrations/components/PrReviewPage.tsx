import { useState } from 'react';

// ── Types ─────────────────────────────────────────────────────────────────────

type CiStatus = 'passing' | 'failing' | 'pending';

interface PullRequest {
  readonly id: number;
  readonly title: string;
  readonly branch: string;
  readonly ciStatus: CiStatus;
  readonly age: string;
  readonly author: string;
  readonly additions: number;
  readonly deletions: number;
  readonly files: readonly DiffFile[];
}

interface DiffFile {
  readonly name: string;
  readonly additions: number;
  readonly deletions: number;
  readonly lines: readonly DiffLine[];
}

type DiffLineKind = 'add' | 'remove' | 'context';

interface DiffLine {
  readonly kind: DiffLineKind;
  readonly lineNo: number | null;
  readonly content: string;
}

type ReviewKind = 'comment' | 'approve' | 'request-changes';

interface InlineComment {
  readonly fileIndex: number;
  readonly lineNo: number;
  readonly text: string;
}

// ── Constants ─────────────────────────────────────────────────────────────────

const CI_STATUS_ICON: Record<CiStatus, string> = {
  passing: '✅',
  failing: '❌',
  pending: '🔄',
};

const CI_STATUS_LABEL: Record<CiStatus, string> = {
  passing: 'Passing',
  failing: 'Failing',
  pending: 'Pending',
};

const CI_STATUS_COLOR: Record<CiStatus, string> = {
  passing: '#22c55e',
  failing: '#ef4444',
  pending: '#f59e0b',
};

const SIDEBAR_WIDTH = 280;
const REVIEW_PANEL_WIDTH = 280;

const MOCK_PRS: readonly PullRequest[] = [
  {
    id: 142,
    title: 'Fix auth timeout',
    branch: 'feature/auth',
    ciStatus: 'passing',
    age: '2h ago',
    author: 'sarah.chen',
    additions: 47,
    deletions: 12,
    files: [
      {
        name: 'src/auth/tokenRefresh.ts',
        additions: 31,
        deletions: 8,
        lines: [
          { kind: 'context', lineNo: 1,   content: ' import { jwtDecode } from \'jwt-decode\';' },
          { kind: 'context', lineNo: 2,   content: ' import { authService } from \'../services/authService\';' },
          { kind: 'context', lineNo: 3,   content: '' },
          { kind: 'remove',  lineNo: 4,   content: '-const REFRESH_TIMEOUT_MS = 30000;' },
          { kind: 'add',     lineNo: null, content: '+const REFRESH_TIMEOUT_MS = 5000;' },
          { kind: 'add',     lineNo: null, content: '+const MAX_RETRY_ATTEMPTS = 3;' },
          { kind: 'context', lineNo: 5,   content: '' },
          { kind: 'context', lineNo: 6,   content: ' export async function refreshAccessToken(token: string): Promise<string> {' },
          { kind: 'remove',  lineNo: 7,   content: '-  return authService.refresh(token);' },
          { kind: 'add',     lineNo: null, content: '+  let attempt = 0;' },
          { kind: 'add',     lineNo: null, content: '+  while (attempt < MAX_RETRY_ATTEMPTS) {' },
          { kind: 'add',     lineNo: null, content: '+    try {' },
          { kind: 'add',     lineNo: null, content: '+      return await authService.refresh(token);' },
          { kind: 'add',     lineNo: null, content: '+    } catch {' },
          { kind: 'add',     lineNo: null, content: '+      attempt++;' },
          { kind: 'add',     lineNo: null, content: '+    }' },
          { kind: 'add',     lineNo: null, content: '+  }' },
          { kind: 'add',     lineNo: null, content: '+  throw new Error(\'Token refresh failed after max retries\');' },
          { kind: 'context', lineNo: 8,   content: ' }' },
        ],
      },
      {
        name: 'src/auth/useAuthStore.ts',
        additions: 16,
        deletions: 4,
        lines: [
          { kind: 'context', lineNo: 1, content: ' import { create } from \'zustand\';' },
          { kind: 'context', lineNo: 2, content: ' import { persist } from \'zustand/middleware\';' },
          { kind: 'remove',  lineNo: 3, content: '-  expiresAt: number;' },
          { kind: 'add',     lineNo: null, content: '+  expiresAt: number;' },
          { kind: 'add',     lineNo: null, content: '+  refreshAttempts: number;' },
          { kind: 'context', lineNo: 4, content: '}' },
        ],
      },
    ],
  },
  {
    id: 141,
    title: 'Add custom fields',
    branch: 'feature/146',
    ciStatus: 'failing',
    age: '4h ago',
    author: 'marcos.dev',
    additions: 234,
    deletions: 89,
    files: [
      {
        name: 'src/features/tasks/components/CustomFieldsPanel.tsx',
        additions: 180,
        deletions: 0,
        lines: [
          { kind: 'add', lineNo: null, content: '+import { useState } from \'react\';' },
          { kind: 'add', lineNo: null, content: '+' },
          { kind: 'add', lineNo: null, content: '+export type FieldType = \'text\' | \'number\' | \'date\' | \'select\';' },
          { kind: 'add', lineNo: null, content: '+' },
          { kind: 'add', lineNo: null, content: '+interface CustomField {' },
          { kind: 'add', lineNo: null, content: '+  id: string;' },
          { kind: 'add', lineNo: null, content: '+  label: string;' },
          { kind: 'add', lineNo: null, content: '+  type: FieldType;' },
          { kind: 'add', lineNo: null, content: '+  value: string;' },
          { kind: 'add', lineNo: null, content: '+}' },
        ],
      },
    ],
  },
  {
    id: 139,
    title: 'Team analytics',
    branch: 'feature/145',
    ciStatus: 'passing',
    age: '1d ago',
    author: 'priya.k',
    additions: 312,
    deletions: 44,
    files: [
      {
        name: 'src/features/reporting/components/TeamAnalyticsPage.tsx',
        additions: 312,
        deletions: 44,
        lines: [
          { kind: 'context', lineNo: 1, content: ' import { useQuery } from \'@tanstack/react-query\';' },
          { kind: 'remove',  lineNo: 2, content: '-import { BarChart } from \'recharts\';' },
          { kind: 'add',     lineNo: null, content: '+import { BarChart, LineChart } from \'recharts\';' },
          { kind: 'context', lineNo: 3, content: '' },
          { kind: 'add',     lineNo: null, content: '+const CHART_HEIGHT = 320;' },
          { kind: 'add',     lineNo: null, content: '+const COLOR_VELOCITY = \'#6366f1\';' },
        ],
      },
    ],
  },
  {
    id: 137,
    title: 'AI forecasting',
    branch: 'feature/142',
    ciStatus: 'pending',
    age: '2d ago',
    author: 'lee.w',
    additions: 89,
    deletions: 22,
    files: [
      {
        name: 'src/features/ai/components/ForecastPage.tsx',
        additions: 89,
        deletions: 22,
        lines: [
          { kind: 'add', lineNo: null, content: '+export function ForecastPage() {' },
          { kind: 'add', lineNo: null, content: '+  return <div>AI Forecast</div>;' },
          { kind: 'add', lineNo: null, content: '+}' },
        ],
      },
    ],
  },
  {
    id: 135,
    title: 'Import tool',
    branch: 'feature/147',
    ciStatus: 'passing',
    age: '3d ago',
    author: 'nina.r',
    additions: 156,
    deletions: 67,
    files: [
      {
        name: 'src/features/import/components/ImportPage.tsx',
        additions: 156,
        deletions: 67,
        lines: [
          { kind: 'context', lineNo: 1, content: ' import { useState, useCallback } from \'react\';' },
          { kind: 'remove',  lineNo: 2, content: '-const MAX_FILE_SIZE = 1048576;' },
          { kind: 'add',     lineNo: null, content: '+const MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024;' },
        ],
      },
    ],
  },
];

// ── Sub-components ────────────────────────────────────────────────────────────

interface PrListItemProps {
  readonly pr: PullRequest;
  readonly selected: boolean;
  readonly onClick: () => void;
}

function PrListItem({ pr, selected, onClick }: PrListItemProps) {
  return (
    <button
      type="button"
      onClick={onClick}
      style={{
        display: 'block',
        width: '100%',
        textAlign: 'left',
        background: selected ? 'var(--color-primary, #6366f1)15' : 'transparent',
        border: 'none',
        borderLeft: selected ? '3px solid var(--color-primary, #6366f1)' : '3px solid transparent',
        borderRadius: 0,
        padding: '10px 14px',
        cursor: 'pointer',
        transition: 'background 0.15s',
      }}
    >
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 4 }}>
        <span style={{ fontSize: 12, fontWeight: 600, color: 'var(--color-primary, #6366f1)' }}>
          #{pr.id}
        </span>
        <span style={{ fontSize: 11, color: 'var(--text-muted, #6b7280)' }}>{pr.age}</span>
      </div>
      <div style={{ fontSize: 13, fontWeight: 500, color: 'var(--text-primary)', marginBottom: 4, lineHeight: 1.3 }}>
        {pr.title}
      </div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <span style={{ fontSize: 11, color: 'var(--text-muted, #6b7280)', fontFamily: 'monospace' }}>
          {pr.branch}
        </span>
        <span style={{ fontSize: 11, color: CI_STATUS_COLOR[pr.ciStatus], fontWeight: 500 }}>
          {CI_STATUS_ICON[pr.ciStatus]} {CI_STATUS_LABEL[pr.ciStatus]}
        </span>
      </div>
    </button>
  );
}

interface DiffLineRowProps {
  readonly line: DiffLine;
  readonly onAddComment: (lineNo: number) => void;
  readonly pendingLineNo: number | null;
}

function DiffLineRow({ line, onAddComment, pendingLineNo }: DiffLineRowProps) {
  const [hovered, setHovered] = useState(false);

  const bgColor: React.CSSProperties['background'] =
    line.kind === 'add'    ? 'rgba(34, 197, 94, 0.12)' :
    line.kind === 'remove' ? 'rgba(239, 68, 68, 0.12)' :
    'transparent';

  const textColor: string =
    line.kind === 'add'    ? '#4ade80' :
    line.kind === 'remove' ? '#f87171' :
    'var(--text-secondary, #9ca3af)';

  const displayLineNo = line.lineNo != null ? String(line.lineNo) : '';
  const isCommentable = line.lineNo != null;
  const isPending = isCommentable && pendingLineNo === line.lineNo;

  return (
    <div
      onMouseEnter={() => setHovered(true)}
      onMouseLeave={() => setHovered(false)}
      style={{ display: 'flex', alignItems: 'stretch', background: bgColor, position: 'relative' }}
    >
      <span style={{
        width: 36,
        minWidth: 36,
        textAlign: 'right',
        paddingRight: 8,
        color: 'var(--text-muted, #6b7280)',
        fontSize: 11,
        fontFamily: 'monospace',
        userSelect: 'none',
        background: 'rgba(0,0,0,0.12)',
        lineHeight: '20px',
      }}>
        {displayLineNo}
      </span>
      <pre style={{ margin: 0, padding: '0 8px', fontSize: 12, fontFamily: 'monospace', color: textColor, flex: 1, lineHeight: '20px', overflowX: 'auto' }}>
        {line.content}
      </pre>
      {isCommentable && (hovered || isPending) && (
        <button
          type="button"
          onClick={() => onAddComment(line.lineNo as number)}
          title="Add comment"
          style={{
            position: 'absolute',
            right: 4,
            top: '50%',
            transform: 'translateY(-50%)',
            width: 18,
            height: 18,
            background: 'var(--color-primary, #6366f1)',
            border: 'none',
            borderRadius: '50%',
            color: '#fff',
            fontSize: 12,
            lineHeight: '18px',
            cursor: 'pointer',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontWeight: 700,
          }}
        >
          +
        </button>
      )}
    </div>
  );
}

interface InlineCommentFormProps {
  readonly lineNo: number;
  readonly onSubmit: (text: string) => void;
  readonly onCancel: () => void;
}

function InlineCommentForm({ lineNo, onSubmit, onCancel }: InlineCommentFormProps) {
  const [text, setText] = useState('');
  return (
    <div style={{
      margin: '4px 8px',
      padding: 10,
      background: 'var(--surface-card, #1e1e2e)',
      border: '1px solid var(--surface-border, #2d2d3a)',
      borderRadius: 'var(--radius-md, 6px)',
    }}>
      <div style={{ fontSize: 11, color: 'var(--text-muted, #6b7280)', marginBottom: 6 }}>
        Comment on line {lineNo}
      </div>
      <textarea
        value={text}
        onChange={e => setText(e.target.value)}
        rows={3}
        placeholder="Leave a comment..."
        style={{
          width: '100%',
          boxSizing: 'border-box',
          resize: 'vertical',
          background: 'var(--surface-bg, #141420)',
          border: '1px solid var(--surface-border, #2d2d3a)',
          borderRadius: 4,
          color: 'var(--text-primary)',
          fontSize: 12,
          padding: '6px 8px',
          fontFamily: 'inherit',
          outline: 'none',
        }}
      />
      <div style={{ display: 'flex', gap: 6, marginTop: 6, justifyContent: 'flex-end' }}>
        <button type="button" onClick={onCancel} style={ghostBtnStyle}>Cancel</button>
        <button
          type="button"
          disabled={!text.trim()}
          onClick={() => { if (text.trim()) onSubmit(text.trim()); }}
          style={primaryBtnStyle(!!text.trim())}
        >
          Comment
        </button>
      </div>
    </div>
  );
}

const ghostBtnStyle: React.CSSProperties = {
  fontSize: 12,
  padding: '4px 10px',
  background: 'transparent',
  border: '1px solid var(--surface-border, #2d2d3a)',
  borderRadius: 4,
  color: 'var(--text-secondary)',
  cursor: 'pointer',
};

const primaryBtnStyle = (active: boolean): React.CSSProperties => ({
  fontSize: 12,
  padding: '4px 10px',
  background: active ? 'var(--color-primary, #6366f1)' : 'var(--surface-border, #2d2d3a)',
  border: 'none',
  borderRadius: 4,
  color: active ? '#fff' : 'var(--text-muted, #6b7280)',
  cursor: active ? 'pointer' : 'not-allowed',
});

interface DiffViewerProps {
  readonly file: DiffFile;
  readonly fileIndex: number;
  readonly inlineComments: readonly InlineComment[];
  readonly onAddComment: (fileIndex: number, lineNo: number, text: string) => void;
}

function DiffViewer({ file, fileIndex, inlineComments, onAddComment }: DiffViewerProps) {
  const [pendingLineNo, setPendingLineNo] = useState<number | null>(null);

  const handleAddComment = (lineNo: number) => {
    setPendingLineNo(lineNo === pendingLineNo ? null : lineNo);
  };

  const handleSubmitComment = (lineNo: number, text: string) => {
    onAddComment(fileIndex, lineNo, text);
    setPendingLineNo(null);
  };

  const commentsForFile = inlineComments.filter(c => c.fileIndex === fileIndex);

  return (
    <div style={{
      border: '1px solid var(--surface-border, #2d2d3a)',
      borderRadius: 'var(--radius-md, 6px)',
      overflow: 'hidden',
      fontFamily: 'monospace',
    }}>
      {file.lines.map((line, idx) => {
        const comments = commentsForFile.filter(c => c.lineNo === line.lineNo);
        return (
          <div key={idx}>
            <DiffLineRow
              line={line}
              onAddComment={handleAddComment}
              pendingLineNo={pendingLineNo}
            />
            {pendingLineNo === line.lineNo && line.lineNo != null && (
              <InlineCommentForm
                lineNo={line.lineNo}
                onSubmit={text => handleSubmitComment(line.lineNo as number, text)}
                onCancel={() => setPendingLineNo(null)}
              />
            )}
            {comments.map((c, ci) => (
              <div key={ci} style={{
                margin: '4px 8px',
                padding: '8px 10px',
                background: 'var(--surface-card, #1e1e2e)',
                border: '1px solid var(--color-primary, #6366f1)40',
                borderRadius: 4,
                fontSize: 12,
                color: 'var(--text-secondary)',
              }}>
                <span style={{ fontWeight: 600, color: 'var(--color-primary, #6366f1)', marginRight: 6 }}>You</span>
                {c.text}
              </div>
            ))}
          </div>
        );
      })}
    </div>
  );
}

// ── Main Component ────────────────────────────────────────────────────────────

export function PrReviewPage() {
  const [selectedPrId, setSelectedPrId] = useState<number>(MOCK_PRS[0].id);
  const [selectedFileIdx, setSelectedFileIdx] = useState(0);
  const [reviewKind, setReviewKind] = useState<ReviewKind>('comment');
  const [reviewText, setReviewText] = useState('');
  const [reviewPanelOpen, setReviewPanelOpen] = useState(true);
  const [inlineComments, setInlineComments] = useState<readonly InlineComment[]>([]);
  const [toast, setToast] = useState<string | null>(null);
  const [expandedFiles, setExpandedFiles] = useState<readonly boolean[]>([true]);

  const pr = MOCK_PRS.find(p => p.id === selectedPrId) ?? MOCK_PRS[0];

  const handleSelectPr = (id: number) => {
    setSelectedPrId(id);
    setSelectedFileIdx(0);
    setInlineComments([]);
    setExpandedFiles(Array(MOCK_PRS.find(p => p.id === id)?.files.length ?? 1).fill(true));
  };

  const handleAddComment = (fileIndex: number, lineNo: number, text: string) => {
    setInlineComments(prev => [...prev, { fileIndex, lineNo, text }]);
  };

  const handleSubmitReview = () => {
    const labels: Record<ReviewKind, string> = {
      comment: 'Comment submitted',
      approve: 'PR approved',
      'request-changes': 'Changes requested',
    };
    setToast(labels[reviewKind]);
    setReviewText('');
    setTimeout(() => setToast(null), 3000);
  };

  const toggleFile = (idx: number) => {
    setExpandedFiles(prev => prev.map((v, i) => (i === idx ? !v : v)));
  };

  return (
    <div style={{ display: 'flex', height: '100%', overflow: 'hidden', position: 'relative' }}>

      {/* Toast */}
      {toast && (
        <div style={{
          position: 'fixed',
          bottom: 24,
          left: '50%',
          transform: 'translateX(-50%)',
          background: '#22c55e',
          color: '#fff',
          padding: '10px 20px',
          borderRadius: 8,
          fontWeight: 600,
          fontSize: 13,
          zIndex: 1000,
          boxShadow: '0 4px 16px rgba(0,0,0,0.3)',
        }}>
          {toast}
        </div>
      )}

      {/* ── PR List Sidebar ──────────────────────────────────────────── */}
      <aside style={{
        width: SIDEBAR_WIDTH,
        minWidth: SIDEBAR_WIDTH,
        borderRight: '1px solid var(--surface-border, #2d2d3a)',
        display: 'flex',
        flexDirection: 'column',
        overflow: 'hidden',
      }}>
        <div style={{
          padding: '14px 14px 10px',
          borderBottom: '1px solid var(--surface-border, #2d2d3a)',
          fontWeight: 700,
          fontSize: 13,
          color: 'var(--text-primary)',
          letterSpacing: 0.2,
        }}>
          Linked Pull Requests
        </div>
        <div style={{ flex: 1, overflowY: 'auto' }}>
          {MOCK_PRS.map(p => (
            <PrListItem
              key={p.id}
              pr={p}
              selected={p.id === selectedPrId}
              onClick={() => handleSelectPr(p.id)}
            />
          ))}
        </div>
      </aside>

      {/* ── Diff Area ───────────────────────────────────────────────── */}
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column', overflow: 'hidden', minWidth: 0 }}>

        {/* PR Header */}
        <div style={{
          padding: '12px 16px',
          borderBottom: '1px solid var(--surface-border, #2d2d3a)',
          background: 'var(--surface-card, #1e1e2e)',
          flexShrink: 0,
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: 12 }}>
            <div>
              <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 4 }}>
                <span style={{ fontSize: 14, fontWeight: 700, color: 'var(--text-primary)' }}>
                  #{pr.id} {pr.title}
                </span>
                <span style={{
                  fontSize: 11,
                  padding: '2px 8px',
                  borderRadius: 20,
                  background: CI_STATUS_COLOR[pr.ciStatus] + '22',
                  color: CI_STATUS_COLOR[pr.ciStatus],
                  fontWeight: 600,
                  border: `1px solid ${CI_STATUS_COLOR[pr.ciStatus]}44`,
                }}>
                  {CI_STATUS_ICON[pr.ciStatus]} {CI_STATUS_LABEL[pr.ciStatus]}
                </span>
              </div>
              <div style={{ fontSize: 12, color: 'var(--text-muted, #6b7280)' }}>
                <span style={{ fontFamily: 'monospace', color: 'var(--text-secondary)' }}>{pr.branch}</span>
                {' → '}
                <span style={{ fontFamily: 'monospace', color: 'var(--text-secondary)' }}>main</span>
                {' · by '}
                <span style={{ fontWeight: 500, color: 'var(--text-secondary)' }}>{pr.author}</span>
              </div>
            </div>
            <div style={{ display: 'flex', gap: 8, alignItems: 'center', flexShrink: 0 }}>
              <button
                type="button"
                onClick={() => setReviewPanelOpen(v => !v)}
                style={ghostBtnStyle}
              >
                {reviewPanelOpen ? 'Hide Review ▶' : 'Review Changes ◀'}
              </button>
              <a
                href={`https://github.com/taskflow-ai/taskflow/pull/${pr.id}`}
                target="_blank"
                rel="noopener noreferrer"
                style={{
                  fontSize: 12,
                  padding: '4px 10px',
                  background: 'transparent',
                  border: '1px solid var(--surface-border, #2d2d3a)',
                  borderRadius: 4,
                  color: 'var(--color-primary, #6366f1)',
                  textDecoration: 'none',
                  whiteSpace: 'nowrap',
                }}
              >
                Open in GitHub ↗
              </a>
            </div>
          </div>

          {/* Stats bar */}
          <div style={{
            marginTop: 10,
            display: 'flex',
            gap: 16,
            fontSize: 12,
            color: 'var(--text-muted, #6b7280)',
          }}>
            <span style={{ color: '#4ade80', fontWeight: 600 }}>+{pr.additions} additions</span>
            <span style={{ color: '#f87171', fontWeight: 600 }}>-{pr.deletions} deletions</span>
            <span>{pr.files.length} files changed</span>
          </div>
        </div>

        <div style={{ flex: 1, display: 'flex', overflow: 'hidden' }}>

          {/* Scrollable diff content */}
          <div style={{ flex: 1, overflowY: 'auto', padding: 16, minWidth: 0 }}>

            {/* File list (collapsible) */}
            <div style={{
              marginBottom: 16,
              border: '1px solid var(--surface-border, #2d2d3a)',
              borderRadius: 'var(--radius-md, 6px)',
              overflow: 'hidden',
            }}>
              <div style={{
                padding: '8px 12px',
                background: 'var(--surface-card, #1e1e2e)',
                fontSize: 12,
                fontWeight: 600,
                color: 'var(--text-secondary)',
                borderBottom: '1px solid var(--surface-border, #2d2d3a)',
              }}>
                Files changed ({pr.files.length})
              </div>
              {pr.files.map((file, idx) => (
                <button
                  key={idx}
                  type="button"
                  onClick={() => { setSelectedFileIdx(idx); toggleFile(idx); }}
                  style={{
                    display: 'flex',
                    width: '100%',
                    textAlign: 'left',
                    padding: '6px 12px',
                    background: selectedFileIdx === idx ? 'var(--color-primary, #6366f1)10' : 'transparent',
                    border: 'none',
                    borderBottom: idx < pr.files.length - 1 ? '1px solid var(--surface-border, #2d2d3a)' : 'none',
                    cursor: 'pointer',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    gap: 8,
                  }}
                >
                  <span style={{ fontSize: 12, fontFamily: 'monospace', color: 'var(--text-primary)', flex: 1, textAlign: 'left' }}>
                    {expandedFiles[idx] ? '▾' : '▸'} {file.name}
                  </span>
                  <span style={{ fontSize: 11, color: '#4ade80', fontWeight: 600 }}>+{file.additions}</span>
                  <span style={{ fontSize: 11, color: '#f87171', fontWeight: 600 }}>-{file.deletions}</span>
                </button>
              ))}
            </div>

            {/* File tabs */}
            <div style={{ display: 'flex', gap: 4, marginBottom: 10, flexWrap: 'wrap' }}>
              {pr.files.map((file, idx) => (
                <button
                  key={idx}
                  type="button"
                  onClick={() => setSelectedFileIdx(idx)}
                  style={{
                    fontSize: 11,
                    padding: '3px 10px',
                    borderRadius: 4,
                    border: '1px solid var(--surface-border, #2d2d3a)',
                    background: selectedFileIdx === idx ? 'var(--color-primary, #6366f1)' : 'transparent',
                    color: selectedFileIdx === idx ? '#fff' : 'var(--text-secondary)',
                    cursor: 'pointer',
                    fontFamily: 'monospace',
                    maxWidth: 200,
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    whiteSpace: 'nowrap',
                  }}
                  title={file.name}
                >
                  {file.name.split('/').pop()}
                </button>
              ))}
            </div>

            {/* Diff viewer */}
            <DiffViewer
              file={pr.files[selectedFileIdx]}
              fileIndex={selectedFileIdx}
              inlineComments={inlineComments}
              onAddComment={handleAddComment}
            />
          </div>

          {/* ── Review Panel ──────────────────────────────────────── */}
          {reviewPanelOpen && (
            <aside style={{
              width: REVIEW_PANEL_WIDTH,
              minWidth: REVIEW_PANEL_WIDTH,
              borderLeft: '1px solid var(--surface-border, #2d2d3a)',
              overflowY: 'auto',
              padding: 16,
              display: 'flex',
              flexDirection: 'column',
              gap: 14,
              flexShrink: 0,
            }}>
              <div style={{ fontWeight: 700, fontSize: 13, color: 'var(--text-primary)' }}>
                Review changes
              </div>

              <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
                {(['comment', 'approve', 'request-changes'] as const).map(kind => {
                  const labels: Record<ReviewKind, string> = {
                    comment: 'Comment',
                    approve: 'Approve',
                    'request-changes': 'Request Changes',
                  };
                  const icons: Record<ReviewKind, string> = {
                    comment: '💬',
                    approve: '✅',
                    'request-changes': '🔄',
                  };
                  return (
                    <label key={kind} style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: 8,
                      fontSize: 13,
                      color: reviewKind === kind ? 'var(--text-primary)' : 'var(--text-secondary)',
                      cursor: 'pointer',
                      fontWeight: reviewKind === kind ? 600 : 400,
                    }}>
                      <input
                        type="radio"
                        name="reviewKind"
                        value={kind}
                        checked={reviewKind === kind}
                        onChange={() => setReviewKind(kind)}
                        style={{ accentColor: 'var(--color-primary, #6366f1)' }}
                      />
                      {icons[kind]} {labels[kind]}
                    </label>
                  );
                })}
              </div>

              <textarea
                value={reviewText}
                onChange={e => setReviewText(e.target.value)}
                rows={6}
                placeholder="Leave a review comment..."
                style={{
                  width: '100%',
                  boxSizing: 'border-box',
                  resize: 'vertical',
                  background: 'var(--surface-bg, #141420)',
                  border: '1px solid var(--surface-border, #2d2d3a)',
                  borderRadius: 'var(--radius-md, 6px)',
                  color: 'var(--text-primary)',
                  fontSize: 12,
                  padding: '8px 10px',
                  fontFamily: 'inherit',
                  outline: 'none',
                }}
              />

              <button
                type="button"
                onClick={handleSubmitReview}
                style={{
                  width: '100%',
                  padding: '8px',
                  background: 'var(--color-primary, #6366f1)',
                  border: 'none',
                  borderRadius: 'var(--radius-md, 6px)',
                  color: '#fff',
                  fontWeight: 600,
                  fontSize: 13,
                  cursor: 'pointer',
                  fontFamily: 'inherit',
                }}
              >
                Submit review
              </button>

              {inlineComments.length > 0 && (
                <div>
                  <div style={{ fontSize: 12, fontWeight: 600, color: 'var(--text-secondary)', marginBottom: 6 }}>
                    Pending comments ({inlineComments.length})
                  </div>
                  {inlineComments.map((c, i) => (
                    <div key={i} style={{
                      padding: '6px 8px',
                      background: 'var(--surface-card, #1e1e2e)',
                      border: '1px solid var(--surface-border, #2d2d3a)',
                      borderRadius: 4,
                      fontSize: 11,
                      color: 'var(--text-secondary)',
                      marginBottom: 4,
                    }}>
                      <span style={{ fontFamily: 'monospace', color: 'var(--text-muted)' }}>
                        line {c.lineNo}:{' '}
                      </span>
                      {c.text}
                    </div>
                  ))}
                </div>
              )}
            </aside>
          )}
        </div>
      </div>
    </div>
  );
}
