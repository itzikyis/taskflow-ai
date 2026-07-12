import { useState } from 'react';
import { TaskListPage } from '@/features/tasks/components/TaskListPage';
import { ProjectListPage } from '@/features/projects/components/ProjectListPage';
import { TeamListPage } from '@/features/teams/components/TeamListPage';
import { LoginPage } from '@/features/auth/components/LoginPage';
import { RegisterPage } from '@/features/auth/components/RegisterPage';
import { useAuthStore } from '@/store/authStore';
import { useLogout } from '@/features/auth/hooks/useAuth';
import { NotificationBell } from '@/features/notifications/components/NotificationBell';
import { ActivityPage } from '@/features/activity/components/ActivityPage';
import { AuditPage } from '@/features/audit/components/AuditPage';
import { SprintPlannerPage } from '@/features/ai/components/SprintPlannerPage';
import { ReleaseNotesPage } from '@/features/ai/components/ReleaseNotesPage';
import { RetrospectivePage } from '@/features/ai/components/RetrospectivePage';
import { TimelinePage } from '@/features/timeline/components/TimelinePage';
import { DashboardPage } from '@/features/reporting/components/DashboardPage';

type View = 'tasks' | 'timeline' | 'dashboard' | 'projects' | 'teams' | 'activity' | 'audit' | 'sprint-planner' | 'release-notes' | 'retrospective';
type AuthView = 'login' | 'register';

const NAV_ITEMS: { id: View; icon: string; label: string }[] = [
  { id: 'tasks',    icon: '✓',  label: 'My Tasks'  },
  { id: 'timeline', icon: '📅', label: 'Timeline'  },
  { id: 'dashboard', icon: '📊', label: 'Dashboard' },
  { id: 'projects', icon: '⬡',  label: 'Projects'  },
  { id: 'teams',    icon: '👥', label: 'Teams'     },
  { id: 'activity', icon: '📋', label: 'Activity'  },
  { id: 'audit',          icon: '🛡️', label: 'Audit Trail'   },
  { id: 'sprint-planner', icon: '🗓️', label: 'Sprint Planner' },
  { id: 'release-notes', icon: '📝', label: 'Release Notes' },
  { id: 'retrospective', icon: '🔁', label: 'Retrospective' },
];

export default function App() {
  const [view, setView]         = useState<View>('tasks');
  const [authView, setAuthView] = useState<AuthView>('login');
  const { isAuthenticated, token } = useAuthStore();
  const logout = useLogout();

  if (!isAuthenticated) {
    return authView === 'login'
      ? <LoginPage    onSwitchToRegister={() => setAuthView('register')} />
      : <RegisterPage onSwitchToLogin={()    => setAuthView('login')}    />;
  }

  const initials = token?.displayName
    ? token.displayName.split(' ').map(w => w[0]).join('').slice(0, 2).toUpperCase()
    : '?';

  return (
    <div className="app-shell">
      {/* ── Sidebar ─────────────────────────────────────────────────── */}
      <aside className="app-sidebar">
        <div className="sidebar-logo">
          <div className="sidebar-logo-mark">
            <div className="sidebar-logo-icon">⚡</div>
            <span className="sidebar-logo-text">TaskFlow AI</span>
          </div>
        </div>

        <nav className="sidebar-nav" aria-label="Main navigation">
          <div className="sidebar-section-label">Workspace</div>
          {NAV_ITEMS.map(item => (
            <button
              key={item.id}
              type="button"
              className={`sidebar-nav-item${view === item.id ? ' active' : ''}`}
              onClick={() => setView(item.id)}
            >
              <span className="nav-icon">{item.icon}</span>
              {item.label}
            </button>
          ))}
        </nav>

        <div className="sidebar-footer">
          <div className="sidebar-user">
            <div className="sidebar-avatar">{initials}</div>
            <div className="sidebar-user-info">
              <div className="sidebar-user-name">{token?.displayName}</div>
              <div className="sidebar-user-email">{token?.email}</div>
            </div>
          </div>
          <button
            type="button"
            onClick={logout}
            className="sidebar-nav-item"
            style={{ marginTop: 8, paddingLeft: 0, paddingRight: 0, color: '#f87171' }}
          >
            <span className="nav-icon">↪</span>
            Sign out
          </button>
        </div>
      </aside>

      {/* ── Main ────────────────────────────────────────────────────── */}
      <div className="app-main">
        <header className="app-topbar">
          <span style={{ fontSize: 15, fontWeight: 600, color: 'var(--text-primary)' }}>
            {view === 'tasks' ? 'My Tasks' : view === 'timeline' ? 'Timeline' : view === 'dashboard' ? 'Dashboard' : view === 'projects' ? 'Projects' : view === 'teams' ? 'Teams' : view === 'activity' ? 'Activity Log' : view === 'audit' ? 'Audit Trail' : view === 'sprint-planner' ? 'Sprint Planner' : view === 'release-notes' ? 'Release Notes' : 'Retrospective'}
          </span>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <NotificationBell />
            <div className="sidebar-avatar" style={{ width: 28, height: 28, fontSize: 11 }}>
              {initials}
            </div>
            <span style={{ fontSize: 13, color: 'var(--text-secondary)', fontWeight: 500 }}>
              {token?.displayName}
            </span>
          </div>
        </header>

        <main className="app-content">
          {view === 'tasks'    && <TaskListPage />}
          {view === 'timeline' && <TimelinePage />}
          {view === 'dashboard' && <DashboardPage />}
          {view === 'projects' && <ProjectListPage />}
          {view === 'teams'    && <TeamListPage />}
          {view === 'activity' && <ActivityPage />}
          {view === 'audit'          && <AuditPage />}
          {view === 'sprint-planner' && <SprintPlannerPage />}
          {view === 'release-notes'  && <ReleaseNotesPage />}
          {view === 'retrospective'  && <RetrospectivePage />}
        </main>
      </div>
    </div>
  );
}
