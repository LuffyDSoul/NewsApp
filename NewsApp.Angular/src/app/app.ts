import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterModule, Router } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { CurrentUser } from './shared/models/auth.model';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterModule],
  template: `
    <div class="app-container">
      <header class="app-header">
        <div class="header-brand">
          <h1 (click)="goHome()">NewsApp</h1>
        </div>
        
        <nav class="header-nav">
          <a routerLink="/news" routerLinkActive="active" *ngIf="currentUser.isAuthenticated">Latest News</a>
          <a routerLink="/reading-lists" routerLinkActive="active" *ngIf="currentUser.isAuthenticated">Reading Lists</a>
        </nav>

        <div class="header-user" *ngIf="currentUser.isAuthenticated; else loginSection">
          <div class="user-info">
            <span class="welcome-text">Welcome, </span>
            <span class="username">{{ currentUser.userName || currentUser.email }}</span>
          </div>
          <button (click)="logout()" class="logout-btn">Logout</button>
        </div>

        <ng-template #loginSection>
          <div class="header-auth">
            <a routerLink="/auth/register" class="register-link">Sign Up</a>
            <a routerLink="/auth/login" class="login-link">Sign In</a>
          </div>
        </ng-template>
      </header>
      
      <main class="app-content">
        <router-outlet></router-outlet>
      </main>
      
      <footer class="app-footer">
        <p>&copy; 2025 NewsApp. Powered by ABP Framework & Angular.</p>
      </footer>
    </div>
  `,
  styles: [`
    .app-container {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
    }
    
    .app-header {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      padding: 1rem 2rem;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 1rem;
    }

    .header-brand h1 {
      margin: 0;
      font-size: 1.8em;
      font-weight: 300;
      cursor: pointer;
      transition: opacity 0.2s ease;
    }

    .header-brand h1:hover {
      opacity: 0.8;
    }
    
    .header-nav {
      flex: 1;
      display: flex;
      justify-content: center;
    }

    .header-nav a {
      color: white;
      text-decoration: none;
      margin: 0 1rem;
      padding: 0.5rem 1rem;
      border-radius: 6px;
      transition: background-color 0.3s ease;
      font-weight: 500;
    }
    
    .header-nav a:hover,
    .header-nav a.active {
      background-color: rgba(255, 255, 255, 0.2);
    }

    .header-user {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .user-info {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      font-size: 0.9em;
    }

    .welcome-text {
      opacity: 0.8;
    }

    .username {
      font-weight: 600;
    }

    .logout-btn {
      background: rgba(255, 255, 255, 0.2);
      color: white;
      border: 1px solid rgba(255, 255, 255, 0.3);
      padding: 0.5rem 1rem;
      border-radius: 6px;
      cursor: pointer;
      transition: background-color 0.3s ease;
      font-weight: 500;
    }

    .logout-btn:hover {
      background: rgba(255, 255, 255, 0.3);
    }

    .header-auth {
      display: flex;
      gap: 1rem;
      align-items: center;
    }

    .header-auth .login-link,
    .header-auth .register-link {
      color: white;
      text-decoration: none;
      padding: 0.5rem 1rem;
      border: 1px solid rgba(255, 255, 255, 0.3);
      border-radius: 6px;
      transition: background-color 0.3s ease;
      font-weight: 500;
    }

    .header-auth .register-link {
      background: rgba(255, 255, 255, 0.1);
    }

    .header-auth .login-link:hover,
    .header-auth .register-link:hover {
      background-color: rgba(255, 255, 255, 0.2);
    }
    
    .app-content {
      flex: 1;
      background-color: #f8f9fa;
    }
    
    .app-footer {
      background-color: #343a40;
      color: white;
      text-align: center;
      padding: 1rem;
      margin-top: auto;
    }
    
    .app-footer p {
      margin: 0;
      font-size: 0.9em;
    }

    @media (max-width: 768px) {
      .app-header {
        flex-direction: column;
        text-align: center;
      }

      .header-nav {
        order: 3;
        width: 100%;
        justify-content: center;
      }

      .user-info {
        align-items: center;
      }
    }
  `]
})
export class App implements OnInit {
  title = 'NewsApp Angular';
  currentUser: CurrentUser = { isAuthenticated: false, roles: [] };

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
  }

  goHome(): void {
    if (this.currentUser.isAuthenticated) {
      this.router.navigate(['/news']);
    } else {
      this.router.navigate(['/auth/login']);
    }
  }

  logout(): void {
    this.authService.logout().subscribe(() => {
      this.router.navigate(['/auth/login']);
    });
  }
}
