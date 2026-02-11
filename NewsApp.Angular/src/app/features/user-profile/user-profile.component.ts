import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { UserProfileService } from '../../core/services/user-profile.service';
import { NotificationService } from '../../core/services/notification.service';
import { 
  UserProfile, 
  UpdateUserProfile, 
  ChangePassword, 
  NewsLanguage,
  ProfileUpdateResult
} from '../../shared/models/user-profile.model';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="profile-container">
      <div class="profile-header">
        <h2>Mi Perfil</h2>
        <p class="profile-subtitle">Gestiona tu información personal y preferencias</p>
      </div>

      <div class="profile-content">
        <!-- Pestañas -->
        <div class="tabs">
          <button 
            class="tab" 
            [class.active]="activeTab === 'profile'"
            (click)="activeTab = 'profile'">
            Información Personal
          </button>
          <button 
            class="tab" 
            [class.active]="activeTab === 'password'"
            (click)="activeTab = 'password'">
            Cambiar Contraseña
          </button>
          <button 
            class="tab" 
            [class.active]="activeTab === 'preferences'"
            (click)="activeTab = 'preferences'">
            Preferencias
          </button>
          <button 
            class="tab" 
            [class.active]="activeTab === 'notifications'"
            (click)="activeTab = 'notifications'">
            Notificaciones
          </button>
        </div>

        <!-- Contenido de las pestañas -->
        <div class="tab-content">
          <!-- Información Personal -->
          <div *ngIf="activeTab === 'profile'" class="tab-panel">
            <form [formGroup]="profileForm" (ngSubmit)="updateProfile()">
              <div class="form-row">
                <div class="form-group">
                  <label for="userName">Nombre de Usuario *</label>
                  <div class="input-with-edit">
                    <input 
                      type="text" 
                      id="userName" 
                      formControlName="userName"
                      class="form-control"
                      [readonly]="!editMode.userName"
                      [class.error]="profileForm.get('userName')?.invalid && profileForm.get('userName')?.touched">
                    <button 
                      type="button" 
                      class="edit-btn" 
                      (click)="toggleEditMode('userName')"
                      [title]="editMode.userName ? 'Bloquear' : 'Editar'">
                      {{ editMode.userName ? '🔒' : '✏️' }}
                    </button>
                  </div>
                  <div class="error-message" *ngIf="profileForm.get('userName')?.invalid && profileForm.get('userName')?.touched">
                    El nombre de usuario es requerido
                  </div>
                </div>

                <div class="form-group">
                  <label for="email">Correo Electrónico *</label>
                  <div class="input-with-edit">
                    <input 
                      type="email" 
                      id="email" 
                      formControlName="email"
                      class="form-control"
                      [readonly]="!editMode.email"
                      [class.error]="profileForm.get('email')?.invalid && profileForm.get('email')?.touched">
                    <button 
                      type="button" 
                      class="edit-btn" 
                      (click)="toggleEditMode('email')"
                      [title]="editMode.email ? 'Bloquear' : 'Editar'">
                      {{ editMode.email ? '🔒' : '✏️' }}
                    </button>
                  </div>
                  <div class="email-status" *ngIf="userProfile?.emailConfirmed; else unconfirmedEmail">
                    <span class="status-confirmed">✓ Correo confirmado</span>
                  </div>
                  <ng-template #unconfirmedEmail>
                    <div class="email-status">
                      <span class="status-unconfirmed">⚠ Correo no confirmado</span>
                      <button 
                        type="button" 
                        class="confirm-email-btn" 
                        (click)="sendEmailConfirmation()"
                        [disabled]="hasChanges()"
                        [title]="hasChanges() ? 'Guarda los cambios primero' : 'Enviar confirmación'">
                        Enviar confirmación
                      </button>
                      <small class="help-text" *ngIf="hasChanges()" style="color: #ff6b6b; font-size: 0.85em; margin-top: 0.25rem; display: block;">
                        💡 Guarda los cambios antes de enviar la confirmación
                      </small>
                    </div>
                  </ng-template>
                  <div class="error-message" *ngIf="profileForm.get('email')?.invalid && profileForm.get('email')?.touched">
                    Ingresa un correo electrónico válido
                  </div>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label for="name">Nombre</label>
                  <div class="input-with-edit">
                    <input 
                      type="text" 
                      id="name" 
                      formControlName="name"
                      class="form-control"
                      [readonly]="!editMode.name">
                    <button 
                      type="button" 
                      class="edit-btn" 
                      (click)="toggleEditMode('name')"
                      [title]="editMode.name ? 'Bloquear' : 'Editar'">
                      {{ editMode.name ? '🔒' : '✏️' }}
                    </button>
                  </div>
                </div>

                <div class="form-group">
                  <label for="surname">Apellido</label>
                  <div class="input-with-edit">
                    <input 
                      type="text" 
                      id="surname" 
                      formControlName="surname"
                      class="form-control"
                      [readonly]="!editMode.surname">
                    <button 
                      type="button" 
                      class="edit-btn" 
                      (click)="toggleEditMode('surname')"
                      [title]="editMode.surname ? 'Bloquear' : 'Editar'">
                      {{ editMode.surname ? '🔒' : '✏️' }}
                    </button>
                  </div>
                </div>
              </div>

              <div class="form-group">
                <label for="phoneNumber">Número de Teléfono</label>
                <div class="input-with-edit">
                  <input 
                    type="tel" 
                    id="phoneNumber" 
                    formControlName="phoneNumber"
                    class="form-control"
                    [readonly]="!editMode.phoneNumber">
                  <button 
                    type="button" 
                    class="edit-btn" 
                    (click)="toggleEditMode('phoneNumber')"
                    [title]="editMode.phoneNumber ? 'Bloquear' : 'Editar'">
                    {{ editMode.phoneNumber ? '🔒' : '✏️' }}
                  </button>
                </div>
              </div>

              <div class="form-actions">
                <button type="submit" class="btn btn-primary" [disabled]="profileForm.invalid || isLoading || !hasChanges()">
                  <span *ngIf="isLoading">Guardando...</span>
                  <span *ngIf="!isLoading">Guardar Cambios</span>
                </button>
                <button type="button" class="btn btn-secondary" (click)="cancelChanges()" [disabled]="isLoading || !hasChanges()">
                  Cancelar
                </button>
              </div>
            </form>
          </div>

          <!-- Cambiar Contraseña -->
          <div *ngIf="activeTab === 'password'" class="tab-panel">
            <form [formGroup]="passwordForm" (ngSubmit)="changePassword()">
              <div class="form-group">
                <label for="currentPassword">Contraseña Actual *</label>
                <input 
                  type="password" 
                  id="currentPassword" 
                  formControlName="currentPassword"
                  class="form-control"
                  [class.error]="passwordForm.get('currentPassword')?.invalid && passwordForm.get('currentPassword')?.touched">
                <div class="error-message" *ngIf="passwordForm.get('currentPassword')?.invalid && passwordForm.get('currentPassword')?.touched">
                  La contraseña actual es requerida
                </div>
              </div>

              <div class="form-group">
                <label for="newPassword">Nueva Contraseña *</label>
                <input 
                  type="password" 
                  id="newPassword" 
                  formControlName="newPassword"
                  class="form-control"
                  [class.error]="passwordForm.get('newPassword')?.invalid && passwordForm.get('newPassword')?.touched">
                <div class="error-message" *ngIf="passwordForm.get('newPassword')?.invalid && passwordForm.get('newPassword')?.touched">
                  La nueva contraseña debe tener al menos 6 caracteres
                </div>
              </div>

              <div class="form-group">
                <label for="confirmNewPassword">Confirmar Nueva Contraseña *</label>
                <input 
                  type="password" 
                  id="confirmNewPassword" 
                  formControlName="confirmNewPassword"
                  class="form-control"
                  [class.error]="passwordForm.get('confirmNewPassword')?.invalid && passwordForm.get('confirmNewPassword')?.touched">
                <div class="error-message" *ngIf="passwordForm.get('confirmNewPassword')?.invalid && passwordForm.get('confirmNewPassword')?.touched">
                  <span *ngIf="passwordForm.get('confirmNewPassword')?.hasError('required')">
                    Confirma tu nueva contraseña
                  </span>
                  <span *ngIf="passwordForm.get('confirmNewPassword')?.hasError('mismatch')">
                    Las contraseñas no coinciden
                  </span>
                </div>
              </div>

              <div class="form-actions">
                <button type="submit" class="btn btn-primary" [disabled]="passwordForm.invalid || isLoading">
                  <span *ngIf="isLoading">Cambiando...</span>
                  <span *ngIf="!isLoading">Cambiar Contraseña</span>
                </button>
              </div>
            </form>
          </div>

          <!-- Preferencias -->
          <div *ngIf="activeTab === 'preferences'" class="tab-panel">
            <form [formGroup]="preferencesForm" (ngSubmit)="updatePreferences()">
              <div class="form-group">
                <label for="newsLanguageCode">Idioma de las Noticias</label>
                <select 
                  id="newsLanguageCode" 
                  formControlName="newsLanguageCode"
                  class="form-control">
                  <option *ngFor="let language of availableLanguages" [value]="language.code">
                    {{ language.name }}
                  </option>
                </select>
                <small class="form-help">
                  Selecciona el idioma en el que prefieres ver las noticias
                </small>
              </div>

              <div class="form-actions">
                <button type="submit" class="btn btn-primary" [disabled]="preferencesForm.invalid || isLoading">
                  <span *ngIf="isLoading">Guardando...</span>
                  <span *ngIf="!isLoading">Guardar Preferencias</span>
                </button>
              </div>
            </form>
          </div>

          <!-- Notificaciones -->
          <div *ngIf="activeTab === 'notifications'" class="tab-panel">
            <h3>Notificaciones por Email</h3>
            
            <div class="notification-status">
              <div *ngIf="userProfile?.emailConfirmed; else emailNotConfirmed" class="status-card confirmed">
                <div class="status-icon">✓</div>
                <div class="status-content">
                  <h4>Email Confirmado</h4>
                  <p>Tu email <strong>{{ userProfile?.email }}</strong> está verificado y listo para recibir notificaciones.</p>
                </div>
              </div>
              <ng-template #emailNotConfirmed>
                <div class="status-card not-confirmed">
                  <div class="status-icon">⚠</div>
                  <div class="status-content">
                    <h4>Email No Confirmado</h4>
                    <p>Debes confirmar tu email antes de recibir notificaciones. Ve a la pestaña "Información Personal" y haz clic en "Enviar confirmación".</p>
                  </div>
                </div>
              </ng-template>
            </div>

            <div class="notification-test-section">
              <h4>Probar Notificaciones</h4>
              <p>Envía un correo de prueba para verificar que las notificaciones funcionan correctamente.</p>
              
              <button 
                class="btn btn-primary" 
                (click)="sendTestNotification()"
                [disabled]="isLoading || !userProfile?.emailConfirmed">
                <span *ngIf="isLoading">Enviando...</span>
                <span *ngIf="!isLoading">📧 Enviar Email de Prueba</span>
              </button>

              <small class="help-text" *ngIf="!userProfile?.emailConfirmed" style="color: #ff6b6b; display: block; margin-top: 0.5rem;">
                💡 Debes confirmar tu email antes de poder recibir notificaciones
              </small>
            </div>

            <div class="notification-info">
              <h4>¿Cuándo recibiré notificaciones?</h4>
              <ul>
                <li><strong>Alertas de Noticias:</strong> Cuando tus alertas configuradas encuentren nuevas noticias que coincidan con tus criterios</li>
                <li><strong>Resumen Diario:</strong> Un resumen de las noticias más importantes del día (si está habilitado)</li>
              </ul>
            </div>
          </div>
        </div>
      </div>

      <!-- Mensajes de éxito/error -->
      <div class="alert alert-success" *ngIf="successMessage">
        {{ successMessage }}
      </div>
      <div class="alert alert-error" *ngIf="errorMessage">
        {{ errorMessage }}
      </div>
    </div>
  `,
  styles: [`
    .profile-container {
      max-width: 800px;
      margin: 0 auto;
      padding: 2rem;
    }

    .profile-header {
      text-align: center;
      margin-bottom: 2rem;
    }

    .profile-header h2 {
      color: #333;
      margin-bottom: 0.5rem;
    }

    .profile-subtitle {
      color: #666;
      margin: 0;
    }

    .profile-content {
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
      overflow: hidden;
    }

    .tabs {
      display: flex;
      border-bottom: 1px solid #e9ecef;
    }

    .tab {
      flex: 1;
      padding: 1rem;
      background: #f8f9fa;
      border: none;
      cursor: pointer;
      font-weight: 500;
      transition: all 0.3s ease;
      border-bottom: 3px solid transparent;
    }

    .tab:hover {
      background: #e9ecef;
    }

    .tab.active {
      background: white;
      border-bottom-color: #667eea;
      color: #667eea;
    }

    .tab-content {
      padding: 2rem;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
      margin-bottom: 1rem;
    }

    .form-group {
      margin-bottom: 1.5rem;
    }

    .form-group label {
      display: block;
      margin-bottom: 0.5rem;
      font-weight: 500;
      color: #333;
    }

    .form-control {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 1rem;
      transition: border-color 0.3s ease;
    }

    .form-control:focus {
      outline: none;
      border-color: #667eea;
      box-shadow: 0 0 0 2px rgba(102, 126, 234, 0.2);
    }

    .form-control:read-only {
      background-color: #f8f9fa;
      cursor: not-allowed;
      color: #6c757d;
    }

    .form-control.error {
      border-color: #dc3545;
    }

    .input-with-edit {
      display: flex;
      gap: 0.5rem;
      align-items: center;
    }

    .input-with-edit input {
      flex: 1;
    }

    .edit-btn {
      background: #667eea;
      color: white;
      border: none;
      padding: 0.75rem 1rem;
      border-radius: 4px;
      cursor: pointer;
      font-size: 1.2rem;
      transition: all 0.3s ease;
      min-width: 50px;
      height: 48px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .edit-btn:hover {
      background: #5568d3;
      transform: translateY(-2px);
      box-shadow: 0 4px 8px rgba(102, 126, 234, 0.3);
    }

    .edit-btn:active {
      transform: translateY(0);
    }

    .error-message {
      color: #dc3545;
      font-size: 0.875rem;
      margin-top: 0.25rem;
    }

    .form-help {
      color: #666;
      font-size: 0.875rem;
      margin-top: 0.25rem;
      display: block;
    }

    .email-status {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin-top: 0.25rem;
    }

    .status-confirmed {
      color: #28a745;
      font-size: 0.875rem;
    }

    .status-unconfirmed {
      color: #ffc107;
      font-size: 0.875rem;
    }

    .confirm-email-btn {
      background: #ffc107;
      color: #333;
      border: none;
      padding: 0.25rem 0.5rem;
      border-radius: 4px;
      font-size: 0.75rem;
      cursor: pointer;
      transition: background-color 0.3s ease;
    }

    .confirm-email-btn:hover {
      background: #e0a800;
    }

    .form-actions {
      margin-top: 2rem;
      text-align: right;
    }

    .btn {
      padding: 0.75rem 1.5rem;
      border: none;
      border-radius: 4px;
      font-size: 1rem;
      font-weight: 500;
      cursor: pointer;
      transition: all 0.3s ease;
    }

    .btn-primary {
      background: #667eea;
      color: white;
    }

    .btn-primary:hover:not(:disabled) {
      background: #5a6fd8;
    }

    .btn-secondary {
      background: #6c757d;
      color: white;
      margin-left: 0.5rem;
    }

    .btn-secondary:hover:not(:disabled) {
      background: #5a6268;
    }

    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .alert {
      margin-top: 1rem;
      padding: 0.75rem 1rem;
      border-radius: 4px;
      font-size: 0.875rem;
    }

    .alert-success {
      background: #d4edda;
      color: #155724;
      border: 1px solid #c3e6cb;
    }

    .alert-error {
      background: #f8d7da;
      color: #721c24;
      border: 1px solid #f5c6cb;
    }

    .notification-status {
      margin-bottom: 2rem;
    }

    .status-card {
      display: flex;
      align-items: center;
      padding: 1.5rem;
      border-radius: 8px;
      margin-bottom: 1.5rem;
    }

    .status-card.confirmed {
      background: #d4edda;
      border: 1px solid #c3e6cb;
    }

    .status-card.not-confirmed {
      background: #fff3cd;
      border: 1px solid #ffeaa7;
    }

    .status-icon {
      font-size: 3rem;
      margin-right: 1.5rem;
    }

    .status-content h4 {
      margin: 0 0 0.5rem 0;
      color: #333;
    }

    .status-content p {
      margin: 0;
      color: #666;
    }

    .notification-test-section {
      background: #f8f9fa;
      padding: 1.5rem;
      border-radius: 8px;
      margin-bottom: 1.5rem;
    }

    .notification-test-section h4 {
      margin-top: 0;
    }

    .notification-info {
      background: #e7f3ff;
      padding: 1.5rem;
      border-radius: 8px;
      border-left: 4px solid #667eea;
    }

    .notification-info h4 {
      margin-top: 0;
      color: #333;
    }

    .notification-info ul {
      margin: 0.5rem 0 0 1.5rem;
      padding: 0;
    }

    .notification-info li {
      margin-bottom: 0.5rem;
      color: #666;
    }

    @media (max-width: 768px) {
      .profile-container {
        padding: 1rem;
      }

      .form-row {
        grid-template-columns: 1fr;
      }

      .tabs {
        flex-direction: column;
      }

      .tab-content {
        padding: 1rem;
      }
    }
  `]
})
export class UserProfileComponent implements OnInit, OnDestroy {
activeTab: 'profile' | 'password' | 'preferences' | 'notifications' = 'profile';
userProfile: UserProfile | null = null;
availableLanguages: NewsLanguage[] = [];
isLoading = false;
successMessage = '';
errorMessage = '';

  // Edit mode tracking
  editMode = {
    userName: false,
    email: false,
    name: false,
    surname: false,
    phoneNumber: false
  };

  // Store original values for cancel functionality
  originalValues: any = {};

  profileForm: FormGroup;
  passwordForm: FormGroup;
  preferencesForm: FormGroup;

  private destroy$ = new Subject<void>();

  private fb = inject(FormBuilder);
  private userProfileService = inject(UserProfileService);
  private notificationService = inject(NotificationService);

  constructor() {
    this.profileForm = this.createProfileForm();
    this.passwordForm = this.createPasswordForm();
    this.preferencesForm = this.createPreferencesForm();
  }

  ngOnInit(): void {
    console.log('UserProfileComponent initialized');
    this.loadProfile();
    this.loadAvailableLanguages();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createProfileForm(): FormGroup {
    return this.fb.group({
      userName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      name: [''],
      surname: [''],
      phoneNumber: ['']
    });
  }

  private createPasswordForm(): FormGroup {
    return this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmNewPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  private createPreferencesForm(): FormGroup {
    return this.fb.group({
      newsLanguageCode: ['en', [Validators.required]]
    });
  }

  private passwordMatchValidator(group: FormGroup) {
    const newPassword = group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmNewPassword')?.value;
    
    if (newPassword !== confirmPassword) {
      group.get('confirmNewPassword')?.setErrors({ mismatch: true });
    } else {
      const confirmPasswordControl = group.get('confirmNewPassword');
      if (confirmPasswordControl?.hasError('mismatch')) {
        confirmPasswordControl.setErrors(null);
      }
    }
    return null;
  }

  private loadProfile(): void {
    console.log('Loading user profile...');
    this.userProfileService.getMyProfile()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (profile: UserProfile) => {
          console.log('Profile loaded successfully:', profile);
          this.userProfile = profile;
          const profileData = {
            userName: profile.userName,
            email: profile.email,
            name: profile.name,
            surname: profile.surname,
            phoneNumber: profile.phoneNumber
          };
          console.log('Profile data to patch:', profileData);
          this.profileForm.patchValue(profileData);
          this.originalValues = { ...profileData };
          this.preferencesForm.patchValue({
            newsLanguageCode: profile.newsLanguageCode
          });
        },
        error: (error: any) => {
          console.error('Error loading profile:', error);
          this.showError('Error al cargar el perfil');
        }
      });
  }

  toggleEditMode(field: string): void {
    this.editMode[field as keyof typeof this.editMode] = !this.editMode[field as keyof typeof this.editMode];
  }

  hasChanges(): boolean {
    if (!this.originalValues) return false;
    const currentValues = this.profileForm.value;
    return Object.keys(this.originalValues).some(
      key => this.originalValues[key] !== currentValues[key]
    );
  }

  cancelChanges(): void {
    this.profileForm.patchValue(this.originalValues);
    // Reset edit mode
    Object.keys(this.editMode).forEach(key => {
      this.editMode[key as keyof typeof this.editMode] = false;
    });
    this.clearMessages();
  }

  private loadAvailableLanguages(): void {
    this.userProfileService.getAvailableNewsLanguages()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (languages: NewsLanguage[]) => {
          this.availableLanguages = languages;
        },
        error: (error: any) => {
          console.error('Error loading languages:', error);
        }
      });
  }

  updateProfile(): void {
    if (this.profileForm.invalid) return;

    this.isLoading = true;
    this.clearMessages();

    const updateData: UpdateUserProfile = {
      ...this.profileForm.value,
      newsLanguageCode: this.userProfile?.newsLanguageCode || 'en'
    };

    this.userProfileService.updateMyProfile(updateData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: ProfileUpdateResult) => {
          this.isLoading = false;
          if (result.success) {
            this.showSuccess(result.message);
            if (result.profile) {
              this.userProfile = result.profile;
              // Update original values after successful save
              this.originalValues = { ...this.profileForm.value };
              // Reset edit mode
              Object.keys(this.editMode).forEach(key => {
                this.editMode[key as keyof typeof this.editMode] = false;
              });
            }
            if (result.requiresEmailConfirmation) {
              setTimeout(() => {
                this.showSuccess('✅ Perfil actualizado. 📧 No olvides hacer clic en "Enviar confirmación" para confirmar tu nuevo email.');
              }, 100);
            }
          } else {
            this.showError(result.message);
          }
        },
        error: (error: any) => {
          this.isLoading = false;
          this.showError('Error al actualizar el perfil');
          console.error('Error updating profile:', error);
        }
      });
  }

  changePassword(): void {
    if (this.passwordForm.invalid) return;

    this.isLoading = true;
    this.clearMessages();

    const passwordData: ChangePassword = this.passwordForm.value;

    this.userProfileService.changePassword(passwordData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: ProfileUpdateResult) => {
          this.isLoading = false;
          if (result.success) {
            this.showSuccess(result.message);
            this.passwordForm.reset();
          } else {
            this.showError(result.message);
          }
        },
        error: (error: any) => {
          this.isLoading = false;
          this.showError('Error al cambiar la contraseña');
          console.error('Error changing password:', error);
        }
      });
  }

  updatePreferences(): void {
    if (this.preferencesForm.invalid) return;

    this.isLoading = true;
    this.clearMessages();

    const languageCode = this.preferencesForm.get('newsLanguageCode')?.value;
    const oldLanguage = this.userProfile?.newsLanguageCode;

    this.userProfileService.updateNewsLanguage(languageCode)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: ProfileUpdateResult) => {
          this.isLoading = false;
          if (result.success) {
            this.showSuccess(result.message + ' - The news feed will refresh automatically.');
            if (this.userProfile) {
              this.userProfile.newsLanguageCode = languageCode;
              const selectedLanguage = this.availableLanguages.find(l => l.code === languageCode);
              if (selectedLanguage) {
                this.userProfile.newsLanguageName = selectedLanguage.name;
              }
            }
            
            // If language changed, emit event or reload news
            if (oldLanguage !== languageCode) {
              // Trigger a window event that the news component can listen to
              window.dispatchEvent(new CustomEvent('languageChanged', { 
                detail: { language: languageCode } 
              }));
              
              // Also store in localStorage for immediate availability
              localStorage.setItem('userLanguage', languageCode);
            }
          } else {
            this.showError(result.message);
          }
        },
        error: (error: any) => {
          this.isLoading = false;
          this.showError('Error al actualizar las preferencias');
          console.error('Error updating preferences:', error);
        }
      });
  }

  sendEmailConfirmation(): void {
    this.userProfileService.sendEmailConfirmation()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: ProfileUpdateResult) => {
          if (result.success) {
            this.showSuccess(result.message);
          } else {
            this.showError(result.message);
          }
        },
        error: (error: any) => {
          this.showError('Error al enviar la confirmación de correo');
          console.error('Error sending email confirmation:', error);
        }
      });
  }

  sendTestNotification(): void {
    if (!this.userProfile?.emailConfirmed) {
      this.showError('Debes confirmar tu email antes de recibir notificaciones');
      return;
    }

    this.isLoading = true;
    this.clearMessages();

    this.notificationService.sendTestNotification()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isLoading = false;
          this.showSuccess('¡Email de prueba enviado! Revisa tu bandeja de entrada en ' + this.userProfile?.email);
        },
        error: (error: any) => {
          this.isLoading = false;
          const errorMessage = error.error?.error?.message || 'Error al enviar el email de prueba';
          this.showError(errorMessage);
          console.error('Error sending test notification:', error);
        }
      });
  }

  private showSuccess(message: string): void {
    this.successMessage = message;
    this.errorMessage = '';
    setTimeout(() => this.successMessage = '', 5000);
  }

  private showError(message: string): void {
    this.errorMessage = message;
    this.successMessage = '';
    setTimeout(() => this.errorMessage = '', 5000);
  }

  private clearMessages(): void {
    this.successMessage = '';
    this.errorMessage = '';
  }
}
