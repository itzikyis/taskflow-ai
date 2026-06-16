import axios from 'axios';
import type { AuthToken, LoginPayload, RegisterPayload } from '@/features/auth/types/auth.types';

const BASE = '/api/auth';

export const authService = {
  register: async (payload: RegisterPayload): Promise<string> => {
    const { data } = await axios.post<string>(`${BASE}/register`, payload);
    return data;
  },

  login: async (payload: LoginPayload): Promise<AuthToken> => {
    const { data } = await axios.post<AuthToken>(`${BASE}/login`, payload);
    return data;
  },
};
