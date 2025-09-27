import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NewsService } from '../../../core/services/news.service';
import { AuthService } from '../../../core/services/auth.service'; // ✅ NUEVO
import { LanguageService } from '../../../core/services/language.service'; // ✅ NUEVO
import { NewsArticleDto, NewsSearchDto, PagedResultDto } from '../../../shared/models/news.model';
import { CurrentUser } from '../../../shared/models/auth.model'; // ✅ NUEVO

@Component({
  selector: 'app-news-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="news-container">
      <!-- ✅ NUEVO: Header con información del idioma -->
      <div class="news-header">
        <div class="header-content">
          <h1>📰 Latest News</h1>
          <div class="language-info" *ngIf="currentUser.isAuthenticated">
            <span class="language-indicator">
              {{ getCurrentLanguageFlag() }} 
              News in {{ getCurrentLanguageName() }}
            </span>
            <small class="language-hint">
              Change language in your <a href="/profile">profile settings</a>
            </small>
          </div>
        </div>
        
        <div class="header-actions">
          <button 
            (click)="refreshNews()" 
            class="refresh-btn"
            [disabled]="loading">
            🔄 {{ loading ? 'Loading...' : 'Refresh' }}
          </button>
          <button 
            (click)="testConnection()" 
            class="test-btn"
            [disabled]="testing">
            {{ testing ? 'Testing...' : 'Test Connection' }}
          </button>
        </div>
      </div>

      <!-- Search Section -->
      <div class="search-section">
        <div class="search-bar">
          <input 
            type="text" 
            [(ngModel)]="searchQuery" 
            placeholder="Search news in your preferred language..." 
            class="search-input"
            (keyup.enter)="searchNews()">
          <button 
            (click)="searchNews()" 
            class="search-btn"
            [disabled]="loading">
            🔍 Search
          </button>
        </div>
        
        <div class="filters">
          <select [(ngModel)]="selectedCategory" class="category-select" (change)="onCategoryChange()">
            <option value="">All Categories</option>
            <option value="business">Business</option>
            <option value="entertainment">Entertainment</option>
            <option value="health">Health</option>
            <option value="science">Science</option>
            <option value="sports">Sports</option>
            <option value="technology">Technology</option>
          </select>
        </div>
      </div>

      <!-- Loading State -->
      <div *ngIf="loading" class="loading-state">
        <div class="spinner"></div>
        <p>Loading personalized news...</p>
      </div>

      <!-- Error State -->
      <div *ngIf="errorMessage" class="error-state">
        <p>{{ errorMessage }}</p>
        <button (click)="refreshNews()" class="retry-btn">Try Again</button>
      </div>

      <!-- Success State for Connection Test -->
      <div *ngIf="connectionSuccess" class="success-state">
        <p>✅ Backend connection successful!</p>
      </div>

      <!-- News Articles -->
      <div *ngIf="!loading && !errorMessage" class="news-grid">
        <div *ngIf="articles.length === 0" class="empty-state">
          <p>No news articles found. Try a different search or category.</p>
        </div>
        
        <article *ngFor="let article of articles" class="news-card">
          <div class="news-image" *ngIf="article.urlToImage">
            <img [src]="article.urlToImage" [alt]="article.title" loading="lazy">
          </div>
          
          <div class="news-content">
            <div class="news-meta">
              <span class="news-source">{{ article.source || 'Unknown Source' }}</span>
              <span class="news-date">{{ formatDate(article.publishedAt) }}</span>
              <!-- ✅ NUEVO: Indicador de idioma del artículo -->
              <span class="article-language" *ngIf="article.languageCode">
                {{ getArticleLanguageFlag(article.languageCode) }}
              </span>
            </div>
            
            <h2 class="news-title">
              <a [href]="article.url" target="_blank" rel="noopener noreferrer">
                {{ article.title }}
              </a>
            </h2>
            
            <p class="news-description" *ngIf="article.description">
              {{ article.description }}
            </p>
            
            <div class="news-footer">
              <span class="news-author" *ngIf="article.author">
                By {{ article.author }}
              </span>
              <a [href]="article.url" target="_blank" class="read-more-btn">
                Read More →
              </a>
            </div>
          </div>
        </article>
      </div>

      <!-- ✅ NUEVO: Tips de personalización -->
      <div class="personalization-tips" *ngIf="!loading && articles.length > 0">
        <h3>💡 Personalization Tips</h3>
        <ul>
          <li>Change your preferred language in <a href="/profile">Profile Settings</a></li>
          <li>News are automatically filtered by your language preference</li>
          <li>Use search to find specific topics in your language</li>
        </ul>
      </div>
    </div>
  `,
  styles: [`
    .news-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 20px;
      background: #f8f9fa;
      min-height: 100vh;
    }

    .news-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 30px;
      padding: 20px;
      background: white;
      border-radius: 12px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
    }

    .header-content h1 {
      margin: 0 0 10px 0;
      color: #333;
      font-size: 2em;
    }

    /* ✅ NUEVOS: Estilos para indicador de idioma */
    .language-info {
      display: flex;
      flex-direction: column;
      gap: 5px;
    }

    .language-indicator {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 6px 12px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      border-radius: 20px;
      font-size: 0.9em;
      font-weight: 500;
    }

    .language-hint {
      color: #666;
      font-size: 0.8em;
    }

    .language-hint a {
      color: #667eea;
      text-decoration: none;
    }

    .language-hint a:hover {
      text-decoration: underline;
    }

    .header-actions {
      display: flex;
      gap: 10px;
      flex-wrap: wrap;
    }

    .refresh-btn, .test-btn {
      padding: 10px 20px;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      font-weight: 500;
      transition: all 0.2s ease;
    }

    .refresh-btn {
      background: #28a745;
      color: white;
    }

    .refresh-btn:hover:not(:disabled) {
      background: #218838;
    }

    .test-btn {
      background: #17a2b8;
      color: white;
    }

    .test-btn:hover:not(:disabled) {
      background: #138496;
    }

    .refresh-btn:disabled, .test-btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .search-section {
      background: white;
      padding: 20px;
      border-radius: 12px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
      margin-bottom: 30px;
    }

    .search-bar {
      display: flex;
      gap: 10px;
      margin-bottom: 15px;
    }

    .search-input {
      flex: 1;
      padding: 12px 16px;
      border: 2px solid #e9ecef;
      border-radius: 8px;
      font-size: 16px;
      transition: border-color 0.2s ease;
    }

    .search-input:focus {
      outline: none;
      border-color: #667eea;
    }

    .search-btn {
      padding: 12px 24px;
      background: #667eea;
      color: white;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      font-weight: 500;
      transition: background 0.2s ease;
    }

    .search-btn:hover:not(:disabled) {
      background: #5a6fd8;
    }

    .search-btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .filters {
      display: flex;
      gap: 15px;
      align-items: center;
    }

    .category-select {
      padding: 8px 12px;
      border: 2px solid #e9ecef;
      border-radius: 6px;
      background: white;
      cursor: pointer;
    }

    .category-select:focus {
      outline: none;
      border-color: #667eea;
    }

    .loading-state, .error-state, .success-state {
      text-align: center;
      padding: 40px 20px;
      background: white;
      border-radius: 12px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
    }

    .spinner {
      width: 40px;
      height: 40px;
      border: 4px solid #f3f3f3;
      border-top: 4px solid #667eea;
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin: 0 auto 15px;
    }

    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }

    .success-state {
      background: #d4edda;
      border: 1px solid #c3e6cb;
      color: #155724;
    }

    .error-state {
      background: #f8d7da;
      border: 1px solid #f5c6cb;
      color: #721c24;
    }

    .retry-btn {
      margin-top: 15px;
      padding: 10px 20px;
      background: #dc3545;
      color: white;
      border: none;
      border-radius: 6px;
      cursor: pointer;
    }

    .empty-state {
      text-align: center;
      padding: 40px;
      color: #666;
    }

    .news-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
      gap: 20px;
    }

    .news-card {
      background: white;
      border-radius: 12px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
      overflow: hidden;
      transition: transform 0.2s ease, box-shadow 0.2s ease;
    }

    .news-card:hover {
      transform: translateY(-5px);
      box-shadow: 0 5px 20px rgba(0,0,0,0.15);
    }

    .news-image {
      height: 200px;
      overflow: hidden;
    }

    .news-image img {
      width: 100%;
      height: 100%;
      object-fit: cover;
      transition: transform 0.2s ease;
    }

    .news-card:hover .news-image img {
      transform: scale(1.05);
    }

    .news-content {
      padding: 20px;
    }

    .news-meta {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 10px;
      font-size: 0.9em;
      color: #666;
      flex-wrap: wrap;
      gap: 10px;
    }

    .news-source {
      font-weight: 600;
      color: #667eea;
    }

    .article-language {
      font-size: 1.2em;
    }

    .news-title {
      margin: 0 0 15px 0;
      font-size: 1.2em;
      line-height: 1.4;
    }

    .news-title a {
      color: #333;
      text-decoration: none;
      transition: color 0.2s ease;
    }

    .news-title a:hover {
      color: #667eea;
    }

    .news-description {
      color: #666;
      line-height: 1.6;
      margin-bottom: 15px;
    }

    .news-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .news-author {
      font-size: 0.9em;
      color: #888;
    }

    .read-more-btn {
      color: #667eea;
      text-decoration: none;
      font-weight: 500;
      transition: color 0.2s ease;
    }

    .read-more-btn:hover {
      color: #5a6fd8;
      text-decoration: underline;
    }

    /* ✅ NUEVOS: Estilos para tips de personalización */
    .personalization-tips {
      background: white;
      padding: 20px;
      border-radius: 12px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
      margin-top: 30px;
      border-left: 4px solid #667eea;
    }

    .personalization-tips h3 {
      margin: 0 0 15px 0;
      color: #333;
    }

    .personalization-tips ul {
      margin: 0;
      padding-left: 20px;
    }

    .personalization-tips li {
      margin-bottom: 8px;
      color: #666;
    }

    .personalization-tips a {
      color: #667eea;
      text-decoration: none;
    }

    .personalization-tips a:hover {
      text-decoration: underline;
    }

    @media (max-width: 768px) {
      .news-container {
        padding: 10px;
      }
      
      .news-header {
        flex-direction: column;
        gap: 15px;
      }
      
      .header-actions {
        width: 100%;
        justify-content: center;
      }
      
      .search-bar {
        flex-direction: column;
      }
      
      .news-grid {
        grid-template-columns: 1fr;
      }
      
      .language-info {
        align-items: center;
        text-align: center;
      }
    }
  `]
})
export class NewsListComponent implements OnInit {
  articles: NewsArticleDto[] = [];
  searchQuery: string = '';
  selectedCategory: string = '';
  loading: boolean = false;
  testing: boolean = false;
  errorMessage: string = '';
  connectionSuccess: boolean = false;
  currentUser: CurrentUser = { isAuthenticated: false, roles: [] }; // ✅ NUEVO

  constructor(
    private newsService: NewsService,
    private authService: AuthService, // ✅ NUEVO
    private languageService: LanguageService // ✅ NUEVO
  ) {}

  ngOnInit(): void {
    // ✅ NUEVO: Suscribirse a cambios del usuario
    this.authService.currentUser$.subscribe((user: CurrentUser) => {
      this.currentUser = user;
    });

    this.loadNews();
  }

  // ✅ MODIFICADO: Usar método personalizado que considera idioma del usuario
  loadNews(): void {
    this.loading = true;
    this.errorMessage = '';
    
    // Usar el nuevo método personalizado que incluye idioma preferido
    this.newsService.getPersonalizedNews(20).subscribe({
      next: (articles: NewsArticleDto[]) => {
        this.articles = articles;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading news:', error);
        this.errorMessage = 'Failed to load news. Please check your connection and try again.';
        this.loading = false;
      }
    });
  }

  // ✅ MODIFICADO: Usar búsqueda personalizada
  searchNews(): void {
    if (!this.searchQuery.trim()) {
      this.loadNews();
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    
    // Usar el nuevo método de búsqueda personalizada
    this.newsService.searchPersonalizedNews(this.searchQuery, this.selectedCategory).subscribe({
      next: (result: PagedResultDto<NewsArticleDto>) => {
        this.articles = result.items;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error searching news:', error);
        this.errorMessage = 'Failed to search news. Please try again.';
        this.loading = false;
      }
    });
  }

  // ✅ MODIFICADO: Usar titulares personalizados
  onCategoryChange(): void {
    this.loading = true;
    this.errorMessage = '';
    
    // Usar el nuevo método de titulares personalizados
    this.newsService.getPersonalizedHeadlines(this.selectedCategory).subscribe({
      next: (result: PagedResultDto<NewsArticleDto>) => {
        this.articles = result.items;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading category news:', error);
        this.errorMessage = 'Failed to load category news. Please try again.';
        this.loading = false;
      }
    });
  }

  refreshNews(): void {
    this.searchQuery = '';
    this.selectedCategory = '';
    this.loadNews();
  }

  testConnection(): void {
    this.testing = true;
    this.connectionSuccess = false;
    this.errorMessage = '';
    
    this.newsService.testConnection().subscribe({
      next: (success: boolean) => {
        this.connectionSuccess = success;
        this.testing = false;
        
        if (success) {
          setTimeout(() => {
            this.connectionSuccess = false;
          }, 3000);
        } else {
          this.errorMessage = 'Backend connection test failed';
        }
      },
      error: (error: any) => {
        console.error('Error testing connection:', error);
        this.errorMessage = 'Backend connection failed';
        this.testing = false;
      }
    });
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  // ✅ NUEVOS: Métodos para idiomas
  getCurrentLanguageName(): string {
    const languageCode = this.authService.getPreferredLanguage();
    return this.languageService.getLanguageName(languageCode);
  }

  getCurrentLanguageFlag(): string {
    const languageCode = this.authService.getPreferredLanguage();
    return this.languageService.getLanguageFlag(languageCode);
  }

  getArticleLanguageFlag(languageCode: string): string {
    return this.languageService.getLanguageFlag(languageCode);
  }
}
