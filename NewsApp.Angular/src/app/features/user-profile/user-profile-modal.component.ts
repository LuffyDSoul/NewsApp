import { Component, EventEmitter, Input, Output, OnInit, OnDestroy, OnChanges, SimpleChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { UserProfileService } from '../../core/services/user-profile.service';
import { 
  UserProfile, 
  UpdateUserProfile, 
  ChangePassword, 
  NewsLanguage,
  ProfileUpdateResult
} from '../../shared/models/user-profile.model';

@Component({
  selector: 'app-user-profile-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="modal-overlay" *ngIf="isOpen" (click)="onOverlayClick($event)">
      <div class="modal-container" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <h2>Mi Perfil</h2>
          <button class="close-btn" (click)="close()" type="button">×</button>
        </div>

        <div class="modal-body">
          <!-- Pestañas -->
          <div class="tabs">
            <button 
              class="tab" 
              [class.active]="activeTab === 'profile'"
              (click)="activeTab = 'profile'" type="button">
              Información Personal
            </button>
            <button 
              class="tab" 
              [class.active]="activeTab === 'password'"
              (click)="activeTab = 'password'" type="button">
              Cambiar Contraseña
            </button>
            <button 
              class="tab" 
              [class.active]="activeTab === 'preferences'"
              (click)="activeTab = 'preferences'" type="button">
              Preferencias
            </button>
          </div>

          <!-- Contenido de las pestañas -->
          <div class="tab-content">
            <!-- Información Personal -->
            <div *ngIf="activeTab === 'profile'" class="tab-panel">
              <form [formGroup]="profileForm" (ngSubmit)="updateProfile()">
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
                      <button type="button" class="confirm-email-btn" (click)="sendEmailConfirmation()">
                        Enviar confirmación
                      </button>
                    </div>
                  </ng-template>
                  <div class="error-message" *ngIf="profileForm.get('email')?.invalid && profileForm.get('email')?.touched">
                    Ingresa un correo electrónico válido
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
          </div>

          <!-- Mensajes de éxito/error -->
          <div class="alert alert-success" *ngIf="successMessage">
            {{ successMessage }}
          </div>
          <div class="alert alert-error" *ngIf="errorMessage">
            {{ errorMessage }}
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      justify-content: center;
      align-items: center;
      z-index: 1000;
      padding: 1rem;
    }

    .modal-container {
      background: white;
      border-radius: 8px;
      box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
      width: 100%;
      max-width: 600px;
      max-height: 90vh;
      overflow-y: auto;
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1.5rem;
      border-bottom: 1px solid #e9ecef;
    }

    .modal-header h2 {
      margin: 0;
      color: #333;
    }

    .close-btn {
      background: none;
      border: none;
      font-size: 1.5rem;
      cursor: pointer;
      color: #666;
      width: 2rem;
      height: 2rem;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 50%;
      transition: background-color 0.2s ease;
    }

    .close-btn:hover {
      background: #f8f9fa;
      color: #333;
    }

    .modal-body {
      padding: 0;
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
      font-size: 0.9rem;
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
      padding: 1.5rem;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
      margin-bottom: 1rem;
    }

    .form-group {
      margin-bottom: 1rem;
    }

    .form-group label {
      display: block;
      margin-bottom: 0.5rem;
      font-weight: 500;
      color: #333;
      font-size: 0.9rem;
    }

    .form-control {
      width: 100%;
      padding: 0.6rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 0.9rem;
      transition: border-color 0.3s ease;
    }

    .form-control:focus {
      outline: none;
      border-color: #667eea;
      box-shadow: 0 0 0 2px rgba(102, 126, 234, 0.2);
    }

    .form-control:read-only {
      background-color: #f5f5f5;
      cursor: not-allowed;
      color: #666;
    }

    .form-control:read-only:focus {
      border-color: #ddd;
      box-shadow: none;
    }

    .input-with-edit {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      width: 100%;
    }

    .input-with-edit .form-control {
      flex: 1;
    }

    .edit-btn {
      background: #667eea;
      color: white;
      border: none;
      padding: 0.5rem;
      border-radius: 4px;
      cursor: pointer;
      font-size: 1rem;
      transition: background 0.3s ease;
      display: flex;
      align-items: center;
      justify-content: center;
      min-width: 36px;
      height: 36px;
    }

    .edit-btn:hover {
      background: #5a6fd8;
    }

    .btn-secondary {
      background: #6c757d;
      color: white;
      margin-right: 0.5rem;
    }

    .btn-secondary:hover:not(:disabled) {
      background: #5a6268;
    }

    .form-control.error {
      border-color: #dc3545;
    }

    .error-message {
      color: #dc3545;
      font-size: 0.8rem;
      margin-top: 0.25rem;
    }

    .form-help {
      color: #666;
      font-size: 0.8rem;
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
      font-size: 0.8rem;
    }

    .status-unconfirmed {
      color: #ffc107;
      font-size: 0.8rem;
    }

    .confirm-email-btn {
      background: #ffc107;
      color: #333;
      border: none;
      padding: 0.2rem 0.4rem;
      border-radius: 3px;
      font-size: 0.7rem;
      cursor: pointer;
      transition: background-color 0.3s ease;
    }

    .confirm-email-btn:hover {
      background: #e0a800;
    }

    .form-actions {
      margin-top: 1.5rem;
      text-align: right;
    }

    .btn {
      padding: 0.6rem 1.2rem;
      border: none;
      border-radius: 4px;
      font-size: 0.9rem;
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

    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .alert {
      margin: 1rem;
      padding: 0.75rem;
      border-radius: 4px;
      font-size: 0.85rem;
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

    @media (max-width: 768px) {
      .modal-container {
        margin: 0;
        max-height: 100vh;
        border-radius: 0;
      }

      .form-row {
        grid-template-columns: 1fr;
      }

      .tab {
        font-size: 0.8rem;
        padding: 0.8rem;
      }

      .tab-content {
        padding: 1rem;
      }
    }
  `]
})
export class UserProfileModalComponent implements OnInit, OnDestroy, OnChanges {
  @Input() isOpen = false;
  @Output() closeModal = new EventEmitter<void>();
  @Output() languageChanged = new EventEmitter<string>();

  activeTab: 'profile' | 'password' | 'preferences' = 'profile';
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

  constructor() {
    this.profileForm = this.createProfileForm();
    this.passwordForm = this.createPasswordForm();
    this.preferencesForm = this.createPreferencesForm();
  }

  ngOnInit(): void {
    console.log('UserProfileModalComponent initialized, isOpen:', this.isOpen);
    if (this.isOpen) {
      this.loadProfile();
      this.loadAvailableLanguages();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isOpen'] && changes['isOpen'].currentValue === true) {
      console.log('Modal opened, loading profile...');
      this.loadProfile();
      this.loadAvailableLanguages();
    }
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
    console.log('Loading profile in modal...');
    this.userProfileService.getMyProfile()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (profile: UserProfile) => {
          console.log('Profile loaded:', profile);
          this.userProfile = profile;
          const profileData = {
            userName: profile.userName,
            email: profile.email,
            name: profile.name,
            surname: profile.surname,
            phoneNumber: profile.phoneNumber
          };
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
            }
            // Reset edit mode and update original values
            Object.keys(this.editMode).forEach(key => {
              this.editMode[key as keyof typeof this.editMode] = false;
            });
            this.originalValues = { ...this.profileForm.value };
            
            if (result.requiresEmailConfirmation) {
              this.showError('Se ha enviado un correo de confirmación a tu nueva dirección.');
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

    this.userProfileService.updateNewsLanguage(languageCode)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: ProfileUpdateResult) => {
          this.isLoading = false;
          if (result.success) {
            this.showSuccess(result.message);
            if (this.userProfile) {
              this.userProfile.newsLanguageCode = languageCode;
              const selectedLanguage = this.availableLanguages.find(l => l.code === languageCode);
              if (selectedLanguage) {
                this.userProfile.newsLanguageName = selectedLanguage.name;
              }
            }
            // Emit event to notify that language changed
            this.languageChanged.emit(languageCode);
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

  onOverlayClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.close();
    }
  }

  close(): void {
    this.isOpen = false;
    this.closeModal.emit();
    this.clearMessages();
    this.activeTab = 'profile';
    this.passwordForm.reset();
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
