import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NewsService } from '../../../core/services/news.service';
import { ReadingListService } from '../../../core/services/reading-list.service';
import { UserProfileService } from '../../../core/services/user-profile.service';
import { NewsArticleDto } from '../../../shared/models/news.model';
import { ReadingListDto, SaveArticleDto, SavedArticleDto, CreateReadingListDto } from '../../../shared/models/reading-list.model';

@Component({
  selector: 'app-news-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="news-container">
      <div class="header-section">
        <h2>Latest News</h2>
        
        <!-- Reading Lists Quick Access -->
        <div class="reading-lists-section" *ngIf="readingLists.length > 0">
          <h4>My Reading Lists</h4>
          <div class="reading-lists-bar">
            <div class="reading-list-item" 
                 *ngFor="let list of readingLists" 
                 (click)="viewReadingList(list.id)"
                 [title]="list.description || list.name">
              <span class="list-name">{{ list.name }}</span>
              <span class="list-count">{{ list.unreadCount }}/{{ list.articleCount }}</span>
            </div>
            <button (click)="showCreateListModal = true" class="add-list-btn" title="Create new reading list">+</button>
          </div>
        </div>
        
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
              <div class="save-actions">
                <button 
                  (click)="saveArticle(article)" 
                  class="save-btn"
                  [class.saved]="isArticleSaved(article.url)"
                  [disabled]="savingArticles.has(article.url)">
                  {{ getSaveButtonText(article.url) }}
                </button>
                <button 
                  *ngIf="readingLists.length > 0"
                  (click)="openSaveToListsModal(article)"
                  class="save-to-lists-btn"
                  [disabled]="savingArticles.has(article.url)">
                  {{ isArticleSaved(article.url) ? 'Manage Lists' : 'Save to List(s)' }}
                </button>
              </div>
              <span class="author" *ngIf="article.author">By {{ article.author }}</span>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Load More Button -->
      <div class="load-more-section" *ngIf="!loading && articles.length > 0 && hasMoreNews">
        <button (click)="loadMoreNews()" class="load-more-btn" [disabled]="loadingMore">
          {{ loadingMore ? 'Loading...' : 'Load More News' }}
        </button>
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

      <!-- Create List Modal -->
      <div class="modal" *ngIf="showCreateListModal" (click)="closeCreateModal($event)">
        <div class="modal-content" (click)="$event.stopPropagation()">
          <h3>Create New Reading List</h3>
          <div class="form-group">
            <label>Name:</label>
            <input type="text" [(ngModel)]="newListName" placeholder="Enter list name" class="form-input">
          </div>
          <div class="form-group">
            <label>Description:</label>
            <textarea [(ngModel)]="newListDescription" placeholder="Optional description" class="form-textarea"></textarea>
          </div>
          <div class="form-group">
            <label>
              <input type="checkbox" [(ngModel)]="newListIsPublic"> Make public
            </label>
          </div>
          <div class="modal-actions">
            <button (click)="createReadingList()" class="create-btn" [disabled]="!newListName.trim() || creatingList">
              {{ creatingList ? 'Creating...' : 'Create' }}
            </button>
            <button (click)="cancelCreateList()" class="cancel-btn">Cancel</button>
          </div>
        </div>
      </div>

      <!-- Save to Lists Modal -->
      <div class="modal" *ngIf="showSaveToListsModal" (click)="closeSaveToListsModal()">
        <div class="modal-content save-to-lists-modal" (click)="$event.stopPropagation()">
          <h3>📋 Save to Reading Lists</h3>
          <p class="modal-subtitle">Select one or more lists to save this article</p>
          
          <div class="lists-checkboxes" *ngIf="readingLists.length > 0">
            <label *ngFor="let list of readingLists" class="checkbox-item">
              <input 
                type="checkbox" 
                [checked]="selectedListIds.has(list.id)"
                (change)="toggleListSelection(list.id)">
              <span class="checkbox-label">
                <span class="list-name">{{ list.name }}</span>
                <span class="list-count">({{ list.articleCount }} articles)</span>
              </span>
            </label>
          </div>

          <div class="no-lists" *ngIf="readingLists.length === 0">
            <p>You don't have any reading lists yet.</p>
            <button (click)="createListFromSaveModal()" class="create-list-link">
              ➕ Create your first list
            </button>
          </div>

          <div class="modal-actions">
            <button 
              (click)="saveToMultipleLists()" 
              class="save-btn" 
              [disabled]="selectedListIds.size === 0 || savingToMultipleLists">
              <ng-container *ngIf="savingToMultipleLists">💾 Saving...</ng-container>
              <ng-container *ngIf="!savingToMultipleLists">💾 Save to {{ selectedListIds.size }} list<span *ngIf="selectedListIds.size !== 1">s</span></ng-container>
            </button>
            <button (click)="closeSaveToListsModal()" class="cancel-btn">❌ Cancel</button>
          </div>
        </div>
      </div>

      <!-- Success notification -->
      <div class="notification success" *ngIf="showSuccessNotification">
        <span>✅ {{ successMessage }}</span>
        <button (click)="showSuccessNotification = false" class="close-notification">×</button>
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

    /* Reading Lists Section */
    .reading-lists-section {
      margin-bottom: 20px;
      padding: 15px;
      background: #f8f9fa;
      border-radius: 8px;
      border: 1px solid #e9ecef;
    }

    .reading-lists-section h4 {
      margin: 0 0 10px 0;
      color: #495057;
      font-size: 1em;
    }

    .reading-lists-bar {
      display: flex;
      gap: 10px;
      flex-wrap: wrap;
      align-items: center;
    }

    .reading-list-item {
      padding: 6px 12px;
      background: white;
      border: 1px solid #dee2e6;
      border-radius: 15px;
      cursor: pointer;
      transition: all 0.2s ease;
      font-size: 0.9em;
    }

    .reading-list-item:hover {
      background: #007bff;
      color: white;
      border-color: #007bff;
      transform: translateY(-1px);
    }

    .list-name {
      margin-right: 8px;
      font-weight: 500;
    }

    .list-count {
      font-size: 0.8em;
      opacity: 0.8;
      background: rgba(0,0,0,0.1);
      padding: 2px 6px;
      border-radius: 10px;
    }

    .add-list-btn {
      width: 30px;
      height: 30px;
      border-radius: 50%;
      border: 2px dashed #007bff;
      background: white;
      color: #007bff;
      font-size: 16px;
      cursor: pointer;
      transition: all 0.2s ease;
    }

    .add-list-btn:hover {
      background: #007bff;
      color: white;
      transform: scale(1.1);
    }

    /* Save Actions */
    .save-actions {
      display: flex;
      gap: 5px;
      align-items: center;
      flex-wrap: wrap;
    }

    .save-btn {
      padding: 6px 12px;
      background: #28a745;
      color: white;
      border: none;
      border-radius: 15px;
      cursor: pointer;
      font-size: 0.8em;
      transition: all 0.2s ease;
    }

    .save-btn:hover {
      background: #218838;
      transform: translateY(-1px);
    }

    .save-btn.saved {
      background: #6c757d;
      cursor: not-allowed;
    }

    .save-btn:disabled {
      background: #6c757d;
      cursor: not-allowed;
    }

    .list-select {
      font-size: 0.8em;
      padding: 4px 8px;
      border: 1px solid #ced4da;
      border-radius: 4px;
      background: white;
    }

    /* Modal Styles */
    .modal {
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background: rgba(0,0,0,0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
    }

    .modal-content {
      background: white;
      padding: 25px;
      border-radius: 8px;
      width: 90%;
      max-width: 400px;
      box-shadow: 0 10px 30px rgba(0,0,0,0.3);
    }

    .modal-content h3 {
      margin-top: 0;
      color: #343a40;
    }

    .form-group {
      margin-bottom: 15px;
    }

    .form-group label {
      display: block;
      margin-bottom: 5px;
      font-weight: 500;
    }

    .form-input, .form-textarea {
      width: 100%;
      padding: 8px 12px;
      border: 1px solid #ced4da;
      border-radius: 4px;
      font-size: 14px;
    }

    .form-textarea {
      resize: vertical;
      height: 60px;
    }

    .modal-actions {
      display: flex;
      gap: 10px;
      justify-content: flex-end;
      margin-top: 20px;
    }

    .create-btn, .cancel-btn {
      padding: 8px 16px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 14px;
    }

    .create-btn {
      background: #007bff;
      color: white;
    }

    .create-btn:hover {
      background: #0056b3;
    }

    .create-btn:disabled {
      background: #6c757d;
      cursor: not-allowed;
    }

    .cancel-btn {
      background: #6c757d;
      color: white;
    }

    .cancel-btn:hover {
      background: #545b62;
    }

    /* Save to Lists Modal Styles */
    .save-to-lists-modal {
      max-width: 500px;
    }

    .modal-subtitle {
      color: #6c757d;
      font-size: 14px;
      margin: -5px 0 15px 0;
    }

    .lists-checkboxes {
      max-height: 300px;
      overflow-y: auto;
      border: 1px solid #e0e0e0;
      border-radius: 4px;
      padding: 10px;
      margin-bottom: 20px;
    }

    .checkbox-item {
      display: flex;
      align-items: center;
      padding: 10px;
      margin-bottom: 5px;
      border-radius: 4px;
      cursor: pointer;
      transition: background-color 0.2s;
    }

    .checkbox-item:hover {
      background-color: #f8f9fa;
    }

    .checkbox-item input[type="checkbox"] {
      margin-right: 10px;
      cursor: pointer;
      width: 18px;
      height: 18px;
    }

    .checkbox-label {
      display: flex;
      align-items: center;
      gap: 8px;
      flex: 1;
    }

    .list-name {
      font-weight: 500;
      color: #343a40;
    }

    .list-count {
      color: #6c757d;
      font-size: 13px;
    }

    .no-lists {
      text-align: center;
      padding: 30px;
      color: #6c757d;
    }

    .no-lists p {
      margin-bottom: 15px;
    }

    .create-list-link {
      background: #28a745;
      color: white;
      border: none;
      padding: 10px 20px;
      border-radius: 4px;
      cursor: pointer;
      font-size: 14px;
    }

    .create-list-link:hover {
      background: #218838;
    }

    .save-to-lists-btn {
      background: #007bff;
      color: white;
      padding: 6px 12px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 12px;
      transition: background-color 0.2s;
    }

    .save-to-lists-btn:hover {
      background: #0056b3;
    }

    .save-to-lists-btn:disabled {
      background: #6c757d;
      cursor: not-allowed;
    }

    .save-btn {
      background: #28a745;
      color: white;
    }

    .save-btn:hover {
      background: #218838;
    }

    .save-btn:disabled {
      background: #6c757d;
      cursor: not-allowed;
    }

    /* Notification */
    .notification {
      position: fixed;
      top: 20px;
      right: 20px;
      padding: 15px 20px;
      border-radius: 8px;
      z-index: 1001;
      display: flex;
      align-items: center;
      gap: 10px;
      max-width: 500px;
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
      animation: slideIn 0.3s ease-out;
    }

    @keyframes slideIn {
      from {
        transform: translateX(100%);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }

    .notification.success {
      background: #d4edda;
      color: #155724;
      border: 1px solid #c3e6cb;
    }

    .notification span {
      flex: 1;
      word-wrap: break-word;
      line-height: 1.5;
    }

    .close-notification {
      background: none;
      border: none;
      font-size: 24px;
      cursor: pointer;
      color: #155724;
      padding: 0;
      width: 24px;
      height: 24px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    .close-notification:hover {
      opacity: 0.7;
    }

    /* Rest of existing styles... */
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
    
    .load-more-section {
      text-align: center;
      padding: 30px 20px;
    }
    
    .load-more-btn {
      padding: 12px 40px;
      background: #007bff;
      color: white;
      border: none;
      border-radius: 5px;
      cursor: pointer;
      font-size: 16px;
      font-weight: 500;
      transition: background 0.2s ease;
    }
    
    .load-more-btn:hover:not(:disabled) {
      background: #0056b3;
    }
    
    .load-more-btn:disabled {
      background: #6c757d;
      cursor: not-allowed;
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
      flex-wrap: wrap;
      gap: 10px;
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
  userLanguage = 'en'; // Default to English, will be updated from user profile
  
  // Pagination properties
  currentPage = 1;
  pageSize = 10;
  hasMoreNews = true;
  loadingMore = false;

  // Reading list properties
  readingLists: ReadingListDto[] = [];
  savedArticleUrls = new Set<string>();
  savingArticles = new Set<string>();
  
  // Modal properties
  showCreateListModal = false;
  showSaveToListsModal = false;
  selectedArticleForSave: NewsArticleDto | null = null;
  selectedListIds = new Set<string>();
  originalListIds = new Set<string>(); // Track original lists for saved articles
  newListName = '';
  newListDescription = '';
  newListIsPublic = false;
  creatingList = false;
  savingToMultipleLists = false;

  // Notification properties
  showSuccessNotification = false;
  successMessage = '';

  constructor(
    private newsService: NewsService,
    private readingListService: ReadingListService,
    private userProfileService: UserProfileService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadReadingLists();
    this.loadSavedArticles();
    this.loadUserLanguage();
  }

  loadUserLanguage() {
    this.userProfileService.getMyProfile().subscribe({
      next: (profile) => {
        this.userLanguage = profile.newsLanguageCode || 'en';
        console.log('User preferred language:', this.userLanguage);
        // Load news with correct language
        if (this.selectedCategory) {
          this.loadByCategory();
        } else {
          this.loadLatestNews();
        }
      },
      error: (err) => {
        console.error('Error loading user language preference:', err);
        this.userLanguage = 'en'; // Fallback to English
        // Still load news even if profile fails
        this.loadLatestNews();
      }
    });
  }

  loadReadingLists() {
    this.readingListService.getMyReadingLists().subscribe({
      next: (lists: ReadingListDto[]) => {
        this.readingLists = lists;
      },
      error: (err: any) => {
        console.error('Error loading reading lists:', err);
      }
    });
  }

  loadSavedArticles() {
    this.readingListService.getMySavedArticles().subscribe({
      next: (articles: SavedArticleDto[]) => {
        this.savedArticleUrls = new Set(articles.map(a => a.url));
      },
      error: (err: any) => {
        console.error('Error loading saved articles:', err);
      }
    });
  }

  isArticleSaved(url: string): boolean {
    return this.savedArticleUrls.has(url);
  }

  getSaveButtonText(url: string): string {
    if (this.savingArticles.has(url)) {
      return '💾 Saving...';
    }
    return this.isArticleSaved(url) ? '✅ Saved' : '💾 Save for Later';
  }

  saveArticle(article: NewsArticleDto) {
    if (this.isArticleSaved(article.url) || this.savingArticles.has(article.url)) {
      return;
    }

    this.savingArticles.add(article.url);

    const saveData: SaveArticleDto = {
      source: article.source || '',
      title: article.title,
      description: article.description,
      url: article.url,
      urlToImage: article.urlToImage,
      publishedAt: article.publishedAt,
      content: article.content,
      languageCode: article.languageCode || 'en',
      author: article.author
    };

    this.readingListService.saveArticle(saveData).subscribe({
      next: () => {
        this.savedArticleUrls.add(article.url);
        this.savingArticles.delete(article.url);
        this.loadReadingLists(); // Refresh to update counts
        this.showSuccessMessage('Article saved for later reading!');
      },
      error: (err: any) => {
        console.error('Error saving article:', err);
        this.savingArticles.delete(article.url);
        this.showErrorMessage('Failed to save article. Please try again.');
      }
    });
  }

  saveToList(article: NewsArticleDto, event: any) {
    const listId = event.target.value;
    if (!listId || this.savingArticles.has(article.url)) {
      return;
    }

    this.savingArticles.add(article.url);

    const saveData: SaveArticleDto = {
      readingListId: listId,
      source: article.source || '',
      title: article.title,
      description: article.description,
      url: article.url,
      urlToImage: article.urlToImage,
      publishedAt: article.publishedAt,
      content: article.content,
      languageCode: article.languageCode || 'en',
      author: article.author
    };

    this.readingListService.saveArticle(saveData).subscribe({
      next: () => {
        this.savedArticleUrls.add(article.url);
        this.savingArticles.delete(article.url);
        this.loadReadingLists(); // Refresh to update counts
        const listName = this.readingLists.find(l => l.id === listId)?.name || 'list';
        this.showSuccessMessage(`Article saved to "${listName}"!`);
        event.target.value = ''; // Reset dropdown
      },
      error: (err: any) => {
        console.error('Error saving article to list:', err);
        this.savingArticles.delete(article.url);
        this.showErrorMessage('Failed to save article to list. Please try again.');
        event.target.value = ''; // Reset dropdown
      }
    });
  }

  createReadingList() {
    if (!this.newListName.trim() || this.creatingList) {
      return;
    }

    this.creatingList = true;

    const createData: CreateReadingListDto = {
      name: this.newListName.trim(),
      description: this.newListDescription.trim() || undefined,
      isPublic: this.newListIsPublic,
      sortOrder: this.readingLists.length
    };

    this.readingListService.createReadingList(createData).subscribe({
      next: (newList) => {
        this.loadReadingLists();
        this.cancelCreateList();
        this.showSuccessMessage(`Reading list "${newList.name}" created!`);
      },
      error: (err: any) => {
        console.error('Error creating reading list:', err);
        this.creatingList = false;
        this.showErrorMessage('Failed to create reading list. Please try again.');
      }
    });
  }

  cancelCreateList() {
    this.showCreateListModal = false;
    this.newListName = '';
    this.newListDescription = '';
    this.newListIsPublic = false;
    this.creatingList = false;
  }

  closeCreateModal(event: any) {
    if (event.target === event.currentTarget) {
      this.cancelCreateList();
    }
  }

  viewReadingList(listId: string) {
    // Navigate to reading lists page with the specific list selected
    this.router.navigate(['/reading-lists'], { queryParams: { list: listId } });
  }

  showSuccessMessage(message: string) {
    this.successMessage = message;
    this.showSuccessNotification = true;
    // Show notification for 5 seconds (increased from 3) to allow reading longer messages
    setTimeout(() => {
      this.showSuccessNotification = false;
    }, 5000);
  }

  showErrorMessage(message: string) {
    // You could implement error notifications similarly
    console.error(message);
  }

  getImageSrc(article: NewsArticleDto): string {
    return article.urlToImage || 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="300" height="200" viewBox="0 0 300 200"%3E%3Crect width="100%25" height="100%25" fill="%23f0f0f0"%3E%3C/rect%3E%3Ctext x="50%25" y="50%25" font-family="Arial" font-size="14" fill="%23999" text-anchor="middle" dy=".3em"%3ENo Image%3C/text%3E%3C/svg%3E';
  }

  loadLatestNews() {
    this.loading = true;
    this.error = null;
    this.currentPage = 1;
    
    // Use getTopHeadlines without category for consistency with pagination
    this.newsService.getTopHeadlines(undefined, undefined, this.userLanguage, this.currentPage, this.pageSize).subscribe({
      next: (result: any) => {
        this.articles = result.items;
        this.hasMoreNews = result.items.length === this.pageSize;
        this.loading = false;
        console.log('Loaded news:', result.items.length, 'hasMoreNews:', this.hasMoreNews);
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
    this.currentPage = 1;
    
    this.newsService.getTopHeadlines(this.selectedCategory, undefined, this.userLanguage, this.currentPage, this.pageSize).subscribe({
      next: (result: any) => {
        this.articles = result.items;
        this.hasMoreNews = result.items.length === this.pageSize;
        this.loading = false;
        console.log('Loaded category news:', result.items.length, 'hasMoreNews:', this.hasMoreNews);
      },
      error: (err: any) => {
        this.handleError(err);
      }
    });
  }

  loadMoreNews() {
    if (this.loadingMore || !this.hasMoreNews) return;
    
    this.loadingMore = true;
    this.currentPage++;
    
    // Use getTopHeadlines for pagination, with or without category
    this.newsService.getTopHeadlines(this.selectedCategory || undefined, undefined, this.userLanguage, this.currentPage, this.pageSize).subscribe({
      next: (result: any) => {
        this.articles = [...this.articles, ...result.items];
        this.hasMoreNews = result.items.length === this.pageSize;
        this.loadingMore = false;
      },
      error: (err: any) => {
        this.loadingMore = false;
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

  // Multi-list save methods
  openSaveToListsModal(article: NewsArticleDto) {
    this.selectedArticleForSave = article;
    this.selectedListIds.clear();
    this.originalListIds.clear();
    this.showSaveToListsModal = true;
    
    // If article is already saved, load the lists where it's saved
    if (this.isArticleSaved(article.url)) {
      this.readingListService.getReadingListIdsForArticle(article.url).subscribe({
        next: (listIds: string[]) => {
          listIds.forEach(id => {
            this.selectedListIds.add(id);
            this.originalListIds.add(id); // Track original state
          });
        },
        error: (err: any) => {
          console.error('Error loading lists for article:', err);
        }
      });
    }
  }

  closeSaveToListsModal() {
    this.showSaveToListsModal = false;
    this.selectedArticleForSave = null;
    this.selectedListIds.clear();
    this.originalListIds.clear();
    this.savingToMultipleLists = false;
  }

  toggleListSelection(listId: string) {
    if (this.selectedListIds.has(listId)) {
      this.selectedListIds.delete(listId);
    } else {
      this.selectedListIds.add(listId);
    }
  }

  async saveToMultipleLists() {
    if (!this.selectedArticleForSave || this.savingToMultipleLists) {
      return;
    }

    // Check if no selection - only allow if article wasn't saved before
    if (this.selectedListIds.size === 0 && this.originalListIds.size === 0) {
      this.showErrorMessage('Debes seleccionar al menos una lista.');
      return;
    }

    this.savingToMultipleLists = true;
    const article = this.selectedArticleForSave;

    // Determine which lists to add to and which to remove from
    const listsToAdd = Array.from(this.selectedListIds).filter(id => !this.originalListIds.has(id));
    const listsToRemove = Array.from(this.originalListIds).filter(id => !this.selectedListIds.has(id));

    const addedLists: string[] = [];
    const removedLists: string[] = [];
    const failedLists: string[] = [];
    
    // Add to new lists
    for (const listId of listsToAdd) {
      const listName = this.readingLists.find(l => l.id === listId)?.name || 'Unknown';
      
      try {
        const saveData: SaveArticleDto = {
          readingListId: listId,
          source: article.source || '',
          title: article.title,
          description: article.description,
          url: article.url,
          urlToImage: article.urlToImage,
          publishedAt: article.publishedAt,
          content: article.content,
          languageCode: article.languageCode || 'en',
          author: article.author
        };
        
        await this.readingListService.saveArticle(saveData).toPromise();
        addedLists.push(listName);
        
        await new Promise(resolve => setTimeout(resolve, 100));
        
      } catch (err: any) {
        console.error(`❌ Error agregando a ${listName}:`, err);
        failedLists.push(listName);
      }
    }

    // Remove from unselected lists
    for (const listId of listsToRemove) {
      const listName = this.readingLists.find(l => l.id === listId)?.name || 'Unknown';
      
      try {
        await this.readingListService.unsaveArticleFromList(article.url, listId).toPromise();
        removedLists.push(listName);
        
        await new Promise(resolve => setTimeout(resolve, 100));
        
      } catch (err: any) {
        console.error(`❌ Error eliminando de ${listName}:`, err);
        failedLists.push(listName);
      }
    }

    // Update UI
    const hasChanges = addedLists.length > 0 || removedLists.length > 0;
    
    if (hasChanges) {
      // Update saved state
      if (this.selectedListIds.size > 0) {
        this.savedArticleUrls.add(article.url);
      } else {
        this.savedArticleUrls.delete(article.url);
      }
      
      this.loadReadingLists(); // Refresh to update counts
      
      // Create a descriptive success message
      let messages: string[] = [];
      
      if (addedLists.length > 0) {
        const addedText = addedLists.length === 1 
          ? addedLists[0]
          : addedLists.slice(0, -1).join(', ') + ' y ' + addedLists[addedLists.length - 1];
        messages.push(`Agregado a: ${addedText}`);
      }
      
      if (removedLists.length > 0) {
        const removedText = removedLists.length === 1 
          ? removedLists[0]
          : removedLists.slice(0, -1).join(', ') + ' y ' + removedLists[removedLists.length - 1];
        messages.push(`Eliminado de: ${removedText}`);
      }
      
      let message = `"${article.title}" - ${messages.join(' | ')}`;
      
      if (failedLists.length > 0) {
        message += ` (Errores en: ${failedLists.join(', ')})`;
      }
      
      this.showSuccessMessage(message);
    } else if (failedLists.length > 0) {
      this.showErrorMessage('No se pudieron realizar los cambios solicitados.');
    } else {
      this.showSuccessMessage('No hay cambios que guardar.');
    }
    
    this.closeSaveToListsModal();
  }

  createListFromSaveModal() {
    // Close the save modal and open the create modal
    this.closeSaveToListsModal();
    this.showCreateListModal = true;
  }
}
