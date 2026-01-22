export interface UserProfile {
  id: string;
  email: string;
  userName: string;
  name?: string;
  surname?: string;
  phoneNumber?: string;
  emailConfirmed: boolean;
  phoneNumberConfirmed: boolean;
  newsLanguageCode: string;
  newsLanguageName: string;
  twoFactorEnabled: boolean;
  creationTime: Date;
  lastModificationTime?: Date;
}

export interface UpdateUserProfile {
  email: string;
  userName: string;
  name?: string;
  surname?: string;
  phoneNumber?: string;
  newsLanguageCode: string;
}

export interface ChangePassword {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface NewsLanguage {
  code: string;
  name: string;
  isSupported: boolean;
}

export interface ProfileUpdateResult {
  success: boolean;
  message: string;
  requiresEmailConfirmation: boolean;
  profile?: UserProfile;
}
