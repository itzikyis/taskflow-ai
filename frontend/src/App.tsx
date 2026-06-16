import { useState } from 'react';
import { TaskListPage } from '@/features/tasks/components/TaskListPage';
import { ProjectListPage } from '@/features/projects/components/ProjectListPage';
import { LoginPage } from '@/features/auth/components/LoginPage';
import { RegisterPage } from '@/features/auth/components/RegisterPage';
import { useAuthStore } from '@/store/authStore';
import { useLogout } from '@/features/auth/hooks/useAuth';

type Tab = 'tasks' | 'projects';
type AuthView = 'login' | 'register';

const tabStyle = (active: boolean): React.CSSProperties => ({
  padding: '0.5rem 1.25rem',
  cursor: 'pointer',
  border: 'none',
  borderBottom: active ? '2px solid #0066cc' : '2px solid transparent',
  background: 'none',
  fontWeight: active ? 700 : 400,
  color: active ? '#0066cc' : '#555',
  fontSize: '1rem',
});

export default function App() {
  const [tab, setTab] = useState<Tab>('tasks');
  const [authView, setAuthView] = useState<AuthView>('login');
  const { isAuthenticated, token } = useAuthStore();
  const logout = useLogout();

  // ── Unauthenticated ──────────────────────────────────────────────────────
  if (!isAuthenticated) {
    return (
      <main style={{ fontFamily: 'system-ui, sans-serif' }}>
        <div style={{ textAlign: 'center', paddingTop: '2rem' }}>
          <h1 style={{ color: '#0066cc' }}>TaskFlow AI</h1>
        </div>
        {authView === 'login'
          ? <LoginPage onSwitchToRegister={() => setAuthView('register')} />
          : <RegisterPage onSwitchToLogin={() => setAuthView('login')} />}
      </main>
    );
  }

  // ── Authenticated ────────────────────────────────────────────────────────
  return (
    <main style={{ fontFamily: 'system-ui, sans-serif', padding: '2rem', maxWidth: 800, margin: '0 auto' }}>
      <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem' }}>
        <h1 style={{ margin: 0 }}>TaskFlow AI</h1>
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', fontSize: '0.875rem' }}>
          <span style={{ color: '#555' }}>
            Hello, <strong>{token?.displayName}</strong>
          </span>
          <button
            type="button"
            onClick={logout}
            style={{ padding: '0.35rem 0.75rem', borderRadius: 6, border: '1px solid #ccc', background: 'none', cursor: 'pointer', fontSize: '0.875rem' }}
          >
            Sign out
          </button>
        </div>
      </header>

      <nav style={{ display: 'flex', gap: '0.5rem', borderBottom: '1px solid #eee', marginBottom: '1.5rem' }}>
        <button style={tabStyle(tab === 'tasks')} onClick={() => setTab('tasks')}>
          Tasks
        </button>
        <button style={tabStyle(tab === 'projects')} onClick={() => setTab('projects')}>
          Projects
        </button>
      </nav>

      {tab === 'tasks' && <TaskListPage />}
      {tab === 'projects' && <ProjectListPage />}
    </main>
  );
}
