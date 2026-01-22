import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { RegisterRequest } from '../../../shared/models/auth.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="register-container">
      <div class="register-card">
        <div class="register-header">
          <h1>Create Account</h1>
          <p>Join NewsApp to get personalized news experience</p>
        </div>

        <form (ngSubmit)="onSubmit()" class="register-form" #registerForm="ngForm">
          <div class="form-group">
            <label for="username">Username</label>
            <input
              type="text"
              id="username"
              name="username"
              [(ngModel)]="userData.userName"
              required
              minlength="3"
              class="form-control"
              [class.error]="submitted && (!userData.userName || userData.userName.length < 3)"
              placeholder="Enter your username">
            <div class="error-message" *ngIf="submitted && !userData.userName">
              Username is required
            </div>
            <div class="error-message" *ngIf="submitted && userData.userName && userData.userName.length < 3">
              Username must be at least 3 characters
            </div>
          </div>

          <div class="form-group">
            <label for="email">Email</label>
            <input
              type="email"
              id="email"
              name="email"
              [(ngModel)]="userData.emailAddress"
              required
              email
              class="form-control"
              [class.error]="submitted && (!userData.emailAddress || !isValidEmail(userData.emailAddress))"
              placeholder="Enter your email address">
            <div class="error-message" *ngIf="submitted && !userData.emailAddress">
              Email is required
            </div>
            <div class="error-message" *ngIf="submitted && userData.emailAddress && !isValidEmail(userData.emailAddress)">
              Please enter a valid email address
            </div>
          </div>

          <div class="form-group">
            <label for="password">Password</label>
            <input
              type="password"
              id="password"
              name="password"
              [(ngModel)]="userData.password"
              required
              minlength="6"
              class="form-control"
              [class.error]="submitted && (!userData.password || userData.password.length < 6)"
              placeholder="Enter your password">
            <div class="error-message" *ngIf="submitted && !userData.password">
              Password is required
            </div>
            <div class="error-message" *ngIf="submitted && userData.password && userData.password.length < 6">
              Password must be at least 6 characters
            </div>
            <div class="password-hint">
              Password should contain at least 6 characters
            </div>
          </div>

          <div class="form-group">
            <label for="confirmPassword">Confirm Password</label>
            <input
              type="password"
              id="confirmPassword"
              name="confirmPassword"
              [(ngModel)]="confirmPassword"
              required
              class="form-control"
              [class.error]="submitted && (!confirmPassword || confirmPassword !== userData.password)"
              placeholder="Confirm your password">
            <div class="error-message" *ngIf="submitted && !confirmPassword">
              Please confirm your password
            </div>
            <div class="error-message" *ngIf="submitted && confirmPassword && confirmPassword !== userData.password">
              Passwords do not match
            </div>
          </div>

          <div class="form-group">
            <label class="checkbox-label">
              <input
                type="checkbox"
                [(ngModel)]="acceptTerms"
                name="acceptTerms"
                required>
              <span class="checkmark"></span>
              I accept the <a href="#" (click)="showTerms($event)">Terms of Service</a> and <a href="#" (click)="showPrivacy($event)">Privacy Policy</a>
            </label>
            <div class="error-message" *ngIf="submitted && !acceptTerms">
              You must accept the terms and conditions
            </div>
          </div>

          <button
            type="submit"
            class="register-btn"
            [disabled]="loading"
            [class.loading]="loading">
            <span *ngIf="!loading">Create Account</span>
            <span *ngIf="loading" class="loading-spinner">
              <div class="spinner"></div>
              Creating account...
            </span>
          </button>

          <div class="error-message" *ngIf="errorMessage">
            {{ errorMessage }}
          </div>

          <div class="success-message" *ngIf="successMessage">
            {{ successMessage }}
          </div>
        </form>

        <div class="register-footer">
          <p>Already have an account? <a href="#" (click)="goToLogin($event)">Sign in here</a></p>
          <p><a href="#" (click)="goToNews($event)">Continue without account</a></p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .register-container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 20px;
    }

    .register-card {
      background: white;
      border-radius: 12px;
      box-shadow: 0 20px 40px rgba(0,0,0,0.1);
      padding: 40px;
      width: 100%;
      max-width: 500px;
      animation: slideUp 0.5s ease-out;
      max-height: 90vh;
      overflow-y: auto;
    }

    @keyframes slideUp {
      from {
        opacity: 0;
        transform: translateY(30px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    .register-header {
      text-align: center;
      margin-bottom: 30px;
    }

    .register-header h1 {
      color: #333;
      margin-bottom: 10px;
      font-size: 2em;
      font-weight: 300;
    }

    .register-header p {
      color: #666;
      margin: 0;
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

    .checkbox-label {
      display: flex;
      align-items: flex-start;
      cursor: pointer;
      font-weight: normal;
      line-height: 1.4;
    }

    .checkbox-label input[type="checkbox"] {
      margin-right: 10px;
      margin-top: 2px;
      transform: scale(1.2);
    }

    .checkbox-label a {
      color: #667eea;
      text-decoration: none;
    }

    .checkbox-label a:hover {
      text-decoration: underline;
    }

    .password-hint {
      font-size: 12px;
      color: #6c757d;
      margin-top: 5px;
    }

    .register-btn {
      width: 100%;
      padding: 15px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      border: none;
      border-radius: 8px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      transition: transform 0.2s ease;
      margin-bottom: 15px;
    }

    .register-btn:hover:not(:disabled) {
      transform: translateY(-2px);
    }

    .register-btn:disabled {
      opacity: 0.7;
      cursor: not-allowed;
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

    .error-message {
      color: #dc3545;
      font-size: 14px;
      margin-top: 5px;
    }

    .success-message {
      color: #28a745;
      font-size: 14px;
      margin-top: 5px;
      padding: 10px;
      background: #d4edda;
      border: 1px solid #c3e6cb;
      border-radius: 5px;
    }

    .register-footer {
      text-align: center;
      margin-top: 20px;
    }

    .register-footer p {
      margin: 10px 0;
      color: #666;
    }

    .register-footer a {
      color: #667eea;
      text-decoration: none;
      font-weight: 500;
    }

    .register-footer a:hover {
      text-decoration: underline;
    }

    @media (max-width: 768px) {
      .register-card {
        padding: 30px 20px;
        margin: 10px;
      }

      .register-header h1 {
        font-size: 1.6em;
      }
    }
  `]
})
export class RegisterComponent implements OnInit {
  userData: RegisterRequest = {
    userName: '',
    emailAddress: '',
    password: '',
    appName: 'NewsApp'
  };

  confirmPassword = '';
  acceptTerms = false;
  loading = false;
  submitted = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // If already logged in, redirect to news
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/news']);
    }
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.isFormValid()) {
      return;
    }

    this.loading = true;

    this.authService.register(this.userData).subscribe({
      next: (response) => {
        this.loading = false;
        this.successMessage = 'Account created successfully! You can now sign in with your credentials.';
        
        // Clear form
        this.resetForm();
        
        // Redirect to login after 2 seconds
        setTimeout(() => {
          this.router.navigate(['/auth/login'], { 
            queryParams: { 
              email: this.userData.emailAddress,
              registered: 'true' 
            } 
          });
        }, 2000);
      },
      error: (error) => {
        console.error('Registration error:', error);
        this.loading = false;
        
        // Handle specific error messages
        if (error.error?.error?.message) {
          this.errorMessage = error.error.error.message;
        } else if (error.error?.message) {
          this.errorMessage = error.error.message;
        } else if (error.status === 400) {
          this.errorMessage = 'Registration failed. Please check your information and try again.';
        } else if (error.status === 409) {
          this.errorMessage = 'Username or email already exists. Please choose different ones.';
        } else {
          this.errorMessage = 'Registration failed. Please try again later.';
        }
      }
    });
  }

  private isFormValid(): boolean {
    // Check each condition explicitly and return boolean
    const isUsernameValid = !!(this.userData.userName && this.userData.userName.length >= 3);
    const isEmailValid = !!(this.userData.emailAddress && this.isValidEmail(this.userData.emailAddress));
    const isPasswordValid = !!(this.userData.password && this.userData.password.length >= 6);
    const isConfirmPasswordValid = !!(this.confirmPassword && this.confirmPassword === this.userData.password);
    const isTermsAccepted = !!this.acceptTerms;

    return (
      isUsernameValid &&
      isEmailValid &&
      isPasswordValid &&
      isConfirmPasswordValid &&
      isTermsAccepted
    );
  }

  private resetForm(): void {
    this.userData = {
      userName: '',
      emailAddress: '',
      password: '',
      appName: 'NewsApp'
    };
    this.confirmPassword = '';
    this.acceptTerms = false;
    this.submitted = false;
  }

  isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  goToLogin(event: Event): void {
    event.preventDefault();
    this.router.navigate(['/auth/login']);
  }

  goToNews(event: Event): void {
    event.preventDefault();
    this.router.navigate(['/news']);
  }

  showTerms(event: Event): void {
    event.preventDefault();
    alert('Terms of Service: By creating an account, you agree to use NewsApp responsibly and follow our community guidelines.');
  }

  showPrivacy(event: Event): void {
    event.preventDefault();
    alert('Privacy Policy: We protect your privacy and only use your email for authentication and news personalization.');
  }
}
