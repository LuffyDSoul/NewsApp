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
  alertLanguage = 'en';
  alertName = '';
  
  // Pagination
  currentPage = 1;
  pageSize = 10;
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
      
      // Load news with filters
      this.loadAlertNews();
    });
  }

  loadAlertNews(): void {
    if (!this.alertKeyword) {
      this.error = 'No keyword specified for this alert';
      return;
    }

    this.loading = true;
    this.error = null;
    this.currentPage = 1;
    
    // Use searchLocalNews which will search by keyword
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
  }

  loadMoreNews(): void {
    if (this.loadingMore || !this.hasMoreNews) {
      return;
    }

    this.loadingMore = true;
    const skipCount = this.currentPage * this.pageSize;

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
