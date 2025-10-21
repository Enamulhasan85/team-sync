export interface User {
  id: string;
  email: string;
  name?: string;
  firstName?: string;
  lastName?: string;
  role?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken?: string;
  user?: User;
  expiresIn?: number;
}

export interface RegisterRequest {
  email: string;
  password: string;
  name?: string;
  firstName?: string;
  lastName?: string;
}

export interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  token: string | null;
}
