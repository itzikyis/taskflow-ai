import { useState } from 'react';
import { useRegister, useLogin } from '../hooks/useAuth';

interface RegisterPageProps {
  onSwitchToLogin: () => void;
}

export function RegisterPage({ onSwitchToLogin }: RegisterPageProps) {
  const [email, setEmail] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [password, setPassword] = useState('');
  const [errorMsg, setErrorMsg] = useState('');

  const registerMutation = useRegister();
  const loginMutation = useLogin();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg('');

    registerMutation.mutate(
      { email: email.trim(), displayName: displayName.trim(), password },
      {
        onSuccess: () => {
          // Auto-login after successful registration
          loginMutation.mutate({ email: email.trim(), password });
        },
        onError: (err: unknown) => {
          const msg =
            err instanceof Error ? err.message : 'Registration failed. Please try again.';
          setErrorMsg(msg);
        },
      },
    );
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

  const isPending = registerMutation.isPending || loginMutation.isPending;

  return (
    <div style={{ maxWidth: 420, margin: '4rem auto', padding: '2rem', border: '1px solid #e5e5e5', borderRadius: 12 }}>
      <h2 style={{ marginTop: 0, marginBottom: '1.5rem' }}>Create account</h2>

      {errorMsg && (
        <p role="alert" style={{ color: 'red', marginBottom: '1rem' }}>{errorMsg}</p>
      )}

      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
        <div>
          <label htmlFor="reg-email" style={labelStyle}>Email</label>
          <input
            id="reg-email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            autoComplete="email"
            style={inputStyle}
          />
        </div>

        <div>
          <label htmlFor="reg-name" style={labelStyle}>Display name</label>
          <input
            id="reg-name"
            type="text"
            value={displayName}
            onChange={(e) => setDisplayName(e.target.value)}
            required
            autoComplete="name"
            style={inputStyle}
          />
        </div>

        <div>
          <label htmlFor="reg-password" style={labelStyle}>Password <span style={{ color: '#888', fontWeight: 400 }}>(min 8 characters)</span></label>
          <input
            id="reg-password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            minLength={8}
            autoComplete="new-password"
            style={inputStyle}
          />
        </div>

        <button
          type="submit"
          disabled={isPending}
          style={{ padding: '0.65rem', fontSize: '1rem', borderRadius: 6, border: 'none', background: '#0066cc', color: '#fff', cursor: 'pointer' }}
        >
          {isPending ? 'Creating account…' : 'Create account'}
        </button>
      </form>

      <p style={{ marginTop: '1.25rem', textAlign: 'center', fontSize: '0.875rem' }}>
        Already have an account?{' '}
        <button
          type="button"
          onClick={onSwitchToLogin}
          style={{ background: 'none', border: 'none', color: '#0066cc', cursor: 'pointer', padding: 0, fontSize: 'inherit' }}
        >
          Sign in
        </button>
      </p>
    </div>
  );
}
