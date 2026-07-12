import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import axios from 'axios';
import type { AuthToken } from '@/features/auth/types/auth.types';

interface AuthState {
  token: AuthToken | null;
  isAuthenticated: boolean;
  login: (token: AuthToken) => void;
  logout: () => void;
}

/** Returns true if the JWT is missing, malformed, or past its `exp` claim. */
function isTokenExpired(accessToken: string | undefined): boolean {
  if (!accessToken) return true;
  try {
    const payload = JSON.parse(atob(accessToken.split('.')[1]));
    if (typeof payload.exp !== 'number') return false;
    // Treat as expired 10s early to avoid races with in-flight requests.
    return payload.exp * 1000 <= Date.now() + 10_000;
  } catch {
    return true;
  }
}

function setAuthHeader(accessToken: string) {
  axios.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
}

function clearAuthHeader() {
  delete axios.defaults.headers.common['Authorization'];
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      isAuthenticated: false,

      login: (token) => {
        // Attach Bearer token to every outgoing Axios request
        setAuthHeader(token.accessToken);
        set({ token, isAuthenticated: true });
      },

      logout: () => {
        clearAuthHeader();
        set({ token: null, isAuthenticated: false });
      },
    }),
    {
      name: 'taskflow-auth',
      // Re-attach the auth header when the store is rehydrated from localStorage,
      // but only if the persisted token is still valid. An expired token is
      // cleared so the app shows the login screen instead of firing authenticated
      // requests that all fail with 401.
      onRehydrateStorage: () => (state) => {
        if (state?.token && !isTokenExpired(state.token.accessToken)) {
          setAuthHeader(state.token.accessToken);
        } else if (state) {
          clearAuthHeader();
          state.token = null;
          state.isAuthenticated = false;
        }
      },
    },
  ),
);

// Safety net: if any request comes back 401 (e.g. the token expired mid-session),
// clear the session so the user is returned to the login screen rather than being
// stuck on a broken page that keeps firing failing requests. Login/register calls
// are excluded so bad-credentials errors don't trigger a spurious logout loop.
axios.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error?.response?.status;
    const url: string = error?.config?.url ?? '';
    const isAuthCall = url.includes('/api/auth/');
    if (status === 401 && !isAuthCall && useAuthStore.getState().isAuthenticated) {
      useAuthStore.getState().logout();
    }
    return Promise.reject(error);
  },
);
