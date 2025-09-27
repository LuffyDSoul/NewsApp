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

// ✅ ACTUALIZADO: Coincide exactamente con UserProfileDto del backend
export interface UserProfile {
  id?: string;
  userName: string;
  email: string;
  name?: string;
  surname?: string;
  emailConfirmed: boolean;
  phoneNumber?: string;
  preferredLanguage?: string; // ✅ Campo de idioma preferido
}

export interface CurrentUser {
  isAuthenticated: boolean;
  id?: string;
  userName?: string;
  email?: string;
  name?: string;
  surname?: string;
  roles: string[];
  preferredLanguage?: string; // ✅ Campo de idioma preferido
}

export interface TokenInfo {
  access_token: string;
  expires_in: number;
  token_type: string;
  refresh_token?: string;
  expires_at: number;
}

// ✅ ACTUALIZADO: Coincide exactamente con UpdateUserProfileDto del backend
export interface UpdateProfileRequest {
  userName: string;
  email: string;
  name?: string;
  surname?: string;
  phoneNumber?: string;
  preferredLanguage?: string; // ✅ Campo de idioma preferido
}

// ✅ ACTUALIZADO: Coincide exactamente con ChangePasswordDto del backend
export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface ProfileUpdateResponse {
  success: boolean;
  message?: string;
}

// ✅ NUEVO: Interfaz para idiomas disponibles
export interface LanguageOption {
  code: string;
  name: string;
  flag: string;
}
