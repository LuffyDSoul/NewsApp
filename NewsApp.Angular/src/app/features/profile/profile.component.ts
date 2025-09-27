import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../core/services/auth.service';
import { LanguageService } from '../../core/services/language.service'; // ✅ NUEVO
import { UserProfile, UpdateProfileRequest, ChangePasswordRequest, CurrentUser, LanguageOption } from '../../shared/models/auth.model';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="profile-container">
      <div class="profile-wrapper">
        
        <!-- Header -->
        <div class="profile-header">
          <div class="avatar-section">
            <div class="avatar">
              {{ getInitials() }}
            </div>
            <div class="user-info">
              <h1>{{ currentUser.userName || 'User' }}</h1>
              <p class="user-email">{{ currentUser.email }}</p>
              <span class="user-status" [class.verified]="userProfile?.emailConfirmed">
                {{ userProfile?.emailConfirmed ? '✅ Email Verified' : '⚠️ Email Not Verified' }}
              </span>
            </div>
          </div>
          <button class="back-btn" (click)="goBack()">
            ← Back to News
          </button>
        </div>

        <!-- Tabs Navigation -->
        <div class="tabs-nav">
          <button 
            class="tab-btn" 
            [class.active]="activeTab === 'profile'"
            (click)="setActiveTab('profile')">
            📄 Profile Information
          </button>
          <button 
            class="tab-btn" 
            [class.active]="activeTab === 'password'"
            (click)="setActiveTab('password')">
            🔒 Change Password
          </button>
        </div>

        <!-- Profile Tab -->
        <div class="tab-content" *ngIf="activeTab === 'profile'">
          <div class="form-card">
            <h2>Personal Information</h2>
            <p class="form-description">Update your profile information below</p>

            <form (ngSubmit)="updateProfile()" #profileForm="ngForm">
              <div class="form-row">
                <div class="form-group">
                  <label for="userName">Username</label>
                  <input
                    type="text"
                    id="userName"
                    name="userName"
                    [(ngModel)]="profileData.userName"
                    required
                    minlength="3"
                    class="form-control"
                    [class.error]="profileSubmitted && (!profileData.userName || profileData.userName.length < 3)"
                    placeholder="Enter username">
                  <div class="error-message" *ngIf="profileSubmitted && !profileData.userName">
                    Username is required
                  </div>
                  <div class="error-message" *ngIf="profileSubmitted && profileData.userName && profileData.userName.length < 3">
                    Username must be at least 3 characters
                  </div>
                </div>

                <div class="form-group">
                  <label for="email">Email Address</label>
                  <input
                    type="email"
                    id="email"
                    name="email"
                    [(ngModel)]="profileData.email"
                    required
                    email
                    class="form-control"
                    [class.error]="profileSubmitted && (!profileData.email || !isValidEmail(profileData.email))"
                    placeholder="Enter email">
                  <div class="error-message" *ngIf="profileSubmitted && !profileData.email">
                    Email is required
                  </div>
                  <div class="error-message" *ngIf="profileSubmitted && profileData.email && !isValidEmail(profileData.email)">
                    Please enter a valid email address
                  </div>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label for="name">First Name</label>
                  <input
                    type="text"
                    id="name"
                    name="name"
                    [(ngModel)]="profileData.name"
                    class="form-control"
                    placeholder="Enter first name">
                </div>

                <div class="form-group">
                  <label for="surname">Last Name</label>
                  <input
                    type="text"
                    id="surname"
                    name="surname"
                    [(ngModel)]="profileData.surname"
                    class="form-control"
                    placeholder="Enter last name">
                </div>
              </div>

              <div class="form-group">
                <label for="phoneNumber">Phone Number</label>
                <input
                  type="tel"
                  id="phoneNumber"
                  name="phoneNumber"
                  [(ngModel)]="profileData.phoneNumber"
                  class="form-control"
                  placeholder="Enter phone number">
              </div>

              <!-- ✅ NUEVO: Campo de idioma preferido -->
              <div class="form-group">
                <label for="preferredLanguage">Preferred Language for News</label>
                <div class="language-selector">
                  <select
                    id="preferredLanguage"
                    name="preferredLanguage"
                    [(ngModel)]="profileData.preferredLanguage"
                    class="form-control language-select">
                    <option value="" disabled>Select your preferred language</option>
                    <option *ngFor="let lang of availableLanguages" [value]="lang.code">
                      {{ lang.flag }} {{ lang.name }}
                    </option>
                  </select>
                  <div class="language-preview" *ngIf="profileData.preferredLanguage">
                    <span class="current-language">
                      {{ getLanguageFlag(profileData.preferredLanguage) }}
                      {{ getLanguageName(profileData.preferredLanguage) }}
                    </span>
                    <small class="language-hint">
                      News will be displayed in this language when available
                    </small>
                  </div>
                </div>
              </div>

              <div class="form-actions">
                <button
                  type="submit"
                  class="save-btn"
                  [disabled]="profileLoading"
                  [class.loading]="profileLoading">
                  <span *ngIf="!profileLoading">💾 Save Changes</span>
                  <span *ngIf="profileLoading" class="loading-spinner">
                    <div class="spinner"></div>
                    Saving...
                  </span>
                </button>

                <button
                  type="button"
                  class="cancel-btn"
                  (click)="resetProfileForm()"
                  [disabled]="profileLoading">
                  🔄 Reset
                </button>
              </div>

              <div class="message success-message" *ngIf="profileSuccessMessage">
                {{ profileSuccessMessage }}
              </div>

              <div class="message error-message" *ngIf="profileErrorMessage">
                {{ profileErrorMessage }}
              </div>
            </form>
          </div>
        </div>

        <!-- Password Tab -->
        <div class="tab-content" *ngIf="activeTab === 'password'">
          <div class="form-card">
            <h2>Change Password</h2>
            <p class="form-description">Enter your current password and choose a new one</p>

            <form (ngSubmit)="changePassword()" #passwordForm="ngForm">
              <div class="form-group">
                <label for="currentPassword">Current Password</label>
                <input
                  type="password"
                  id="currentPassword"
                  name="currentPassword"
                  [(ngModel)]="passwordData.currentPassword"
                  required
                  class="form-control"
                  [class.error]="passwordSubmitted && !passwordData.currentPassword"
                  placeholder="Enter current password">
                <div class="error-message" *ngIf="passwordSubmitted && !passwordData.currentPassword">
                  Current password is required
                </div>
              </div>

              <div class="form-group">
                <label for="newPassword">New Password</label>
                <input
                  type="password"
                  id="newPassword"
                  name="newPassword"
                  [(ngModel)]="passwordData.newPassword"
                  required
                  minlength="6"
                  class="form-control"
                  [class.error]="passwordSubmitted && (!passwordData.newPassword || passwordData.newPassword.length < 6)"
                  placeholder="Enter new password">
                <div class="error-message" *ngIf="passwordSubmitted && !passwordData.newPassword">
                  New password is required
                </div>
                <div class="error-message" *ngIf="passwordSubmitted && passwordData.newPassword && passwordData.newPassword.length < 6">
                  Password must be at least 6 characters
                </div>
                <div class="password-hint">
                  Password should contain at least 6 characters
                </div>
              </div>

              <div class="form-group">
                <label for="confirmNewPassword">Confirm New Password</label>
                <input
                  type="password"
                  id="confirmNewPassword"
                  name="confirmNewPassword"
                  [(ngModel)]="confirmNewPassword"
                  required
                  class="form-control"
                  [class.error]="passwordSubmitted && (!confirmNewPassword || confirmNewPassword !== passwordData.newPassword)"
                  placeholder="Confirm new password">
                <div class="error-message" *ngIf="passwordSubmitted && !confirmNewPassword">
                  Please confirm your new password
                </div>
                <div class="error-message" *ngIf="passwordSubmitted && confirmNewPassword && confirmNewPassword !== passwordData.newPassword">
                  Passwords do not match
                </div>
              </div>

              <div class="form-actions">
                <button
                  type="submit"
                  class="save-btn"
                  [disabled]="passwordLoading"
                  [class.loading]="passwordLoading">
                  <span *ngIf="!passwordLoading">🔒 Change Password</span>
                  <span *ngIf="passwordLoading" class="loading-spinner">
                    <div class="spinner"></div>
                    Changing...
                  </span>
                </button>

                <button
                  type="button"
                  class="cancel-btn"
                  (click)="resetPasswordForm()"
                  [disabled]="passwordLoading">
                  🔄 Clear
                </button>
              </div>

              <div class="message success-message" *ngIf="passwordSuccessMessage">
                {{ passwordSuccessMessage }}
              </div>

              <div class="message error-message" *ngIf="passwordErrorMessage">
                {{ passwordErrorMessage }}
              </div>
            </form>
          </div>
        </div>

      </div>
    </div>
  `,
  styles: [`
    .profile-container {
      min-height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 20px;
    }

    .profile-wrapper {
      max-width: 900px;
      margin: 0 auto;
    }

    .profile-header {
      background: white;
      border-radius: 12px;
      padding: 30px;
      margin-bottom: 20px;
      box-shadow: 0 4px 15px rgba(0,0,0,0.1);
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .avatar-section {
      display: flex;
      align-items: center;
      gap: 20px;
    }

    .avatar {
      width: 80px;
      height: 80px;
      border-radius: 50%;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 2em;
      font-weight: bold;
      text-transform: uppercase;
    }

    .user-info h1 {
      margin: 0 0 5px 0;
      color: #333;
      font-size: 1.8em;
    }

    .user-email {
      margin: 0 0 10px 0;
      color: #666;
      font-size: 1.1em;
    }

    .user-status {
      padding: 4px 12px;
      border-radius: 20px;
      font-size: 0.9em;
      background: #f8d7da;
      color: #721c24;
    }

    .user-status.verified {
      background: #d4edda;
      color: #155724;
    }

    .back-btn {
      padding: 10px 20px;
      background: #6c757d;
      color: white;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      font-weight: 500;
      transition: background 0.2s ease;
    }

    .back-btn:hover {
      background: #545b62;
    }

    .tabs-nav {
      display: flex;
      gap: 0;
      margin-bottom: 20px;
    }

    .tab-btn {
      flex: 1;
      padding: 15px 20px;
      background: rgba(255,255,255,0.9);
      border: none;
      cursor: pointer;
      font-weight: 500;
      transition: all 0.2s ease;
      border-bottom: 3px solid transparent;
    }

    .tab-btn:first-child {
      border-radius: 12px 0 0 0;
    }

    .tab-btn:last-child {
      border-radius: 0 12px 0 0;
    }

    .tab-btn.active {
      background: white;
      border-bottom-color: #667eea;
      transform: translateY(-2px);
    }

    .tab-btn:hover:not(.active) {
      background: rgba(255,255,255,0.95);
    }

    .tab-content {
      animation: fadeIn 0.3s ease-out;
    }

    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(10px); }
      to { opacity: 1; transform: translateY(0); }
    }

    .form-card {
      background: white;
      border-radius: 0 0 12px 12px;
      padding: 40px;
      box-shadow: 0 4px 15px rgba(0,0,0,0.1);
    }

    .form-card h2 {
      margin: 0 0 10px 0;
      color: #333;
      font-size: 1.5em;
    }

    .form-description {
      margin: 0 0 30px 0;
      color: #666;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 20px;
    }

    .form-group {
      margin-bottom: 20px;
    }

    label {
      display: block;
      margin-bottom: 8px;
      color: #333;
      font-weight: 500;
    }

    .form-control {
      width: 100%;
      padding: 12px 15px;
      border: 2px solid #e9ecef;
      border-radius: 8px;
      font-size: 16px;
      transition: border-color 0.2s ease;
      box-sizing: border-box;
    }

    .form-control:focus {
      outline: none;
      border-color: #667eea;
    }

    .form-control.error {
      border-color: #dc3545;
    }

    .password-hint {
      font-size: 12px;
      color: #6c757d;
      margin-top: 5px;
    }

    .language-selector {
      position: relative;
    }

    .language-select {
      appearance: none;
      background: white url('data:image/svg+xml;charset=US-ASCII,<svg xmlns="http://www.w3.org/2000/svg" width="4" height="5" viewBox="0 0 4 5"><path fill="%23666" d="M2 0L0 2h4zm0 5L0 3h4z"/></svg>') no-repeat right 12px center;
      background-size: 12px;
      padding-right: 40px;
    }

    .language-preview {
      margin-top: 10px;
      padding: 10px 15px;
      background: #f8f9fa;
      border-radius: 6px;
      border-left: 4px solid #667eea;
    }

    .current-language {
      display: flex;
      align-items: center;
      gap: 8px;
      font-weight: 500;
      color: #333;
    }

    .language-hint {
      display: block;
      color: #6c757d;
      font-size: 12px;
      margin-top: 5px;
    }

    .form-actions {
      display: flex;
      gap: 15px;
      margin-top: 30px;
    }

    .save-btn, .cancel-btn {
      padding: 12px 24px;
      border: none;
      border-radius: 8px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s ease;
    }

    .save-btn {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      flex: 2;
    }

    .save-btn:hover:not(:disabled) {
      transform: translateY(-2px);
    }

    .save-btn:disabled {
      opacity: 0.7;
      cursor: not-allowed;
    }

    .cancel-btn {
      background: #6c757d;
      color: white;
      flex: 1;
    }

    .cancel-btn:hover:not(:disabled) {
      background: #545b62;
    }

    .loading-spinner {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 10px;
    }

    .spinner {
      width: 16px;
      height: 16px;
      border: 2px solid transparent;
      border-top: 2px solid white;
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }

    .message {
      padding: 12px 16px;
      border-radius: 8px;
      margin-top: 15px;
      font-weight: 500;
    }

    .success-message {
      background: #d4edda;
      color: #155724;
      border: 1px solid #c3e6cb;
    }

    .error-message {
      background: #f8d7da;
      color: #721c24;
      border: 1px solid #f5c6cb;
    }

    @media (max-width: 768px) {
      .profile-container {
        padding: 10px;
      }

      .profile-header {
        flex-direction: column;
        gap: 20px;
        text-align: center;
      }

      .form-card {
        padding: 20px;
      }

      .form-row {
        grid-template-columns: 1fr;
        gap: 10px;
      }

      .form-actions {
        flex-direction: column;
      }

      .tabs-nav {
        flex-direction: column;
      }

      .tab-btn:first-child {
        border-radius: 12px 12px 0 0;
      }

      .tab-btn:last-child {
        border-radius: 0 0 12px 12px;
      }
    }
  `]
})
export class ProfileComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  activeTab: 'profile' | 'password' = 'profile';
  
  currentUser: CurrentUser = { isAuthenticated: false, roles: [] };
  userProfile: UserProfile | null = null;
  availableLanguages: LanguageOption[] = []; // ✅ NUEVO: Lista de idiomas disponibles
  
  // Profile form data
  profileData: UpdateProfileRequest = {
    userName: '',
    email: '',
    name: '',
    surname: '',
    phoneNumber: '',
    preferredLanguage: 'en' // ✅ NUEVO: Idioma preferido por defecto
  };
  
  // Password form data
  passwordData: ChangePasswordRequest = {
    currentPassword: '',
    newPassword: ''
  };
  
  confirmNewPassword = '';
  
  // Form states
  profileSubmitted = false;
  passwordSubmitted = false;
  profileLoading = false;
  passwordLoading = false;
  
  // Messages
  profileSuccessMessage = '';
  profileErrorMessage = '';
  passwordSuccessMessage = '';
  passwordErrorMessage = '';
  
  constructor(
    private authService: AuthService,
    private languageService: LanguageService, // ✅ NUEVO: Servicio de idiomas
    private router: Router
  ) {}
  
  ngOnInit(): void {
    // Load available languages - ES UN ARRAY, NO UN OBSERVABLE
    this.availableLanguages = this.languageService.getAvailableLanguages();
    
    // Subscribe to current user changes
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe((user: CurrentUser) => {
        this.currentUser = user;
        if (!user.isAuthenticated) {
          this.router.navigate(['/auth/login']);
        }
      });
    
    // Load user profile data
    this.loadProfile();
  }
  
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
  
  loadProfile(): void {
    this.authService.getCurrentUser().subscribe({
      next: (profile: UserProfile) => {
        this.userProfile = profile;
        this.profileData = {
          userName: profile.userName,
          email: profile.email,
          name: profile.name || '',
          surname: profile.surname || '',
          phoneNumber: profile.phoneNumber || '',
          preferredLanguage: profile.preferredLanguage || 'en' // ✅ NUEVO: Cargar idioma preferido
        };
      },
      error: (error: any) => {
        console.error('Error loading profile:', error);
        this.profileErrorMessage = 'Failed to load profile data';
      }
    });
  }
  
  setActiveTab(tab: 'profile' | 'password'): void {
    this.activeTab = tab;
    // Clear messages when switching tabs
    this.clearMessages();
  }
  
  updateProfile(): void {
    this.profileSubmitted = true;
    this.clearMessages();
    
    if (!this.isProfileFormValid()) {
      return;
    }
    
    this.profileLoading = true;
    
    // ✅ NUEVO: Logging para debug
    console.log('🌍 Profile update data being sent:', this.profileData);
    console.log('🌍 PreferredLanguage value:', this.profileData.preferredLanguage);
    
    this.authService.updateProfile(this.profileData).subscribe({
      next: () => {
        this.profileLoading = false;
        this.profileSuccessMessage = '✅ Profile updated successfully!';
        this.profileSubmitted = false;
        
        // ✅ NUEVO: Logging después del éxito
        console.log('🌍 Profile updated successfully, refreshing user data...');
        
        // Actualizar idioma inmediatamente en la interfaz
        if (this.profileData.preferredLanguage) {
          this.authService.updatePreferredLanguage(this.profileData.preferredLanguage);
        }
        
        // Refresh user data
        this.authService.refreshProfile().subscribe({
          next: (refreshedProfile: UserProfile) => {
            console.log('🌍 Refreshed profile data:', refreshedProfile);
            console.log('🌍 New preferred language:', refreshedProfile.preferredLanguage);
          },
          error: (refreshError: any) => {
            console.error('🌍 Error refreshing profile:', refreshError);
          }
        });
        
        // Clear success message after 3 seconds
        setTimeout(() => {
          this.profileSuccessMessage = '';
        }, 3000);
      },
      error: (error: any) => {
        this.profileLoading = false;
        console.error('🌍 Profile update error:', error);
        console.error('🌍 Error details:', {
          status: error.status,
          message: error.message,
          error: error.error
        });
        
        if (error.status === 400) {
          this.profileErrorMessage = 'Invalid data. Please check your information.';
        } else if (error.status === 409) {
          this.profileErrorMessage = 'Username or email already exists.';
        } else {
          this.profileErrorMessage = 'Failed to update profile. Please try again.';
        }
      }
    });
  }
  
  changePassword(): void {
    this.passwordSubmitted = true;
    this.clearPasswordMessages();
    
    if (!this.isPasswordFormValid()) {
      return;
    }
    
    this.passwordLoading = true;
    
    this.authService.changePassword(this.passwordData).subscribe({
      next: () => {
        this.passwordLoading = false;
        this.passwordSuccessMessage = '✅ Password changed successfully!';
        this.resetPasswordForm();
        
        // Clear success message after 3 seconds
        setTimeout(() => {
          this.passwordSuccessMessage = '';
        }, 3000);
      },
      error: (error: any) => {
        this.passwordLoading = false;
        console.error('Change password error:', error);
        
        if (error.status === 400) {
          this.passwordErrorMessage = 'Current password is incorrect or new password is invalid.';
        } else {
          this.passwordErrorMessage = 'Failed to change password. Please try again.';
        }
      }
    });
  }
  
  resetProfileForm(): void {
    if (this.userProfile) {
      this.profileData = {
        userName: this.userProfile.userName,
        email: this.userProfile.email,
        name: this.userProfile.name || '',
        surname: this.userProfile.surname || '',
        phoneNumber: this.userProfile.phoneNumber || '',
        preferredLanguage: this.userProfile.preferredLanguage || 'en' // ✅ NUEVO: Reset idioma preferido
      };
    }
    this.profileSubmitted = false;
    this.clearMessages();
  }
  
  resetPasswordForm(): void {
    this.passwordData = {
      currentPassword: '',
      newPassword: ''
    };
    this.confirmNewPassword = '';
    this.passwordSubmitted = false;
    this.clearPasswordMessages();
  }
  
  private isProfileFormValid(): boolean {
    const isUsernameValid = !!(this.profileData.userName && this.profileData.userName.length >= 3);
    const isEmailValid = !!(this.profileData.email && this.isValidEmail(this.profileData.email));
    
    return isUsernameValid && isEmailValid;
  }
  
  private isPasswordFormValid(): boolean {
    const isCurrentPasswordValid = !!this.passwordData.currentPassword;
    const isNewPasswordValid = !!(this.passwordData.newPassword && this.passwordData.newPassword.length >= 6);
    const isConfirmPasswordValid = !!(this.confirmNewPassword && this.confirmNewPassword === this.passwordData.newPassword);
    
    return isCurrentPasswordValid && isNewPasswordValid && isConfirmPasswordValid;
  }
  
  isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }
  
  getInitials(): string {
    const name = this.currentUser.name || this.currentUser.userName || 'U';
    const surname = this.currentUser.surname || '';
    
    if (surname) {
      return (name.charAt(0) + surname.charAt(0)).toUpperCase();
    }
    
    return name.substring(0, 2).toUpperCase();
  }

  // ✅ NUEVOS: Métodos para idiomas
  getLanguageName(languageCode: string): string {
    return this.languageService.getLanguageName(languageCode);
  }

  getLanguageFlag(languageCode: string): string {
    return this.languageService.getLanguageFlag(languageCode);
  }
  
  private clearMessages(): void {
    this.profileSuccessMessage = '';
    this.profileErrorMessage = '';
  }
  
  private clearPasswordMessages(): void {
    this.passwordSuccessMessage = '';
    this.passwordErrorMessage = '';
  }
  
  goBack(): void {
    this.router.navigate(['/news']);
  }
}
