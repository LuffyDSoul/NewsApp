// Interfaces para autenticación
export interface LoginRequest {
  userNameOrEmailAddress: string;
  password: string;
  rememberMe?: boolean;
}

export interface LoginResponse {
  access_token: string;
  expires_in: number;
  token_type: string;
  refresh_token?: string;
}

export interface RegisterRequest {
  userName: string;
  emailAddress: string;
  password: string;
  appName: string;
}

export interface UserProfile {
  id?: string;
  userName: string;
  email: string;
  name?: string;
  surname?: string;
  emailConfirmed: boolean;
  phoneNumber?: string;
}

export interface CurrentUser {
  isAuthenticated: boolean;
  id?: string;
  userName?: string;
  email?: string;
  name?: string;
  surname?: string;
  roles: string[];
}

export interface TokenInfo {
  access_token: string;
  expires_in: number;
  token_type: string;
  refresh_token?: string;
  expires_at: number;
}
