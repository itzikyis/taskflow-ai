export interface AuthToken {
  accessToken: string;
  userId: string;
  email: string;
  displayName: string;
}

export interface RegisterPayload {
  email: string;
  displayName: string;
  password: string;
}

export interface LoginPayload {
  email: string;
  password: string;
}
