import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LoginRequest, CurrentUser } from '../../../shared/models/auth.model'; // ✅ Agregando CurrentUser import
import { filter, take } from 'rxjs/operators';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="login-container">
      <div class="login-card">
        <div class="login-header">
          <h1>NewsApp Login</h1>
          <p>Sign in to access your personalized news</p>
        </div>

        <div class="demo-credentials" *ngIf="!hideDemo">
          <div class="demo-info">
            <h3>🚀 Demo Credentials</h3>
            <p><strong>Username:</strong> admin</p>
            <p><strong>Password:</strong> 1q2w3E*</p>
            <button type="button" (click)="useDemoCredentials()" class="demo-btn">
              Use Demo Login
            </button>
          </div>
        </div>

        <form (ngSubmit)="onSubmit()" class="login-form" #loginForm="ngForm">
          <div class="form-group">
            <label for="username">Username or Email</label>
            <input
              type="text"
              id="username"
              name="username"
              [(ngModel)]="credentials.userNameOrEmailAddress"
              required
              class="form-control"
              [class.error]="submitted && !credentials.userNameOrEmailAddress"
              placeholder="Enter your username or email">
            <div class="error-message" *ngIf="submitted && !credentials.userNameOrEmailAddress">
              Username or email is required
            </div>
          </div>

          <div class="form-group">
            <label for="password">Password</label>
            <input
              type="password"
              id="password"
              name="password"
              [(ngModel)]="credentials.password"
              required
              class="form-control"
              [class.error]="submitted && !credentials.password"
              placeholder="Enter your password">
            <div class="error-message" *ngIf="submitted && !credentials.password">
              Password is required
            </div>
          </div>

          <div class="form-group">
            <label class="checkbox-label">
              <input
                type="checkbox"
                [(ngModel)]="credentials.rememberMe"
                name="rememberMe">
              <span class="checkmark"></span>
              Remember me
            </label>
          </div>

          <button
            type="submit"
            class="login-btn"
            [disabled]="loading"
            [class.loading]="loading">
            <span *ngIf="!loading">Sign In</span>
            <span *ngIf="loading" class="loading-spinner">
              <div class="spinner"></div>
              Signing in...
            </span>
          </button>

          <div class="error-message" *ngIf="errorMessage">
            {{ errorMessage }}
          </div>
        </form>

        <div class="login-footer">
          <p>Don't have an account? <a href="#" (click)="goToRegister($event)">Register here</a></p>
          <p><a href="#" (click)="goToNews($event)">Continue without login</a></p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .login-container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 20px;
    }

    .login-card {
      background: white;
      border-radius: 12px;
      box-shadow: 0 20px 40px rgba(0,0,0,0.1);
      padding: 40px;
      width: 100%;
      max-width: 450px;
      animation: slideUp 0.5s ease-out;
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

    .login-header {
      text-align: center;
      margin-bottom: 30px;
    }

    .login-header h1 {
      color: #333;
      margin-bottom: 10px;
      font-size: 2em;
      font-weight: 300;
    }

    .login-header p {
      color: #666;
      margin: 0;
    }

    .demo-credentials {
      background: #f8f9fa;
      border: 2px solid #e9ecef;
      border-radius: 8px;
      padding: 20px;
      margin-bottom: 25px;
      text-align: center;
    }

    .demo-info h3 {
      margin-top: 0;
      color: #495057;
      font-size: 1.1em;
    }

    .demo-info p {
      margin: 8px 0;
      color: #6c757d;
    }

    .demo-btn {
      background: #17a2b8;
      color: white;
      border: none;
      padding: 10px 20px;
      border-radius: 6px;
      cursor: pointer;
      font-weight: 500;
      transition: background 0.2s ease;
      margin-top: 10px;
    }

    .demo-btn:hover {
      background: #138496;
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
      align-items: center;
      cursor: pointer;
      font-weight: normal;
    }

    .checkbox-label input[type="checkbox"] {
      margin-right: 10px;
      transform: scale(1.2);
    }

    .login-btn {
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

    .login-btn:hover:not(:disabled) {
      transform: translateY(-2px);
    }

    .login-btn:disabled {
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

    .login-footer {
      text-align: center;
      margin-top: 20px;
    }

    .login-footer p {
      margin: 10px 0;
      color: #666;
    }

    .login-footer a {
      color: #667eea;
      text-decoration: none;
      font-weight: 500;
    }

    .login-footer a:hover {
      text-decoration: underline;
    }
  `]
})
export class LoginComponent implements OnInit {
  credentials: LoginRequest = {
    userNameOrEmailAddress: '',
    password: '',
    rememberMe: false
  };

  loading = false;
  submitted = false;
  errorMessage = '';
  hideDemo = false;
  private returnUrl = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Get return url from route parameters or default to '/news'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/news';

    // Check if user just registered
    const registered = this.route.snapshot.queryParams['registered'];
    const email = this.route.snapshot.queryParams['email'];
    
    if (registered === 'true' && email) {
      this.credentials.userNameOrEmailAddress = email;
      this.hideDemo = true;
    }

    // Check if already logged in
    if (this.authService.isAuthenticated()) {
      this.router.navigate([this.returnUrl]);
    }
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessage = '';

    if (!this.credentials.userNameOrEmailAddress || !this.credentials.password) {
      return;
    }

    this.loading = true;

    this.authService.login(this.credentials).subscribe({
      next: (success: boolean) => {
        this.loading = false;
        if (success) {
          // Force a small delay to ensure state synchronization
          setTimeout(() => {
            // Double-check authentication status before navigating
            if (this.authService.isAuthenticated()) {
              this.router.navigate([this.returnUrl]);
            } else {
              // If still not authenticated, wait for the state to update
              this.authService.currentUser$.pipe(
                filter((user: CurrentUser) => user.isAuthenticated),
                take(1)
              ).subscribe(() => {
                this.router.navigate([this.returnUrl]);
              });
            }
          }, 50);
        } else {
          this.errorMessage = 'Invalid username/email or password';
        }
      },
      error: (error: any) => {
        console.error('Login error:', error);
        this.errorMessage = 'Login failed. Please check your credentials and try again.';
        this.loading = false;
      }
    });
  }

  useDemoCredentials(): void {
    this.credentials.userNameOrEmailAddress = 'admin';
    this.credentials.password = '1q2w3E*';
    this.hideDemo = true;
  }

  goToRegister(event: Event): void {
    event.preventDefault();
    this.router.navigate(['/auth/register']);
  }

  goToNews(event: Event): void {
    event.preventDefault();
    this.router.navigate(['/news']);
  }
}
