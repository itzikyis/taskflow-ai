import { useState } from 'react';
import { useLogin } from '../hooks/useAuth';

interface LoginPageProps {
  onSwitchToRegister: () => void;
}

export function LoginPage({ onSwitchToRegister }: LoginPageProps) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const loginMutation = useLogin();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    loginMutation.mutate({ email: email.trim(), password });
  };

  const inputStyle: React.CSSProperties = {
    display: 'block',
    width: '100%',
    padding: '0.6rem 0.75rem',
    fontSize: '1rem',
    border: '1px solid #ccc',
    borderRadius: 6,
    boxSizing: 'border-box',
  };

  const labelStyle: React.CSSProperties = {
    display: 'block',
    marginBottom: '0.25rem',
    fontWeight: 500,
    fontSize: '0.875rem',
  };

  return (
    <div style={{ maxWidth: 420, margin: '4rem auto', padding: '2rem', border: '1px solid #e5e5e5', borderRadius: 12 }}>
      <h2 style={{ marginTop: 0, marginBottom: '1.5rem' }}>Sign in</h2>

      {loginMutation.isError && (
        <p role="alert" style={{ color: 'red', marginBottom: '1rem' }}>
          Invalid email or password.
        </p>
      )}

      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
        <div>
          <label htmlFor="login-email" style={labelStyle}>Email</label>
          <input
            id="login-email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            autoComplete="email"
            style={inputStyle}
          />
        </div>

        <div>
          <label htmlFor="login-password" style={labelStyle}>Password</label>
          <input
            id="login-password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            autoComplete="current-password"
            style={inputStyle}
          />
        </div>

        <button
          type="submit"
          disabled={loginMutation.isPending}
          style={{ padding: '0.65rem', fontSize: '1rem', borderRadius: 6, border: 'none', background: '#0066cc', color: '#fff', cursor: 'pointer' }}
        >
          {loginMutation.isPending ? 'Signing in…' : 'Sign in'}
        </button>
      </form>

      <p style={{ marginTop: '1.25rem', textAlign: 'center', fontSize: '0.875rem' }}>
        Don't have an account?{' '}
        <button
          type="button"
          onClick={onSwitchToRegister}
          style={{ background: 'none', border: 'none', color: '#0066cc', cursor: 'pointer', padding: 0, fontSize: 'inherit' }}
        >
          Register
        </button>
      </p>
    </div>
  );
}
