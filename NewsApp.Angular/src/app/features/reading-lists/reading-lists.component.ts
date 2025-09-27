import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ReadingListService } from '../../core/services/reading-list.service';
import { 
  ReadingListDto, 
  SavedArticleDto, 
  SavedArticleStatsDto,
  CreateReadingListDto,
  UpdateReadingListDto
} from '../../shared/models/reading-list.model';

@Component({
  selector: 'app-reading-lists',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="reading-lists-container">
      <div class="header-section">
        <h2>📚 My Reading Lists</h2>
        <div class="stats-section" *ngIf="stats">
          <div class="stat-item">
            <span class="stat-number">{{ stats.totalCount }}</span>
            <span class="stat-label">Total Articles</span>
          </div>
          <div class="stat-item">
            <span class="stat-number">{{ stats.unreadCount }}</span>
            <span class="stat-label">Unread</span>
          </div>
          <div class="stat-item">
            <span class="stat-number">{{ stats.savedThisWeekCount }}</span>
            <span class="stat-label">Saved This Week</span>
          </div>
          <div class="stat-item">
            <span class="stat-number">{{ stats.readThisWeekCount }}</span>
            <span class="stat-label">Read This Week</span>
          </div>
        </div>
        <button (click)="showCreateModal = true" class="create-btn">
          ➕ Create New List
        </button>
      </div>

      <!-- All Saved Articles View -->
      <div class="all-articles-section">
        <h3>💾 All Saved Articles</h3>
        <div class="filters">
          <label class="filter-checkbox">
            <input type="checkbox" [(ngModel)]="showOnlyUnread" (change)="loadSavedArticles()">
            <span class="checkmark"></span>
            Show only unread ({{ stats?.unreadCount || 0 }})
          </label>
          <select [(ngModel)]="selectedListFilter" (change)="loadSavedArticles()" class="list-filter">
            <option value="">🗂️ All lists ({{ stats?.totalCount || 0 }})</option>
            <option *ngFor="let list of readingLists" [value]="list.id">
              📋 {{ list.name }} ({{ list.articleCount }})
            </option>
          </select>
        </div>
        
        <div class="articles-grid" *ngIf="savedArticles.length > 0">
          <div class="article-card" 
               *ngFor="let article of savedArticles" 
               [class.read]="article.isRead"
               [class.highlighted]="article.readingListId === highlightedListId">
            <img [src]="getImageSrc(article)" 
                 [alt]="article.title" 
                 class="article-image" 
                 (error)="onImageError($event)"/>
            <div class="article-content">
              <div class="article-header">
                <h4 [title]="article.title">{{ article.title }}</h4>
                <div class="article-meta">
                  <span class="source">📰 {{ article.source }}</span>
                  <span class="date">🕒 {{ article.savedAt | date:'short' }}</span>
                  <span class="list-name" *ngIf="article.readingListName">
                    📋 {{ article.readingListName }}
                  </span>
                  <span class="read-status" [class.read]="article.isRead">
                    {{ article.isRead ? '✅ Read' : '📖 Unread' }}
                  </span>
                </div>
              </div>
              <p class="article-description">{{ article.description || 'No description available' }}</p>
              <div class="article-actions">
                <a [href]="article.url" target="_blank" class="read-link">
                  🔗 Read Article
                </a>
                <button 
                  (click)="toggleReadStatus(article)" 
                  class="read-btn"
                  [class.read]="article.isRead">
                  {{ article.isRead ? '📖 Mark Unread' : '✅ Mark Read' }}
                </button>
                <button (click)="removeArticle(article)" class="remove-btn">
                  🗑️ Remove
                </button>
              </div>
              <div class="article-notes" *ngIf="article.notes">
                <strong>📝 Notes:</strong> {{ article.notes }}
              </div>
              <div class="article-tags" *ngIf="article.tags">
                <strong>🏷️ Tags:</strong> {{ article.tags }}
              </div>
            </div>
          </div>
        </div>

        <div class="no-articles" *ngIf="savedArticles.length === 0 && !loadingArticles">
          <h3>📭 No saved articles found</h3>
          <p>{{ getNoArticlesMessage() }}</p>
        </div>
      </div>

      <!-- Reading Lists Grid -->
      <div class="lists-section" *ngIf="readingLists.length > 0">
        <h3>📂 My Reading Lists</h3>
        <div class="lists-grid">
          <div class="list-card" *ngFor="let list of readingLists" [style.border-left-color]="getListColor(list.color)">
            <div class="list-header">
              <h3 [style.color]="getListColor(list.color)">{{ list.name }}</h3>
              <div class="list-counts">
                <span class="total">{{ list.articleCount }} articles</span>
                <span class="unread" *ngIf="list.unreadCount > 0">({{ list.unreadCount }} unread)</span>
              </div>
            </div>
            <p class="list-description" *ngIf="list.description">{{ list.description }}</p>
            <div class="list-meta">
              <span class="created">📅 {{ list.createdAt | date:'short' }}</span>
              <span class="visibility" [class.public]="list.isPublic">
                {{ list.isPublic ? '🌍 Public' : '🔒 Private' }}
              </span>
            </div>
            <div class="list-actions">
              <button (click)="viewListArticles(list)" class="view-btn">
                👀 View ({{ list.articleCount }})
              </button>
              <button (click)="editList(list)" class="edit-btn">
                ✏️ Edit
              </button>
              <button (click)="deleteList(list)" class="delete-btn" 
                      [disabled]="deletingList === list.id">
                {{ deletingList === list.id ? '🗑️ Deleting...' : '🗑️ Delete' }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <div class="no-lists" *ngIf="readingLists.length === 0 && !loading">
        <h3>📝 No reading lists yet</h3>
        <p>Create your first reading list to organize your saved articles by topics!</p>
        <button (click)="showCreateModal = true" class="create-first-btn">
          ➕ Create Your First List
        </button>
      </div>

      <div class="loading" *ngIf="loading || loadingArticles">
        <div class="spinner"></div>
        Loading {{ loading ? 'reading lists' : 'articles' }}...
      </div>

      <!-- Create/Edit Modal -->
      <div class="modal" *ngIf="showCreateModal || showEditModal" (click)="closeModal($event)">
        <div class="modal-content" (click)="$event.stopPropagation()">
          <h3>{{ showEditModal ? '✏️ Edit Reading List' : '➕ Create New Reading List' }}</h3>
          <div class="form-group">
            <label>Name*:</label>
            <input type="text" 
                   [(ngModel)]="formData.name" 
                   placeholder="Enter list name (e.g., Tech News, Health Articles)" 
                   class="form-input"
                   maxlength="100">
            <small class="form-hint">{{ formData.name.length }}/100 characters</small>
          </div>
          <div class="form-group">
            <label>Description:</label>
            <textarea [(ngModel)]="formData.description" 
                      placeholder="Optional description of this reading list" 
                      class="form-textarea"
                      maxlength="500"></textarea>
            <small class="form-hint">{{ formData.description.length }}/500 characters</small>
          </div>
          <div class="form-group">
            <label>
              <input type="checkbox" [(ngModel)]="formData.isPublic"> 
              🌍 Make this list public (others can see it)
            </label>
          </div>
          <div class="form-group">
            <label>Color theme:</label>
            <select [(ngModel)]="formData.color" class="color-select">
              <option value="">🎨 Default</option>
              <option value="blue">🔵 Blue</option>
              <option value="green">🟢 Green</option>
              <option value="red">🔴 Red</option>
              <option value="purple">🟣 Purple</option>
              <option value="orange">🟠 Orange</option>
              <option value="pink">🩷 Pink</option>
              <option value="teal">🔵 Teal</option>
            </select>
          </div>
          <div class="modal-actions">
            <button (click)="saveList()" class="save-btn" [disabled]="!formData.name.trim() || saving">
              {{ saving ? '💾 Saving...' : (showEditModal ? '✅ Update' : '➕ Create') }}
            </button>
            <button (click)="closeModal()" class="cancel-btn">❌ Cancel</button>
          </div>
        </div>
      </div>

      <!-- Success/Error notifications -->
      <div class="notification success" *ngIf="showSuccessNotification">
        <span>{{ successMessage }}</span>
        <button (click)="showSuccessNotification = false" class="close-notification">×</button>
      </div>

      <div class="notification error" *ngIf="showErrorNotification">
        <span>{{ errorMessage }}</span>
        <button (click)="showErrorNotification = false" class="close-notification">×</button>
      </div>
    </div>
  `,
  styles: [`
    .reading-lists-container {
      padding: 20px;
      max-width: 1400px;
      margin: 0 auto;
      background: #f8f9fa;
      min-height: 100vh;
    }

    .header-section {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 30px;
      flex-wrap: wrap;
      gap: 20px;
      background: white;
      padding: 20px;
      border-radius: 12px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
    }

    .header-section h2 {
      color: #343a40;
      margin: 0;
      font-size: 1.8rem;
    }

    .stats-section {
      display: flex;
      gap: 20px;
      flex-wrap: wrap;
    }

    .stat-item {
      text-align: center;
      padding: 10px;
      background: #f8f9fa;
      border-radius: 8px;
      min-width: 80px;
    }

    .stat-number {
      display: block;
      font-size: 24px;
      font-weight: bold;
      color: #007bff;
    }

    .stat-label {
      font-size: 11px;
      color: #6c757d;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .create-btn, .create-first-btn {
      padding: 12px 24px;
      background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
      color: white;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      font-size: 14px;
      font-weight: 600;
      transition: all 0.3s ease;
      box-shadow: 0 2px 10px rgba(0,123,255,0.3);
    }

    .create-btn:hover, .create-first-btn:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 20px rgba(0,123,255,0.4);
    }

    .all-articles-section, .lists-section {
      margin-bottom: 40px;
      padding: 25px;
      background: white;
      border-radius: 12px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
    }

    .all-articles-section h3, .lists-section h3 {
      margin-top: 0;
      color: #495057;
      font-size: 1.4rem;
      border-bottom: 2px solid #e9ecef;
      padding-bottom: 10px;
    }

    .filters {
      display: flex;
      gap: 20px;
      margin-bottom: 25px;
      align-items: center;
      flex-wrap: wrap;
    }

    .filter-checkbox {
      display: flex;
      align-items: center;
      cursor: pointer;
      font-weight: 500;
    }

    .filter-checkbox input[type="checkbox"] {
      margin-right: 8px;
    }

    .list-filter {
      padding: 8px 12px;
      border: 2px solid #e9ecef;
      border-radius: 6px;
      background: white;
      font-size: 14px;
      min-width: 200px;
    }

    .list-filter:focus {
      border-color: #007bff;
      outline: none;
    }

    .articles-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
      gap: 25px;
    }

    .article-card {
      background: white;
      border: 1px solid #dee2e6;
      border-radius: 12px;
      overflow: hidden;
      transition: all 0.3s ease;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
    }

    .article-card:hover {
      transform: translateY(-3px);
      box-shadow: 0 8px 25px rgba(0,0,0,0.15);
    }

    .article-card.read {
      opacity: 0.7;
      background: #f8f9fa;
    }

    .article-card.highlighted {
      border-color: #007bff;
      box-shadow: 0 0 0 3px rgba(0,123,255,0.1);
    }

    .article-image {
      width: 100%;
      height: 180px;
      object-fit: cover;
    }

    .article-content {
      padding: 20px;
    }

    .article-header h4 {
      margin: 0 0 12px 0;
      font-size: 1.1em;
      line-height: 1.4;
      color: #212529;
    }

    .article-meta {
      display: flex;
      gap: 15px;
      margin-bottom: 12px;
      font-size: 0.8em;
      color: #6c757d;
      flex-wrap: wrap;
    }

    .source {
      font-weight: 600;
      color: #495057;
    }

    .list-name {
      color: #007bff;
      font-weight: 600;
    }

    .read-status {
      padding: 2px 8px;
      border-radius: 12px;
      font-size: 0.75em;
      font-weight: 600;
      background: #ffc107;
      color: #212529;
    }

    .read-status.read {
      background: #28a745;
      color: white;
    }

    .article-description {
      font-size: 0.9em;
      color: #6c757d;
      margin-bottom: 15px;
      line-height: 1.5;
    }

    .article-actions {
      display: flex;
      gap: 10px;
      flex-wrap: wrap;
      align-items: center;
    }

    .read-link, .read-btn, .remove-btn {
      padding: 6px 12px;
      font-size: 0.8em;
      border-radius: 6px;
      text-decoration: none;
      border: none;
      cursor: pointer;
      font-weight: 500;
      transition: all 0.2s ease;
    }

    .read-link {
      background: #007bff;
      color: white;
    }

    .read-link:hover {
      background: #0056b3;
      transform: translateY(-1px);
    }

    .read-btn {
      background: #28a745;
      color: white;
    }

    .read-btn:hover {
      background: #218838;
    }

    .read-btn.read {
      background: #ffc107;
      color: #212529;
    }

    .remove-btn {
      background: #dc3545;
      color: white;
    }

    .remove-btn:hover {
      background: #c82333;
    }

    .article-notes, .article-tags {
      margin-top: 12px;
      padding: 8px 12px;
      background: #e9ecef;
      border-radius: 6px;
      font-size: 0.8em;
      border-left: 3px solid #007bff;
    }

    .lists-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
      gap: 25px;
    }

    .list-card {
      border: 1px solid #e9ecef;
      border-radius: 12px;
      padding: 20px;
      background: white;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
      transition: all 0.3s ease;
      border-left: 4px solid #007bff;
    }

    .list-card:hover {
      transform: translateY(-3px);
      box-shadow: 0 8px 25px rgba(0,0,0,0.15);
    }

    .list-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 12px;
    }

    .list-header h3 {
      margin: 0;
      color: #343a40;
      font-size: 1.2em;
    }

    .list-counts {
      font-size: 0.9em;
      color: #6c757d;
      text-align: right;
    }

    .unread {
      color: #007bff;
      font-weight: 600;
    }

    .list-description {
      color: #6c757d;
      font-size: 0.9em;
      margin-bottom: 15px;
      line-height: 1.4;
    }

    .list-meta {
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: 0.8em;
      color: #868e96;
      margin-bottom: 15px;
    }

    .visibility.public {
      color: #28a745;
      font-weight: 600;
    }

    .list-actions {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
    }

    .view-btn, .edit-btn, .delete-btn {
      padding: 6px 12px;
      border: none;
      border-radius: 6px;
      cursor: pointer;
      font-size: 0.8em;
      font-weight: 500;
      transition: all 0.2s ease;
    }

    .view-btn {
      background: #007bff;
      color: white;
    }

    .edit-btn {
      background: #ffc107;
      color: #212529;
    }

    .delete-btn {
      background: #dc3545;
      color: white;
    }

    .delete-btn:disabled {
      background: #6c757d;
      cursor: not-allowed;
    }

    .no-lists, .no-articles {
      text-align: center;
      padding: 60px 20px;
      color: #6c757d;
    }

    .no-lists h3, .no-articles h3 {
      font-size: 1.5rem;
      margin-bottom: 15px;
    }

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
      padding: 30px;
      border-radius: 12px;
      width: 90%;
      max-width: 500px;
      box-shadow: 0 20px 60px rgba(0,0,0,0.3);
    }

    .modal-content h3 {
      margin-top: 0;
      color: #343a40;
      font-size: 1.3rem;
    }

    .form-group {
      margin-bottom: 20px;
    }

    .form-group label {
      display: block;
      margin-bottom: 8px;
      font-weight: 600;
      color: #495057;
    }

    .form-input, .form-textarea, .color-select {
      width: 100%;
      padding: 10px 12px;
      border: 2px solid #e9ecef;
      border-radius: 6px;
      font-size: 14px;
      transition: border-color 0.3s ease;
    }

    .form-input:focus, .form-textarea:focus, .color-select:focus {
      outline: none;
      border-color: #007bff;
    }

    .form-textarea {
      resize: vertical;
      height: 80px;
    }

    .form-hint {
      font-size: 0.75em;
      color: #6c757d;
      margin-top: 4px;
    }

    .modal-actions {
      display: flex;
      gap: 12px;
      justify-content: flex-end;
      margin-top: 25px;
    }

    .save-btn, .cancel-btn {
      padding: 10px 20px;
      border: none;
      border-radius: 6px;
      cursor: pointer;
      font-weight: 600;
    }

    .save-btn {
      background: #007bff;
      color: white;
    }

    .save-btn:disabled {
      background: #6c757d;
      cursor: not-allowed;
    }

    .cancel-btn {
      background: #6c757d;
      color: white;
    }

    .loading {
      text-align: center;
      padding: 60px 20px;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 15px;
    }

    .spinner {
      width: 40px;
      height: 40px;
      border: 4px solid #f3f3f3;
      border-top: 4px solid #007bff;
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }

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
      font-weight: 500;
      box-shadow: 0 4px 20px rgba(0,0,0,0.2);
    }

    .notification.success {
      background: #d4edda;
      color: #155724;
      border: 1px solid #c3e6cb;
    }

    .notification.error {
      background: #f8d7da;
      color: #721c24;
      border: 1px solid #f5c6cb;
    }

    .close-notification {
      background: none;
      border: none;
      font-size: 18px;
      cursor: pointer;
      margin-left: 10px;
    }
  `]
})
export class ReadingListsComponent implements OnInit {
  readingLists: ReadingListDto[] = [];
  savedArticles: SavedArticleDto[] = [];
  stats: SavedArticleStatsDto | null = null;
  
  loading = false;
  loadingArticles = false;
  deletingList: string | null = null;
  saving = false;

  // Filters
  showOnlyUnread = false;
  selectedListFilter = '';
  highlightedListId = '';

  // Modal state
  showCreateModal = false;
  showEditModal = false;
  editingList: ReadingListDto | null = null;
  formData = {
    name: '',
    description: '',
    isPublic: false,
    color: ''
  };

  // Notifications
  showSuccessNotification = false;
  showErrorNotification = false;
  successMessage = '';
  errorMessage = '';

  constructor(
    private readingListService: ReadingListService,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    this.loadReadingLists();
    this.loadSavedArticles();
    this.loadStats();

    // Check if we need to highlight a specific list from query params
    this.route.queryParams.subscribe(params => {
      if (params['list']) {
        this.selectedListFilter = params['list'];
        this.highlightedListId = params['list'];
        this.loadSavedArticles();
      }
    });
  }

  loadReadingLists() {
    this.loading = true;
    this.readingListService.getMyReadingLists().subscribe({
      next: (lists: ReadingListDto[]) => {
        this.readingLists = lists.sort((a, b) => a.sortOrder - b.sortOrder);
        this.loading = false;
      },
      error: (err: any) => {
        this.handleError('Failed to load reading lists', err);
        this.loading = false;
      }
    });
  }

  loadSavedArticles() {
    this.loadingArticles = true;
    const readingListId = this.selectedListFilter || undefined;
    
    this.readingListService.getMySavedArticles(readingListId, this.showOnlyUnread, 100).subscribe({
      next: (articles: SavedArticleDto[]) => {
        this.savedArticles = articles.sort((a, b) => 
          new Date(b.savedAt).getTime() - new Date(a.savedAt).getTime()
        );
        this.loadingArticles = false;
      },
      error: (err: any) => {
        this.handleError('Failed to load saved articles', err);
        this.loadingArticles = false;
      }
    });
  }

  loadStats() {
    this.readingListService.getSavedArticleStats().subscribe({
      next: (stats: SavedArticleStatsDto) => {
        this.stats = stats;
      },
      error: (err: any) => {
        console.error('Failed to load stats:', err);
      }
    });
  }

  toggleReadStatus(article: SavedArticleDto) {
    const action = article.isRead 
      ? this.readingListService.markAsUnread(article.id)
      : this.readingListService.markAsRead(article.id);

    action.subscribe({
      next: () => {
        article.isRead = !article.isRead;
        this.loadStats(); // Refresh stats
        this.showSuccess(article.isRead ? 'Article marked as read!' : 'Article marked as unread!');
      },
      error: (err: any) => {
        this.handleError('Failed to update article status', err);
      }
    });
  }

  removeArticle(article: SavedArticleDto) {
    if (confirm(`Remove "${article.title}" from your saved articles?`)) {
      this.readingListService.unsaveArticle(article.id).subscribe({
        next: () => {
          this.savedArticles = this.savedArticles.filter(a => a.id !== article.id);
          this.loadReadingLists(); // Refresh to update counts
          this.loadStats();
          this.showSuccess('Article removed from your saved articles!');
        },
        error: (err: any) => {
          this.handleError('Failed to remove article', err);
        }
      });
    }
  }

  viewListArticles(list: ReadingListDto) {
    this.selectedListFilter = list.id;
    this.highlightedListId = list.id;
    this.loadSavedArticles();
    
    // Scroll to articles section
    document.querySelector('.all-articles-section')?.scrollIntoView({ 
      behavior: 'smooth' 
    });
  }

  editList(list: ReadingListDto) {
    this.editingList = list;
    this.formData = {
      name: list.name,
      description: list.description || '',
      isPublic: list.isPublic,
      color: list.color || ''
    };
    this.showEditModal = true;
  }

  deleteList(list: ReadingListDto) {
    const message = list.articleCount > 0 
      ? `Delete "${list.name}" with ${list.articleCount} articles? The articles will be moved to "Uncategorized".`
      : `Delete "${list.name}"?`;
    
    if (confirm(message)) {
      this.deletingList = list.id;
      this.readingListService.deleteReadingList(list.id).subscribe({
        next: () => {
          this.readingLists = this.readingLists.filter(l => l.id !== list.id);
          this.deletingList = null;
          if (this.selectedListFilter === list.id) {
            this.selectedListFilter = '';
            this.highlightedListId = '';
            this.loadSavedArticles();
          }
          this.showSuccess(`Reading list "${list.name}" deleted!`);
        },
        error: (err: any) => {
          this.handleError('Failed to delete reading list', err);
          this.deletingList = null;
        }
      });
    }
  }

  saveList() {
    if (!this.formData.name.trim() || this.saving) return;

    this.saving = true;

    if (this.showEditModal && this.editingList) {
      const updateData: UpdateReadingListDto = {
        name: this.formData.name.trim(),
        description: this.formData.description.trim() || undefined,
        isPublic: this.formData.isPublic,
        color: this.formData.color || undefined,
        sortOrder: this.editingList.sortOrder
      };

      this.readingListService.updateReadingList(this.editingList.id, updateData).subscribe({
        next: (updatedList) => {
          this.loadReadingLists();
          this.closeModal();
          this.showSuccess(`Reading list "${updatedList.name}" updated!`);
        },
        error: (err: any) => {
          this.handleError('Failed to update reading list', err);
          this.saving = false;
        }
      });
    } else {
      const createData: CreateReadingListDto = {
        name: this.formData.name.trim(),
        description: this.formData.description.trim() || undefined,
        isPublic: this.formData.isPublic,
        color: this.formData.color || undefined,
        sortOrder: this.readingLists.length
      };

      this.readingListService.createReadingList(createData).subscribe({
        next: (newList) => {
          this.loadReadingLists();
          this.closeModal();
          this.showSuccess(`Reading list "${newList.name}" created!`);
        },
        error: (err: any) => {
          this.handleError('Failed to create reading list', err);
          this.saving = false;
        }
      });
    }
  }

  closeModal(event?: any) {
    if (event && event.target !== event.currentTarget) {
      return;
    }
    
    this.showCreateModal = false;
    this.showEditModal = false;
    this.editingList = null;
    this.formData = { name: '', description: '', isPublic: false, color: '' };
    this.saving = false;
  }

  getImageSrc(article: SavedArticleDto): string {
    return article.urlToImage || 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="300" height="150" viewBox="0 0 300 150"%3E%3Crect width="100%25" height="100%25" fill="%23f0f0f0"%3E%3C/rect%3E%3Ctext x="50%25" y="50%25" font-family="Arial" font-size="12" fill="%23999" text-anchor="middle" dy=".3em"%3E📰 No Image%3C/text%3E%3C/svg%3E';
  }

  onImageError(event: any) {
    event.target.src = this.getImageSrc({} as SavedArticleDto);
  }

  getListColor(color?: string): string {
    const colors: { [key: string]: string } = {
      blue: '#007bff',
      green: '#28a745',
      red: '#dc3545',
      purple: '#6f42c1',
      orange: '#fd7e14',
      pink: '#e83e8c',
      teal: '#20c997'
    };
    return colors[color || ''] || '#007bff';
  }

  getNoArticlesMessage(): string {
    if (this.selectedListFilter) {
      const list = this.readingLists.find(l => l.id === this.selectedListFilter);
      const listName = list?.name || 'this list';
      return this.showOnlyUnread 
        ? `No unread articles in "${listName}". Great job staying up to date!`
        : `No articles saved in "${listName}" yet. Start saving interesting articles to this list!`;
    }
    return this.showOnlyUnread 
      ? 'All caught up! No unread articles. Great job staying informed!'
      : 'Start saving interesting articles to read later. Use the "Save for Later" button on any news article!';
  }

  private showSuccess(message: string) {
    this.successMessage = message;
    this.showSuccessNotification = true;
    setTimeout(() => {
      this.showSuccessNotification = false;
    }, 4000);
  }

  private showError(message: string) {
    this.errorMessage = message;
    this.showErrorNotification = true;
    setTimeout(() => {
      this.showErrorNotification = false;
    }, 5000);
  }

  private handleError(message: string, err: any) {
    console.error(message, err);
    const errorMsg = err.error?.message || err.message || message;
    this.showError(errorMsg);
  }
}
