import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { UserProfileService } from '../../core/services/user-profile.service';
import { takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-confirm-email',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="confirm-email-container">
      <div class="confirm-card">
        <div *ngIf="isLoading" class="loading-state">
          <div class="spinner"></div>
          <h2>Verificando tu email...</h2>
          <p>Por favor espera mientras confirmamos tu dirección de email.</p>
        </div>

        <div *ngIf="!isLoading && success" class="success-state">
          <div class="success-icon">✓</div>
          <h2>¡Email Confirmado!</h2>
          <p>{{ message }}</p>
          <button class="btn btn-primary" (click)="goToProfile()">
            Ir a Mi Perfil
          </button>
        </div>

        <div *ngIf="!isLoading && !success" class="error-state">
          <div class="error-icon">✕</div>
          <h2>Error al Confirmar</h2>
          <p>{{ message }}</p>
          <div class="error-actions">
            <button class="btn btn-primary" (click)="goToProfile()">
              Ir a Mi Perfil
            </button>
            <button class="btn btn-secondary" (click)="goToLogin()">
              Ir a Login
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .confirm-email-container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 2rem;
    }

    .confirm-card {
      background: white;
      border-radius: 12px;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
      padding: 3rem;
      max-width: 500px;
      width: 100%;
      text-align: center;
    }

    .loading-state,
    .success-state,
    .error-state {
      animation: fadeIn 0.5s ease-in;
    }

    @keyframes fadeIn {
      from {
        opacity: 0;
        transform: translateY(20px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    .spinner {
      width: 60px;
      height: 60px;
      margin: 0 auto 1.5rem;
      border: 4px solid #f3f3f3;
      border-top: 4px solid #667eea;
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }

    .success-icon {
      width: 80px;
      height: 80px;
      margin: 0 auto 1.5rem;
      background: #28a745;
      color: white;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 3rem;
      font-weight: bold;
      animation: scaleIn 0.5s ease-out;
    }

    .error-icon {
      width: 80px;
      height: 80px;
      margin: 0 auto 1.5rem;
      background: #dc3545;
      color: white;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 3rem;
      font-weight: bold;
      animation: scaleIn 0.5s ease-out;
    }

    @keyframes scaleIn {
      from {
        transform: scale(0);
      }
      to {
        transform: scale(1);
      }
    }

    h2 {
      color: #333;
      margin-bottom: 1rem;
      font-size: 1.75rem;
    }

    p {
      color: #666;
      margin-bottom: 2rem;
      line-height: 1.6;
    }

    .btn {
      padding: 0.75rem 2rem;
      border: none;
      border-radius: 6px;
      font-size: 1rem;
      font-weight: 500;
      cursor: pointer;
      transition: all 0.3s ease;
      text-decoration: none;
      display: inline-block;
    }

    .btn-primary {
      background: #667eea;
      color: white;
    }

    .btn-primary:hover {
      background: #5568d3;
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
    }

    .btn-secondary {
      background: #6c757d;
      color: white;
      margin-left: 0.5rem;
    }

    .btn-secondary:hover {
      background: #5a6268;
    }

    .error-actions {
      display: flex;
      gap: 0.5rem;
      justify-content: center;
      flex-wrap: wrap;
    }

    @media (max-width: 768px) {
      .confirm-card {
        padding: 2rem;
      }

      h2 {
        font-size: 1.5rem;
      }

      .error-actions {
        flex-direction: column;
      }

      .btn-secondary {
        margin-left: 0;
      }
    }
  `]
})
export class ConfirmEmailComponent implements OnInit {
  isLoading = true;
  success = false;
  message = '';
  
  private destroy$ = new Subject<void>();
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private userProfileService = inject(UserProfileService);

  ngOnInit(): void {
    // Get token from query params
    const token = this.route.snapshot.queryParamMap.get('token');
    
    if (!token) {
      this.isLoading = false;
      this.success = false;
      this.message = 'No se proporcionó un token de verificación válido.';
      return;
    }

    this.confirmEmail(token);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private confirmEmail(token: string): void {
    this.userProfileService.confirmEmail(token)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.isLoading = false;
          this.success = result.success;
          this.message = result.message;
        },
        error: (error) => {
          console.error('Error confirming email:', error);
          this.isLoading = false;
          this.success = false;
          this.message = 'Error al confirmar el email. Por favor, intenta nuevamente o contacta con soporte.';
        }
      });
  }

  goToProfile(): void {
    this.router.navigate(['/profile']);
  }

  goToLogin(): void {
    this.router.navigate(['/auth/login']);
  }
}
