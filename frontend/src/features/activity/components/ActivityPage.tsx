import { useState } from 'react';
import { useAuthStore } from '@/store/authStore';
import { useRecentActivity, useActorActivity } from '../hooks/useActivity';
import { ActivityItem } from './ActivityItem';
import type { ActivityLog } from '../types/activity.types';

const ENTITY_TYPES = ['All', 'Task', 'Comment', 'Board', 'Project', 'Team'] as const;
type EntityTypeFilter = (typeof ENTITY_TYPES)[number];

const PAGE_SIZE = 50;

export function ActivityPage() {
  const { token } = useAuthStore();
  const userId = token?.userId ?? '';

  const [mode, setMode] = useState<'all' | 'mine'>('all');
  const [entityFilter, setEntityFilter] = useState<EntityTypeFilter>('All');
  const [page, setPage] = useState(1);

  const recentQuery = useRecentActivity(page);
  const actorQuery = useActorActivity(mode === 'mine' ? userId : '', page);

  const activeQuery = mode === 'all' ? recentQuery : actorQuery;
  const allLogs: ActivityLog[] = activeQuery.data ?? [];

  const filtered =
    entityFilter === 'All' ? allLogs : allLogs.filter(l => l.entityType === entityFilter);

  const isLoading = activeQuery.isLoading;
  const canLoadMore = allLogs.length === PAGE_SIZE;

  function handleModeChange(next: 'all' | 'mine') {
    setMode(next);
    setPage(1);
  }

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Activity Log</h1>
        <div style={{ display: 'flex', gap: 8 }}>
          <button
            type="button"
            className={`tf-btn tf-btn-sm ${mode === 'mine' ? 'tf-btn-primary' : 'tf-btn-ghost'}`}
            onClick={() => handleModeChange('mine')}
          >
            My activity
          </button>
          <button
            type="button"
            className={`tf-btn tf-btn-sm ${mode === 'all' ? 'tf-btn-primary' : 'tf-btn-ghost'}`}
            onClick={() => handleModeChange('all')}
          >
            All activity
          </button>
        </div>
      </div>

      {/* Filters */}
      <div style={{ display: 'flex', gap: 12, marginBottom: 20 }}>
        <select
          className="tf-input"
          style={{ width: 160 }}
          value={entityFilter}
          onChange={e => setEntityFilter(e.target.value as EntityTypeFilter)}
        >
          {ENTITY_TYPES.map(et => (
            <option key={et} value={et}>
              {et === 'All' ? 'All entities' : et}
            </option>
          ))}
        </select>
      </div>

      {/* Activity list */}
      <div className="tf-card">
        {isLoading ? (
          <>
            {[0, 1, 2, 3, 4].map(i => (
              <div
                key={i}
                className="skeleton"
                style={{ height: 36, marginBottom: 8, borderRadius: 'var(--radius-sm)' }}
              />
            ))}
          </>
        ) : filtered.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon">🕐</div>
            <div className="empty-state-text">No activity yet</div>
          </div>
        ) : (
          filtered.map(log => <ActivityItem key={log.id} log={log} />)
        )}
      </div>

      {/* Load more */}
      {canLoadMore && !isLoading && (
        <div style={{ marginTop: 16, textAlign: 'center' }}>
          <button
            type="button"
            className="tf-btn tf-btn-ghost"
            onClick={() => setPage(p => p + 1)}
          >
            Load more
          </button>
        </div>
      )}
    </div>
  );
}
