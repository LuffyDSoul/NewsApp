import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterModule } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterModule],
  template: `
    <div class="app-container">
      <header class="app-header">
        <h1>News App</h1>
        <nav>
          <a routerLink="/news" routerLinkActive="active">Latest News</a>
        </nav>
      </header>
      
      <main class="app-content">
        <router-outlet></router-outlet>
      </main>
      
      <footer class="app-footer">
        <p>&copy; 2025 News App. Powered by ABP Framework.</p>
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
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    
    .app-header h1 {
      margin: 0;
      font-size: 1.8em;
      font-weight: 300;
    }
    
    .app-header nav a {
      color: white;
      text-decoration: none;
      margin-left: 2rem;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      transition: background-color 0.3s ease;
    }
    
    .app-header nav a:hover,
    .app-header nav a.active {
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
  `]
})
export class App {
  title = 'NewsApp Angular';
}
