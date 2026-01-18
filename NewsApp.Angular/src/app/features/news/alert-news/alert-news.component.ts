import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NewsService } from '../../../core/services/news.service';
import { NewsArticleDto, PagedResultDto } from '../../../shared/models/news.model';
import { ReadingListService } from '../../../core/services/reading-list.service';
import { ReadingListDto, SavedArticleDto, SaveArticleDto } from '../../../shared/models/reading-list.model';

@Component({
  selector: 'app-alert-news',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './alert-news.component.html',
  styleUrls: ['./alert-news.component.scss']
})
export class AlertNewsComponent implements OnInit {
  articles: NewsArticleDto[] = [];
  loading = false;
  error: string | null = null;
  
  // Filter parameters from alert
  alertKeyword = '';
  alertLanguage = 'en';
  alertName = '';
  
  // Notification parameters
  fromNotification = false;
  notificationId: string | null = null;
  expectedArticleCount = 0;
  
  // Pagination
  currentPage = 1;
  pageSize = 5;
  hasMoreNews = true;
  loadingMore = false;
  
  // Reading lists
  readingLists: ReadingListDto[] = [];
  savedArticleUrls = new Set<string>();
  savingArticles = new Set<string>();
  showCreateListModal = false;
  showSaveToListsModal = false;
  selectedArticleForSave: NewsArticleDto | null = null;
  selectedListIds = new Set<string>();
  originalListIds = new Set<string>();
  
  newListName = '';
  newListDescription = '';
  isCreatingList = false;

  // API call limiter - 5 calls per keyword
  private apiCallCounts: Map<string, number> = new Map();
  private readonly MAX_API_CALLS_PER_FILTER = 5;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private newsService: NewsService,
    private readingListService: ReadingListService
  ) {}

  ngOnInit(): void {
    this.loadReadingLists();
    this.loadSavedArticles();
    
    // Get filter parameters from query params
    this.route.queryParams.subscribe(params => {
      this.alertKeyword = params['keyword'] || '';
      this.alertLanguage = params['language'] || 'en';
      this.alertName = params['alertName'] || 'Alert';
      
      // Check if coming from notification
      this.fromNotification = params['fromNotification'] === 'true';
      this.notificationId = params['notificationId'] || null;
      this.expectedArticleCount = parseInt(params['articleCount']) || 0;
      
      // Check if we have specific article URLs from the notification
      const articleUrls = params['articleUrls'] || '';
      
      if (this.fromNotification && articleUrls) {
        // Load specific articles from the notification
        this.loadArticlesByUrls(articleUrls);
      } else {
        // Adjust pageSize if coming from notification to show exact number of articles
        if (this.fromNotification && this.expectedArticleCount > 0) {
          this.pageSize = this.expectedArticleCount;
          console.log(`Coming from notification: expecting ${this.expectedArticleCount} articles`);
        }
        
        // Load news with filters
        this.loadAlertNews();
      }
    });
  }

  loadAlertNews(): void {
    // Validate that we have a keyword
    if (!this.alertKeyword || !this.alertKeyword.trim()) {
      this.error = 'No keyword specified for this alert';
      return;
    }

    this.loading = true;
    this.error = null;
    this.currentPage = 1;
    
    // When coming from "View News" (not a notification), always get fresh recent news
    // Use a larger pageSize to ensure we get recent articles
    const effectivePageSize = this.fromNotification ? this.pageSize : 20;
    
    const filterKey = `keyword_${this.alertKeyword}`;
    const currentCount = this.apiCallCounts.get(filterKey) || 0;
    
    if (currentCount >= this.MAX_API_CALLS_PER_FILTER) {
      this.error = `Se ha alcanzado el límite de ${this.MAX_API_CALLS_PER_FILTER} llamadas para: ${this.alertKeyword}`;
      this.loading = false;
      return;
    }
    
    this.apiCallCounts.set(filterKey, currentCount + 1);
    
    // For keywords, use getTopHeadlines which calls NewsAPI directly for fresh results
    this.newsService.getTopHeadlines(this.alertKeyword, undefined, this.alertLanguage, 1, effectivePageSize).subscribe({
      next: (result: PagedResultDto<NewsArticleDto>) => {
        this.articles = result.items || [];
        // Sort by published date (newest first)
        this.articles.sort((a, b) => {
          const dateA = new Date(a.publishedAt).getTime();
          const dateB = new Date(b.publishedAt).getTime();
          return dateB - dateA;
        });
        this.hasMoreNews = result.items?.length >= this.pageSize;
        this.loading = false;
      },
      error: (err) => {
        this.handleError(err);
      }
    });
  }

  loadArticlesByUrls(urlsString: string): void {
    if (!urlsString) {
      this.error = 'No article URLs provided';
      return;
    }

    this.loading = true;
    this.error = null;

    // Split the comma-separated URLs
    const urls = urlsString.split(',').map(url => url.trim()).filter(url => url);
    
    console.log(`Loading ${urls.length} specific articles from notification`);
    console.log('Article URLs:', urls);

    // Search for recent news with the keyword to find the articles
    // Use searchLocalNews first to check local DB, then fall back to API
    this.newsService.searchLocalNews(this.alertKeyword, this.alertLanguage, 0, 200).subscribe({
      next: (result: PagedResultDto<NewsArticleDto>) => {
        console.log(`Loaded ${result.items?.length || 0} articles from local database`);
        
        // Try to match articles by URL
        let matchedArticles = (result.items || []).filter(article => urls.includes(article.url));
        
        if (matchedArticles.length > 0) {
          // Found some or all articles in local DB
          console.log(`Found ${matchedArticles.length} articles in local database`);
          this.articles = matchedArticles;
        } else {
          // No matches in local DB, try to get recent articles from API
          console.log('No matches in local DB, trying fresh API data...');
          
          // Show the most recent articles as fallback
          this.articles = (result.items || [])
            .sort((a, b) => new Date(b.publishedAt).getTime() - new Date(a.publishedAt).getTime())
            .slice(0, this.expectedArticleCount || 10);
          
          if (this.articles.length === 0) {
            // If still no articles, show message
            this.error = `No se encontraron artículos para "${this.alertKeyword}". Los artículos específicos de la notificación ya no están disponibles.`;
          } else {
            console.log(`Showing ${this.articles.length} most recent articles as fallback`);
          }
        }
        
        // Sort by published date (newest first)
        this.articles.sort((a, b) => {
          const dateA = new Date(a.publishedAt).getTime();
          const dateB = new Date(b.publishedAt).getTime();
          return dateB - dateA;
        });
        
        this.hasMoreNews = false; // No pagination for notification articles
        this.loading = false;
        console.log(`Showing ${this.articles.length} articles (expected: ${this.expectedArticleCount})`);
      },
      error: (err) => {
        console.error('Error loading articles:', err);
        this.handleError(err);
      }
    });
  }

  loadMoreNews(): void {
    if (this.loadingMore || !this.hasMoreNews || !this.alertKeyword) {
      return;
    }

    // Check API call limit
    const filterKey = `keyword_${this.alertKeyword}`;
    const currentCount = this.apiCallCounts.get(filterKey) || 0;
    
    if (currentCount >= this.MAX_API_CALLS_PER_FILTER) {
      this.hasMoreNews = false;
      return;
    }
    
    // Increment call count
    this.apiCallCounts.set(filterKey, currentCount + 1);

    this.loadingMore = true;
    const skipCount = this.currentPage * this.pageSize;

    this.newsService.searchLocalNews(this.alertKeyword, this.alertLanguage, skipCount, this.pageSize).subscribe({
      next: (result: PagedResultDto<NewsArticleDto>) => {
        const newArticles = result.items || [];
        // Sort new articles by date
        newArticles.sort((a, b) => {
          const dateA = new Date(a.publishedAt).getTime();
          const dateB = new Date(b.publishedAt).getTime();
          return dateB - dateA;
        });
        this.articles = [...this.articles, ...newArticles];
        this.currentPage++;
        this.hasMoreNews = newArticles.length >= this.pageSize;
        this.loadingMore = false;
      },
      error: (err) => {
        console.error('Error loading more news:', err);
        this.loadingMore = false;
      }
    });
  }

  handleError(err: any): void {
    this.loading = false;
    if (err.status === 404) {
      this.error = 'No news found matching your alert criteria';
    } else if (err.status === 401) {
      this.error = 'You need to be logged in to view news';
    } else {
      this.error = err.error?.error?.message || 'Failed to load news. Please try again.';
    }
  }

  goBack(): void {
    this.router.navigate(['/news-alerts']);
  }

  // Reading List Methods
  loadReadingLists(): void {
    this.readingListService.getMyReadingLists().subscribe({
      next: (lists: ReadingListDto[]) => {
        this.readingLists = lists;
      },
      error: (err) => {
        console.error('Error loading reading lists:', err);
      }
    });
  }

  loadSavedArticles(): void {
    this.readingListService.getMySavedArticles().subscribe({
      next: (articles: SavedArticleDto[]) => {
        articles.forEach(article => {
          if (article.url) {
            this.savedArticleUrls.add(article.url);
          }
        });
      },
      error: (err) => {
        console.error('Error loading saved articles:', err);
      }
    });
  }

  isArticleSaved(article: NewsArticleDto): boolean {
    return this.savedArticleUrls.has(article.url);
  }

  openSaveToListsModal(article: NewsArticleDto): void {
    this.selectedArticleForSave = article;
    this.selectedListIds.clear();
    this.originalListIds.clear();

    if (this.isArticleSaved(article)) {
      this.readingListService.getReadingListIdsForArticle(article.url).subscribe({
        next: (listIds: string[]) => {
          listIds.forEach(id => {
            this.selectedListIds.add(id);
            this.originalListIds.add(id);
          });
        },
        error: (err) => {
          console.error('Error getting lists for article:', err);
        }
      });
    }

    this.showSaveToListsModal = true;
  }

  toggleListSelection(listId: string): void {
    if (this.selectedListIds.has(listId)) {
      this.selectedListIds.delete(listId);
    } else {
      this.selectedListIds.add(listId);
    }
  }

  saveToSelectedLists(): void {
    if (!this.selectedArticleForSave) return;

    const listsToAdd = Array.from(this.selectedListIds).filter(id => !this.originalListIds.has(id));
    const listsToRemove = Array.from(this.originalListIds).filter(id => !this.selectedListIds.has(id));

    let operations = 0;
    const totalOperations = listsToAdd.length + listsToRemove.length;

    if (totalOperations === 0) {
      this.closeSaveToListsModal();
      return;
    }

    this.savingArticles.add(this.selectedArticleForSave.url);

    listsToAdd.forEach(listId => {
      const articleToSave: SaveArticleDto = {
        title: this.selectedArticleForSave!.title,
        url: this.selectedArticleForSave!.url,
        description: this.selectedArticleForSave!.description || '',
        source: this.selectedArticleForSave!.source || '',
        publishedAt: this.selectedArticleForSave!.publishedAt || new Date().toISOString(),
        languageCode: this.alertLanguage,
        readingListId: listId
      };
      
      this.readingListService.saveArticle(articleToSave).subscribe({
        next: () => {
          operations++;
          if (operations === totalOperations) {
            this.savedArticleUrls.add(this.selectedArticleForSave!.url);
            this.savingArticles.delete(this.selectedArticleForSave!.url);
            this.closeSaveToListsModal();
          }
        },
        error: (err) => {
          console.error('Error adding article to list:', err);
          operations++;
          if (operations === totalOperations) {
            this.savingArticles.delete(this.selectedArticleForSave!.url);
            this.closeSaveToListsModal();
          }
        }
      });
    });

    listsToRemove.forEach(listId => {
      this.readingListService.unsaveArticleFromList(this.selectedArticleForSave!.url, listId).subscribe({
        next: () => {
          operations++;
          if (operations === totalOperations) {
            const stillSaved = Array.from(this.selectedListIds).length > 0;
            if (!stillSaved) {
              this.savedArticleUrls.delete(this.selectedArticleForSave!.url);
            }
            this.savingArticles.delete(this.selectedArticleForSave!.url);
            this.closeSaveToListsModal();
          }
        },
        error: (err) => {
          console.error('Error removing article from list:', err);
          operations++;
          if (operations === totalOperations) {
            this.savingArticles.delete(this.selectedArticleForSave!.url);
            this.closeSaveToListsModal();
          }
        }
      });
    });
  }

  closeSaveToListsModal(): void {
    this.showSaveToListsModal = false;
    this.selectedArticleForSave = null;
    this.selectedListIds.clear();
    this.originalListIds.clear();
  }

  openCreateListModal(): void {
    this.showCreateListModal = true;
    this.newListName = '';
    this.newListDescription = '';
  }

  closeCreateListModal(): void {
    this.showCreateListModal = false;
    this.newListName = '';
    this.newListDescription = '';
  }

  createNewList(): void {
    if (!this.newListName.trim()) {
      return;
    }

    this.isCreatingList = true;
    this.readingListService.createReadingList({
      name: this.newListName.trim(),
      description: this.newListDescription.trim(),
      isPublic: false,
      sortOrder: 0
    }).subscribe({
      next: (newList: ReadingListDto) => {
        this.readingLists.push(newList);
        this.isCreatingList = false;
        this.closeCreateListModal();
      },
      error: (err) => {
        console.error('Error creating list:', err);
        this.isCreatingList = false;
      }
    });
  }

  openArticle(article: NewsArticleDto): void {
    window.open(article.url, '_blank');
  }

  formatDate(dateString: string | Date): string {
    const date = typeof dateString === 'string' ? new Date(dateString) : dateString;
    
    // Format: Jan 18, 2026 at 3:45 PM
    const options: Intl.DateTimeFormatOptions = {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    };
    
    return date.toLocaleString('en-US', options);
  }
}
