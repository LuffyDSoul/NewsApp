import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { NewsArticleDto, NewsSearchDto, PagedResultDto, NewsSourceDto } from '../../shared/models/news.model';

@Injectable({
  providedIn: 'root'
})
export class NewsService {
  private readonly baseUrl = `${environment.apiUrl}/news`;

  constructor(private http: HttpClient) {}

  // Búsqueda de noticias
  searchNews(searchDto: NewsSearchDto): Observable<PagedResultDto<NewsArticleDto>> {
    return this.http.post<PagedResultDto<NewsArticleDto>>(`${this.baseUrl}/search`, searchDto)
      .pipe(catchError(this.handleError<PagedResultDto<NewsArticleDto>>('searchNews', { totalCount: 0, items: [] })));
  }

  // Obtener titulares principales
  getTopHeadlines(
    category?: string,
    country?: string,
    language: string = 'en',
    page: number = 1,
    pageSize: number = 20
  ): Observable<PagedResultDto<NewsArticleDto>> {
    let params = new HttpParams()
      .set('language', language)
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (category) params = params.set('category', category);
    if (country) params = params.set('country', country);

    return this.http.get<PagedResultDto<NewsArticleDto>>(`${this.baseUrl}/get-top-headlines`, { params })
      .pipe(catchError(this.handleError<PagedResultDto<NewsArticleDto>>('getTopHeadlines', { totalCount: 0, items: [] })));
  }

  // Obtener noticias más recientes
  getLatestNews(count: number = 10, languageCode?: string): Observable<NewsArticleDto[]> {
    let params = new HttpParams().set('count', count.toString());
    if (languageCode) params = params.set('languageCode', languageCode);

    return this.http.get<NewsArticleDto[]>(`${this.baseUrl}/get-latest`, { params })
      .pipe(catchError(this.handleError<NewsArticleDto[]>('getLatestNews', [])));
  }

  // Obtener fuentes de noticias
  getSources(language?: string, country?: string): Observable<NewsSourceDto[]> {
    let params = new HttpParams();
    if (language) params = params.set('language', language);
    if (country) params = params.set('country', country);

    return this.http.get<NewsSourceDto[]>(`${this.baseUrl}/get-sources`, { params })
      .pipe(catchError(this.handleError<NewsSourceDto[]>('getSources', [])));
  }

  // Obtener noticias de fuentes específicas
  getNewsFromSources(
    sources: string,
    language: string = 'en',
    page: number = 1,
    pageSize: number = 20
  ): Observable<PagedResultDto<NewsArticleDto>> {
    const params = new HttpParams()
      .set('sources', sources)
      .set('language', language)
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResultDto<NewsArticleDto>>(`${this.baseUrl}/get-from-sources`, { params })
      .pipe(catchError(this.handleError<PagedResultDto<NewsArticleDto>>('getNewsFromSources', { totalCount: 0, items: [] })));
  }

  // Buscar noticias localmente
  searchLocalNews(
    searchText: string,
    languageCode?: string,
    skipCount: number = 0,
    maxResultCount: number = 10
  ): Observable<PagedResultDto<NewsArticleDto>> {
    let params = new HttpParams()
      .set('searchText', searchText)
      .set('skipCount', skipCount.toString())
      .set('maxResultCount', maxResultCount.toString());

    if (languageCode) params = params.set('languageCode', languageCode);

    return this.http.get<PagedResultDto<NewsArticleDto>>(`${this.baseUrl}/search-local`, { params })
      .pipe(catchError(this.handleError<PagedResultDto<NewsArticleDto>>('searchLocalNews', { totalCount: 0, items: [] })));
  }

  // Probar conexión
  testConnection(): Observable<boolean> {
    return this.http.get<boolean>(`${this.baseUrl}/test-connection`)
      .pipe(catchError(this.handleError<boolean>('testConnection', false)));
  }

  // Obtener noticias por fuente
  getBySource(
    source: string, 
    skipCount: number = 0, 
    maxResultCount: number = 10
  ): Observable<PagedResultDto<NewsArticleDto>> {
    const params = new HttpParams()
      .set('source', source)
      .set('skipCount', skipCount.toString())
      .set('maxResultCount', maxResultCount.toString());

    return this.http.get<PagedResultDto<NewsArticleDto>>(`${this.baseUrl}/get-by-source`, { params })
      .pipe(catchError(this.handleError<PagedResultDto<NewsArticleDto>>('getBySource', { totalCount: 0, items: [] })));
  }

  /**
   * Handle Http operation that failed.
   * Let the app continue.
   * @param operation - name of the operation that failed
   * @param result - optional value to return as the observable result
   */
  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error(`${operation} failed:`, error);
      
      // Lanzar el error para que el componente lo maneje
      throw error;
    };
  }
}
