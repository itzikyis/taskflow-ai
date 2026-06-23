import { useState } from 'react';
import { useRegister, useLogin } from '../hooks/useAuth';

interface RegisterPageProps {
  onSwitchToLogin: () => void;
}

export function RegisterPage({ onSwitchToLogin }: RegisterPageProps) {
  const [email, setEmail]             = useState('');
  const [displayName, setDisplayName] = useState('');
  const [password, setPassword]       = useState('');
  const [errorMsg, setErrorMsg]       = useState('');

  const registerMutation = useRegister();
  const loginMutation    = useLogin();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg('');
    registerMutation.mutate(
      { email: email.trim(), displayName: displayName.trim(), password },
      {
        onSuccess: () => { loginMutation.mutate({ email: email.trim(), password }); },
        onError: (err: unknown) => {
          setErrorMsg(err instanceof Error ? err.message : 'Registration failed. Please try again.');
        },
      },
    );
  };

  const isPending = registerMutation.isPending || loginMutation.isPending;

  return (
    <div className="auth-shell">
      <div className="auth-card">
        <div className="auth-logo">
          <div className="auth-logo-icon">⚡</div>
          <span className="auth-logo-text">TaskFlow AI</span>
        </div>

        <h1 className="auth-title">Create your account</h1>
        <p className="auth-subtitle">Start managing tasks smarter</p>

        {errorMsg && (
          <p role="alert" className="form-error" style={{ marginBottom: 16 }}>{errorMsg}</p>
        )}

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-group">
            <label htmlFor="reg-name" className="form-label">Full name</label>
            <input
              id="reg-name"
              type="text"
              className="tf-input"
              value={displayName}
              onChange={e => setDisplayName(e.target.value)}
              placeholder="Jane Smith"
              required
              autoComplete="name"
              autoFocus
            />
          </div>

          <div className="form-group">
            <label htmlFor="reg-email" className="form-label">Email address</label>
            <input
              id="reg-email"
              type="email"
              className="tf-input"
              value={email}
              onChange={e => setEmail(e.target.value)}
              placeholder="you@company.com"
              required
              autoComplete="email"
            />
          </div>

          <div className="form-group">
            <label htmlFor="reg-password" className="form-label">
              Password
              <span style={{ color: 'var(--text-muted)', fontWeight: 400, marginLeft: 6 }}>(min 8 characters)</span>
            </label>
            <input
              id="reg-password"
              type="password"
              className="tf-input"
              value={password}
              onChange={e => setPassword(e.target.value)}
              placeholder="••••••••"
              required
              minLength={8}
              autoComplete="new-password"
            />
          </div>

          <button
            type="submit"
            disabled={isPending}
            className="tf-btn tf-btn-primary"
            style={{ width: '100%', justifyContent: 'center', padding: '10px 16px', fontSize: 15, marginTop: 4 }}
          >
            {isPending ? 'Creating account…' : 'Create account →'}
          </button>
        </form>

        <p className="auth-divider" style={{ marginTop: 20 }}>
          Already have an account?{' '}
          <button type="button" className="auth-link" onClick={onSwitchToLogin}>
            Sign in
          </button>
        </p>
      </div>
    </div>
  );
}
