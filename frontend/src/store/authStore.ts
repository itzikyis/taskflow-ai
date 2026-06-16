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

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      isAuthenticated: false,

      login: (token) => {
        // Attach Bearer token to every outgoing Axios request
        axios.defaults.headers.common['Authorization'] = `Bearer ${token.accessToken}`;
        set({ token, isAuthenticated: true });
      },

      logout: () => {
        delete axios.defaults.headers.common['Authorization'];
        set({ token: null, isAuthenticated: false });
      },
    }),
    {
      name: 'taskflow-auth',
      // Re-attach the auth header when the store is rehydrated from localStorage
      onRehydrateStorage: () => (state) => {
        if (state?.token) {
          axios.defaults.headers.common['Authorization'] =
            `Bearer ${state.token.accessToken}`;
        }
      },
    },
  ),
);
