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
  alertCategory = '';
  alertCategories: string[] = [];
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

  // API call limiter - 5 calls per keyword/category
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
      this.alertCategory = params['category'] || '';
      this.alertLanguage = params['language'] || 'en';
      this.alertName = params['alertName'] || 'Alert';
      
      // Check if coming from notification
      this.fromNotification = params['fromNotification'] === 'true';
      this.notificationId = params['notificationId'] || null;
      this.expectedArticleCount = parseInt(params['articleCount']) || 0;
      
      // Adjust pageSize if coming from notification to show exact number of articles
      if (this.fromNotification && this.expectedArticleCount > 0) {
        this.pageSize = this.expectedArticleCount;
        console.log(`Coming from notification: expecting ${this.expectedArticleCount} articles`);
      }
      
      // Handle multiple categories
      if (params['categories']) {
        this.alertCategories = params['categories'].split(',').map((c: string) => c.trim()).filter((c: string) => c);
        console.log('Alert categories:', this.alertCategories);
      } else if (this.alertCategory) {
        this.alertCategories = [this.alertCategory];
      }
      
      // Load news with filters
      this.loadAlertNews();
    });
  }

  loadAlertNews(): void {
    // Validate that we have either keyword or categories
    if (!this.alertKeyword && this.alertCategories.length === 0) {
      this.error = 'No search criteria specified for this alert';
      return;
    }

    this.loading = true;
    this.error = null;
    this.currentPage = 1;
    
    // Use keyword search if available
    if (this.alertKeyword) {
      const filterKey = `keyword_${this.alertKeyword}`;
      const currentCount = this.apiCallCounts.get(filterKey) || 0;
      
      if (currentCount >= this.MAX_API_CALLS_PER_FILTER) {
        this.error = `Se ha alcanzado el límite de ${this.MAX_API_CALLS_PER_FILTER} llamadas para: ${this.alertKeyword}`;
        this.loading = false;
        return;
      }
      
      this.apiCallCounts.set(filterKey, currentCount + 1);
      
      this.newsService.searchLocalNews(this.alertKeyword, this.alertLanguage, 0, this.pageSize).subscribe({
        next: (result: PagedResultDto<NewsArticleDto>) => {
          this.articles = result.items || [];
          this.hasMoreNews = result.items?.length >= this.pageSize;
          this.loading = false;
        },
        error: (err) => {
          this.handleError(err);
        }
      });
    } else if (this.alertCategories.length > 1) {
      // Multiple categories - load and combine
      this.loadNewsForMultipleCategories();
    } else if (this.alertCategories.length === 1) {
      // Single category
      const category = this.alertCategories[0];
      const filterKey = `category_${category}`;
      const currentCount = this.apiCallCounts.get(filterKey) || 0;
      
      if (currentCount >= this.MAX_API_CALLS_PER_FILTER) {
        this.error = `Se ha alcanzado el límite de ${this.MAX_API_CALLS_PER_FILTER} llamadas para: ${category}`;
        this.loading = false;
        return;
      }
      
      this.apiCallCounts.set(filterKey, currentCount + 1);
      
      this.newsService.getTopHeadlines(category, undefined, this.alertLanguage, 1, this.pageSize).subscribe({
        next: (result: PagedResultDto<NewsArticleDto>) => {
          this.articles = result.items || [];
          this.hasMoreNews = result.items?.length >= this.pageSize;
          this.loading = false;
        },
        error: (err) => {
          this.handleError(err);
        }
      });
    }
  }

  loadNewsForMultipleCategories(): void {
    // Check API call limit for each category
    for (const category of this.alertCategories) {
      const filterKey = `category_${category}`;
      const currentCount = this.apiCallCounts.get(filterKey) || 0;
      
      if (currentCount >= this.MAX_API_CALLS_PER_FILTER) {
        this.error = `Se ha alcanzado el límite de ${this.MAX_API_CALLS_PER_FILTER} llamadas para: ${category}`;
        this.loading = false;
        return;
      }
    }
    
    // Load news for each category
    const categoryRequests = this.alertCategories.map(category => {
      const filterKey = `category_${category}`;
      const currentCount = this.apiCallCounts.get(filterKey) || 0;
      this.apiCallCounts.set(filterKey, currentCount + 1);
      
      return this.newsService.getTopHeadlines(category, undefined, this.alertLanguage, 1, this.pageSize);
    });

    // Use Promise.all to load all categories at once
    Promise.all(categoryRequests.map(req => req.toPromise()))
      .then(results => {
        // Combine all articles from all categories
        const allArticles: NewsArticleDto[] = [];
        const seenUrls = new Set<string>();
        
        results.forEach((result, index) => {
          console.log(`Category ${this.alertCategories[index]} returned:`, result?.items?.length || 0, 'articles');
          if (result && result.items && Array.isArray(result.items)) {
            result.items.forEach((article: NewsArticleDto) => {
              // Avoid duplicates
              if (!seenUrls.has(article.url)) {
                seenUrls.add(article.url);
                allArticles.push(article);
              }
            });
          }
        });

        // Sort by published date (newest first)
        allArticles.sort((a, b) => {
          const dateA = new Date(a.publishedAt).getTime();
          const dateB = new Date(b.publishedAt).getTime();
          return dateB - dateA;
        });

        this.articles = allArticles;
        this.hasMoreNews = false; // Disable pagination for multi-category search
        this.loading = false;
        console.log(`Loaded news from ${this.alertCategories.length} categories:`, allArticles.length, 'total articles');
      })
      .catch(err => {
        console.error('Error in loadNewsForMultipleCategories:', err);
        this.handleError(err);
      });
  }

  loadMoreNews(): void {
    if (this.loadingMore || !this.hasMoreNews) {
      return;
    }

    // Check API call limit
    const filterKey = this.alertKeyword ? `keyword_${this.alertKeyword}` : `category_${this.alertCategory}`;
    const currentCount = this.apiCallCounts.get(filterKey) || 0;
    
    if (currentCount >= this.MAX_API_CALLS_PER_FILTER) {
      this.hasMoreNews = false;
      return;
    }
    
    // Increment call count
    this.apiCallCounts.set(filterKey, currentCount + 1);

    this.loadingMore = true;
    const skipCount = this.currentPage * this.pageSize;

    if (this.alertKeyword) {
      this.newsService.searchLocalNews(this.alertKeyword, this.alertLanguage, skipCount, this.pageSize).subscribe({
        next: (result: PagedResultDto<NewsArticleDto>) => {
          const newArticles = result.items || [];
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
    } else if (this.alertCategory) {
      const page = this.currentPage + 1;
      this.newsService.getTopHeadlines(this.alertCategory, undefined, this.alertLanguage, page, this.pageSize).subscribe({
        next: (result: PagedResultDto<NewsArticleDto>) => {
          const newArticles = result.items || [];
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
    const now = new Date();
    const diffTime = Math.abs(now.getTime() - date.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    
    if (diffDays === 0) {
      return 'Today';
    } else if (diffDays === 1) {
      return 'Yesterday';
    } else if (diffDays < 7) {
      return `${diffDays} days ago`;
    } else {
      return date.toLocaleDateString();
    }
  }
}
