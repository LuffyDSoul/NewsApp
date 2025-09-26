import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NewsService } from '../../../core/services/news.service';
import { NewsArticleDto } from '../../../shared/models/news.model';

@Component({
  selector: 'app-news-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="news-container">
      <div class="header-section">
        <h2>Latest News</h2>
        
        <div class="search-section">
          <input 
            type="text" 
            [(ngModel)]="searchQuery" 
            placeholder="Search news..." 
            class="search-input"
            (keyup.enter)="searchNews()">
          <button (click)="searchNews()" class="search-btn" [disabled]="loading">
            {{ loading ? 'Searching...' : 'Search' }}
          </button>
          <button (click)="loadLatestNews()" class="refresh-btn" [disabled]="loading">
            Refresh
          </button>
        </div>

        <div class="filter-section">
          <label>Category:</label>
          <select [(ngModel)]="selectedCategory" (change)="loadByCategory()" class="category-select">
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
      
      <div class="loading" *ngIf="loading">
        <div class="spinner"></div>
        Loading news...
      </div>
      
      <div class="news-grid" *ngIf="!loading && articles.length > 0">
        <div class="news-card" *ngFor="let article of articles; trackBy: trackByUrl">
          <img [src]="getImageSrc(article)" [alt]="article.title" class="news-image" (error)="onImageError($event)"/>
          <div class="news-content">
            <h3 [title]="article.title">{{ article.title }}</h3>
            <p class="news-description" [title]="article.description">
              {{ article.description || 'No description available' }}
            </p>
            <div class="news-meta">
              <span class="source" [title]="article.source">{{ article.source }}</span>
              <span class="date">{{ article.publishedAt | date:'short' }}</span>
            </div>
            <div class="news-actions">
              <a [href]="article.url" target="_blank" rel="noopener noreferrer" class="read-more">
                Read Full Article
              </a>
              <span class="author" *ngIf="article.author">By {{ article.author }}</span>
            </div>
          </div>
        </div>
      </div>
      
      <div class="no-results" *ngIf="!loading && articles.length === 0 && !error">
        <h3>No news found</h3>
        <p>Try adjusting your search terms or refresh for latest news.</p>
      </div>
      
      <div class="error" *ngIf="error">
        <h3>⚠️ Error Loading News</h3>
        <p>{{ error }}</p>
        <button (click)="loadLatestNews()" class="retry-btn">Try Again</button>
        <button (click)="testConnection()" class="test-btn">Test Connection</button>
      </div>

      <div class="connection-status" *ngIf="connectionTested">
        <p [class]="connectionStatus ? 'success' : 'failure'">
          {{ connectionStatus ? '✅ Backend connection successful' : '❌ Backend connection failed' }}
        </p>
      </div>
    </div>
  `,
  styles: [`
    .news-container {
      padding: 20px;
      max-width: 1200px;
      margin: 0 auto;
    }

    .header-section {
      margin-bottom: 30px;
    }

    .header-section h2 {
      color: #343a40;
      margin-bottom: 20px;
    }

    .search-section {
      display: flex;
      gap: 10px;
      margin-bottom: 15px;
      align-items: center;
    }

    .search-input {
      flex: 1;
      padding: 10px 15px;
      border: 2px solid #e9ecef;
      border-radius: 5px;
      font-size: 16px;
    }

    .search-input:focus {
      outline: none;
      border-color: #007bff;
    }

    .search-btn, .refresh-btn, .retry-btn, .test-btn {
      padding: 10px 20px;
      background: #007bff;
      color: white;
      border: none;
      border-radius: 5px;
      cursor: pointer;
      font-size: 14px;
      transition: background 0.2s ease;
    }

    .search-btn:hover, .refresh-btn:hover, .retry-btn:hover, .test-btn:hover {
      background: #0056b3;
    }

    .search-btn:disabled, .refresh-btn:disabled {
      background: #6c757d;
      cursor: not-allowed;
    }

    .filter-section {
      display: flex;
      gap: 10px;
      align-items: center;
    }

    .category-select {
      padding: 8px 12px;
      border: 1px solid #ced4da;
      border-radius: 4px;
      font-size: 14px;
    }

    .loading {
      text-align: center;
      padding: 40px 20px;
      font-size: 1.1em;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 10px;
    }

    .spinner {
      width: 20px;
      height: 20px;
      border: 2px solid #f3f3f3;
      border-top: 2px solid #007bff;
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
    
    .news-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
      gap: 25px;
    }
    
    .news-card {
      border: 1px solid #e9ecef;
      border-radius: 12px;
      overflow: hidden;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
      transition: transform 0.3s ease, box-shadow 0.3s ease;
      background: white;
    }
    
    .news-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 8px 25px rgba(0,0,0,0.15);
    }
    
    .news-image {
      width: 100%;
      height: 200px;
      object-fit: cover;
    }
    
    .news-content {
      padding: 20px;
    }
    
    .news-content h3 {
      margin: 0 0 12px 0;
      font-size: 1.2em;
      line-height: 1.4;
      color: #212529;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }
    
    .news-description {
      color: #6c757d;
      font-size: 0.95em;
      line-height: 1.5;
      margin: 12px 0;
      display: -webkit-box;
      -webkit-line-clamp: 3;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }
    
    .news-meta {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin: 15px 0;
      font-size: 0.85em;
      color: #868e96;
    }

    .source {
      font-weight: 600;
      color: #495057;
    }
    
    .news-actions {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: 15px;
    }

    .read-more {
      color: #007bff;
      text-decoration: none;
      font-weight: 600;
      font-size: 0.9em;
      padding: 8px 16px;
      border: 2px solid #007bff;
      border-radius: 20px;
      transition: all 0.2s ease;
    }
    
    .read-more:hover {
      background: #007bff;
      color: white;
      text-decoration: none;
    }

    .author {
      font-size: 0.8em;
      color: #868e96;
      font-style: italic;
    }
    
    .error {
      text-align: center;
      padding: 30px;
      background: #f8d7da;
      border: 1px solid #f5c6cb;
      border-radius: 8px;
      color: #721c24;
    }

    .error h3 {
      margin-bottom: 15px;
    }

    .no-results {
      text-align: center;
      padding: 40px 20px;
      color: #6c757d;
    }

    .connection-status {
      margin-top: 20px;
      text-align: center;
      padding: 10px;
      border-radius: 5px;
    }

    .connection-status .success {
      color: #155724;
      background: #d4edda;
    }

    .connection-status .failure {
      color: #721c24;
      background: #f8d7da;
    }
  `]
})
export class NewsListComponent implements OnInit {
  articles: NewsArticleDto[] = [];
  loading = false;
  error: string | null = null;
  searchQuery = '';
  selectedCategory = '';
  connectionTested = false;
  connectionStatus = false;

  constructor(private newsService: NewsService) {}

  ngOnInit() {
    this.loadLatestNews();
  }

  getImageSrc(article: NewsArticleDto): string {
    return article.urlToImage || 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="300" height="200" viewBox="0 0 300 200"%3E%3Crect width="100%25" height="100%25" fill="%23f0f0f0"%3E%3C/rect%3E%3Ctext x="50%25" y="50%25" font-family="Arial" font-size="14" fill="%23999" text-anchor="middle" dy=".3em"%3ENo Image%3C/text%3E%3C/svg%3E';
  }

  loadLatestNews() {
    this.loading = true;
    this.error = null;
    
    this.newsService.getLatestNews(20).subscribe({
      next: (articles: NewsArticleDto[]) => {
        this.articles = articles;
        this.loading = false;
      },
      error: (err: any) => {
        this.handleError(err);
      }
    });
  }

  searchNews() {
    if (!this.searchQuery.trim()) {
      this.loadLatestNews();
      return;
    }

    this.loading = true;
    this.error = null;
    
    this.newsService.searchLocalNews(this.searchQuery, undefined, 0, 20).subscribe({
      next: (result: any) => {
        this.articles = result.items;
        this.loading = false;
      },
      error: (err: any) => {
        this.handleError(err);
      }
    });
  }

  loadByCategory() {
    this.loading = true;
    this.error = null;
    
    this.newsService.getTopHeadlines(this.selectedCategory, undefined, 'en', 1, 20).subscribe({
      next: (result: any) => {
        this.articles = result.items;
        this.loading = false;
      },
      error: (err: any) => {
        this.handleError(err);
      }
    });
  }

  testConnection() {
    this.newsService.testConnection().subscribe({
      next: (status: boolean) => {
        this.connectionTested = true;
        this.connectionStatus = status;
      },
      error: () => {
        this.connectionTested = true;
        this.connectionStatus = false;
      }
    });
  }

  private handleError(err: any) {
    console.error('Error loading news:', err);
    this.error = err.error?.message || err.message || 'Failed to load news. Please check your connection and try again.';
    this.loading = false;
  }

  trackByUrl(index: number, article: NewsArticleDto): string {
    return article.url;
  }

  onImageError(event: any) {
    event.target.src = 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="300" height="200" viewBox="0 0 300 200"%3E%3Crect width="100%25" height="100%25" fill="%23f0f0f0"%3E%3C/rect%3E%3Ctext x="50%25" y="50%25" font-family="Arial" font-size="14" fill="%23999" text-anchor="middle" dy=".3em"%3ENo Image Available%3C/text%3E%3C/svg%3E';
  }
}
