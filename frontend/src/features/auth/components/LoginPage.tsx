import { useState } from 'react';
import { useLogin } from '../hooks/useAuth';

interface LoginPageProps {
  onSwitchToRegister: () => void;
}

export function LoginPage({ onSwitchToRegister }: LoginPageProps) {
  const [email, setEmail]       = useState('');
  const [password, setPassword] = useState('');
  const loginMutation = useLogin();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    loginMutation.mutate({ email: email.trim(), password });
  };

  return (
    <div className="auth-shell">
      <div className="auth-card">
        <div className="auth-logo">
          <div className="auth-logo-icon">⚡</div>
          <span className="auth-logo-text">TaskFlow AI</span>
        </div>

        <h1 className="auth-title">Welcome back</h1>
        <p className="auth-subtitle">Sign in to your workspace</p>

        {loginMutation.isError && (
          <p role="alert" className="form-error" style={{ marginBottom: 16 }}>
            Invalid email or password. Please try again.
          </p>
        )}

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-group">
            <label htmlFor="login-email" className="form-label">Email address</label>
            <input
              id="login-email"
              type="email"
              className="tf-input"
              value={email}
              onChange={e => setEmail(e.target.value)}
              placeholder="you@company.com"
              required
              autoComplete="email"
              autoFocus
            />
          </div>

          <div className="form-group">
            <label htmlFor="login-password" className="form-label">Password</label>
            <input
              id="login-password"
              type="password"
              className="tf-input"
              value={password}
              onChange={e => setPassword(e.target.value)}
              placeholder="••••••••"
              required
              autoComplete="current-password"
            />
          </div>

          <button
            type="submit"
            disabled={loginMutation.isPending}
            className="tf-btn tf-btn-primary"
            style={{ width: '100%', justifyContent: 'center', padding: '10px 16px', fontSize: 15, marginTop: 4 }}
          >
            {loginMutation.isPending ? 'Signing in…' : 'Sign in →'}
          </button>
        </form>

        <p className="auth-divider" style={{ marginTop: 20 }}>
          Don't have an account?{' '}
          <button type="button" className="auth-link" onClick={onSwitchToRegister}>
            Create one
          </button>
        </p>
      </div>
    </div>
  );
}
