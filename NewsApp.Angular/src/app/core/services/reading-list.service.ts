import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  ReadingListDto, 
  CreateReadingListDto, 
  UpdateReadingListDto,
  SavedArticleDto,
  SaveArticleDto,
  UpdateSavedArticleDto,
  SavedArticleStatsDto,
  BulkUpdateSavedArticlesDto
} from '../../shared/models/reading-list.model';

@Injectable({
  providedIn: 'root'
})
export class ReadingListService {
  private baseUrl = environment.apiUrl || 'https://localhost:44341/api';

  constructor(private http: HttpClient) {}

  // Reading List methods
  getMyReadingLists(): Observable<ReadingListDto[]> {
    return this.http.get<ReadingListDto[]>(`${this.baseUrl}/reading-lists/my-lists`);
  }

  getReadingListWithArticles(id: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/reading-lists/${id}/with-articles`);
  }

  getPublicReadingLists(maxCount: number = 50): Observable<ReadingListDto[]> {
    return this.http.get<ReadingListDto[]>(`${this.baseUrl}/reading-lists/public?maxCount=${maxCount}`);
  }

  createReadingList(input: CreateReadingListDto): Observable<ReadingListDto> {
    return this.http.post<ReadingListDto>(`${this.baseUrl}/reading-lists`, input);
  }

  updateReadingList(id: string, input: UpdateReadingListDto): Observable<ReadingListDto> {
    return this.http.put<ReadingListDto>(`${this.baseUrl}/reading-lists/${id}`, input);
  }

  deleteReadingList(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/reading-lists/${id}`);
  }

  reorderReadingLists(listOrders: { [key: string]: number }): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/reading-lists/reorder`, listOrders);
  }

  // Saved Article methods
  getMySavedArticles(readingListId?: string, onlyUnread: boolean = false, maxCount: number = 100): Observable<SavedArticleDto[]> {
    let url = `${this.baseUrl}/saved-articles/my-articles?onlyUnread=${onlyUnread}&maxCount=${maxCount}`;
    if (readingListId) {
      url += `&readingListId=${readingListId}`;
    }
    return this.http.get<SavedArticleDto[]>(url);
  }

  getSavedArticle(id: string): Observable<SavedArticleDto> {
    return this.http.get<SavedArticleDto>(`${this.baseUrl}/saved-articles/${id}`);
  }

  isArticleSaved(url: string): Observable<boolean> {
    const encodedUrl = encodeURIComponent(url);
    return this.http.get<boolean>(`${this.baseUrl}/saved-articles/is-saved?url=${encodedUrl}`);
  }

  saveArticle(input: SaveArticleDto): Observable<SavedArticleDto> {
    return this.http.post<SavedArticleDto>(`${this.baseUrl}/saved-articles/save`, input);
  }

  updateSavedArticle(id: string, input: UpdateSavedArticleDto): Observable<SavedArticleDto> {
    return this.http.put<SavedArticleDto>(`${this.baseUrl}/saved-articles/${id}`, input);
  }

  unsaveArticle(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/saved-articles/${id}`);
  }

  unsaveArticleByUrl(url: string): Observable<void> {
    const encodedUrl = encodeURIComponent(url);
    return this.http.delete<void>(`${this.baseUrl}/saved-articles/by-url?url=${encodedUrl}`);
  }

  markAsRead(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/saved-articles/${id}/mark-as-read`, {});
  }

  markAsUnread(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/saved-articles/${id}/mark-as-unread`, {});
  }

  moveToReadingList(articleId: string, readingListId?: string): Observable<void> {
    const url = readingListId 
      ? `${this.baseUrl}/saved-articles/${articleId}/move-to-list/${readingListId}`
      : `${this.baseUrl}/saved-articles/${articleId}/move-to-list`;
    return this.http.post<void>(url, {});
  }

  bulkUpdateSavedArticles(input: BulkUpdateSavedArticlesDto): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/saved-articles/bulk-update`, input);
  }

  getSavedArticleStats(): Observable<SavedArticleStatsDto> {
    return this.http.get<SavedArticleStatsDto>(`${this.baseUrl}/saved-articles/stats`);
  }

  // New Lists API methods
  addArticleToList(listId: string, articleId: string, notes?: string): Observable<any> {
    const payload = {
      articleId: articleId,
      notes: notes
    };
    return this.http.post(`${this.baseUrl}/app/reading-list/${listId}/add-article`, payload);
  }

  getReadingListIdsForArticle(url: string): Observable<string[]> {
    const encodedUrl = encodeURIComponent(url);
    return this.http.get<string[]>(`${this.baseUrl}/saved-articles/lists-for-article?url=${encodedUrl}`);
  }

  unsaveArticleFromList(url: string, readingListId: string): Observable<void> {
    const encodedUrl = encodeURIComponent(url);
    return this.http.delete<void>(`${this.baseUrl}/saved-articles/by-url-and-list?url=${encodedUrl}&readingListId=${readingListId}`);
  }
}
