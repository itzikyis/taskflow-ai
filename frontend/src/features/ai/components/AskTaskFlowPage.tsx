import { useState } from 'react';
import { search, type TaskSummaryDto } from '@/services/aiSearchService';

const EXAMPLE_QUERIES = [
  'Show me overdue high-priority tasks assigned to me',
  'What tasks are still in progress?',
  'Find critical tasks that are past due',
  'Show all to-do tasks with low priority',
  'What tasks are in review?',
];

const PRIORITY_COLORS: Record<string, string> = {
  Critical: '#dc2626',
  High: '#ea580c',
  Medium: '#ca8a04',
  Low: '#16a34a',
};

const STATUS_COLORS: Record<string, string> = {
  Todo: '#64748b',
  InProgress: '#2563eb',
  InReview: '#7c3aed',
  Done: '#16a34a',
};

function StatusBadge({ status }: { status: string }) {
  return (
    <span style={{
      padding: '2px 8px',
      borderRadius: 10,
      fontSize: 11,
      fontWeight: 600,
      background: `${STATUS_COLORS[status] ?? '#64748b'}1a`,
      color: STATUS_COLORS[status] ?? '#64748b',
      border: `1px solid ${STATUS_COLORS[status] ?? '#64748b'}40`,
    }}>
      {status === 'InProgress' ? 'In Progress' : status === 'InReview' ? 'In Review' : status}
    </span>
  );
}

function PriorityBadge({ priority }: { priority: string }) {
  return (
    <span style={{
      padding: '2px 8px',
      borderRadius: 10,
      fontSize: 11,
      fontWeight: 600,
      background: `${PRIORITY_COLORS[priority] ?? '#64748b'}1a`,
      color: PRIORITY_COLORS[priority] ?? '#64748b',
      border: `1px solid ${PRIORITY_COLORS[priority] ?? '#64748b'}40`,
    }}>
      {priority}
    </span>
  );
}

function TaskCard({ task }: { task: TaskSummaryDto }) {
  const isOverdue =
    task.dueDate !== null &&
    task.status !== 'Done' &&
    new Date(task.dueDate) < new Date();

  return (
    <div style={{
      padding: '14px 16px',
      background: 'var(--surface-card)',
      border: '1px solid var(--surface-border)',
      borderRadius: 'var(--radius-md, 8px)',
      display: 'flex',
      flexDirection: 'column',
      gap: 8,
    }}>
      <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', gap: 12 }}>
        <span style={{ fontWeight: 600, fontSize: 14, color: 'var(--text-primary)', flex: 1 }}>
          {task.title}
        </span>
        <div style={{ display: 'flex', gap: 6, flexShrink: 0 }}>
          <StatusBadge status={task.status} />
          <PriorityBadge priority={task.priority} />
        </div>
      </div>

      {task.description && (
        <p style={{
          margin: 0,
          fontSize: 13,
          color: 'var(--text-secondary, #64748b)',
          lineHeight: 1.5,
          display: '-webkit-box',
          WebkitLineClamp: 2,
          WebkitBoxOrient: 'vertical',
          overflow: 'hidden',
        }}>
          {task.description}
        </p>
      )}

      {task.dueDate && (
        <div style={{ fontSize: 12, color: isOverdue ? '#dc2626' : 'var(--text-secondary, #64748b)' }}>
          {isOverdue ? '⚠️ Overdue · ' : '📅 Due '}
          {new Date(task.dueDate).toLocaleDateString()}
        </div>
      )}
    </div>
  );
}

/** Natural-language task search powered by the Ask TaskFlow backend endpoint. */
export function AskTaskFlowPage() {
  const [query, setQuery] = useState('');
  const [loading, setLoading] = useState(false);
  const [interpretation, setInterpretation] = useState<string | null>(null);
  const [results, setResults] = useState<TaskSummaryDto[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleSearch = async (q: string) => {
    const trimmed = q.trim();
    if (!trimmed) return;
    setQuery(trimmed);
    setLoading(true);
    setError(null);
    setResults(null);
    setInterpretation(null);

    try {
      const data = await search(trimmed);
      setResults(data.tasks);
      setInterpretation(data.interpretation);
    } catch {
      setError('Search failed. Please check your connection and try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
      <div className="page-header">
        <div>
          <h1 className="page-title">Ask TaskFlow</h1>
          <p className="page-subtitle">
            Search tasks using plain English — describe what you are looking for and let TaskFlow find it.
          </p>
        </div>
      </div>

      {/* Search bar */}
      <div style={{ display: 'flex', gap: 8 }}>
        <input
          className="tf-input"
          style={{ flex: 1, fontSize: 14 }}
          placeholder='e.g. "show me overdue high-priority tasks assigned to me"'
          value={query}
          onChange={e => setQuery(e.target.value)}
          onKeyDown={e => { if (e.key === 'Enter') handleSearch(query); }}
          disabled={loading}
          aria-label="Natural language search query"
        />
        <button
          className="tf-btn tf-btn-primary"
          onClick={() => handleSearch(query)}
          disabled={loading || !query.trim()}
        >
          {loading ? '⏳ Searching…' : '🔍 Search'}
        </button>
      </div>

      {/* Example queries */}
      {results === null && !loading && (
        <div style={{
          padding: '24px 20px',
          background: 'var(--surface-card)',
          border: '1px solid var(--surface-border)',
          borderRadius: 'var(--radius-md, 8px)',
          textAlign: 'center',
        }}>
          <div style={{ fontSize: 32, marginBottom: 10 }}>🔍</div>
          <p style={{ margin: '0 0 6px', fontWeight: 600, fontSize: 14, color: 'var(--text-primary)' }}>
            Ask anything about your tasks
          </p>
          <p style={{ margin: '0 0 20px', fontSize: 13, color: 'var(--text-secondary, #64748b)' }}>
            Try one of these example queries or type your own:
          </p>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8, justifyContent: 'center' }}>
            {EXAMPLE_QUERIES.map(q => (
              <button
                key={q}
                className="tf-btn"
                style={{ fontSize: 12 }}
                onClick={() => handleSearch(q)}
              >
                {q}
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Error state */}
      {error && (
        <div style={{
          padding: '12px 16px',
          background: '#fef2f2',
          border: '1px solid #fecaca',
          borderRadius: 8,
          color: '#dc2626',
          fontSize: 13,
        }}>
          {error}
        </div>
      )}

      {/* Results */}
      {results !== null && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          {/* Interpretation banner */}
          {interpretation && (
            <div style={{
              padding: '10px 14px',
              background: 'var(--color-primary, #6366f1)1a',
              border: '1px solid var(--color-primary, #6366f1)40',
              borderRadius: 8,
              fontSize: 13,
              color: 'var(--text-primary)',
            }}>
              <strong>Interpreted as:</strong> {interpretation}
              <span style={{ marginLeft: 8, color: 'var(--text-secondary, #64748b)', fontSize: 12 }}>
                · {results.length} task{results.length !== 1 ? 's' : ''} found
              </span>
            </div>
          )}

          {results.length === 0 ? (
            <div style={{
              padding: '32px 20px',
              textAlign: 'center',
              background: 'var(--surface-card)',
              border: '1px solid var(--surface-border)',
              borderRadius: 8,
              color: 'var(--text-secondary, #64748b)',
              fontSize: 14,
            }}>
              No tasks matched your query. Try rephrasing or broadening your search.
            </div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
              {results.map(task => (
                <TaskCard key={task.id} task={task} />
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
