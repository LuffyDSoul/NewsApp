import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { NewsArticleDto, NewsSearchDto, PagedResultDto, NewsSourceDto } from '../../shared/models/news.model';
import { AuthService } from './auth.service'; // ✅ NUEVO: Import AuthService

@Injectable({
  providedIn: 'root'
})
export class NewsService {
  private readonly baseUrl = `${environment.apiUrl}/news`;

  constructor(
    private http: HttpClient,
    private authService: AuthService // ✅ NUEVO: Inyectar AuthService
  ) {}

  // ✅ NUEVO: Obtener idioma preferido del usuario
  private getUserPreferredLanguage(): string {
    return this.authService.getPreferredLanguage();
  }

  // Búsqueda de noticias (ahora usa idioma preferido)
  searchNews(searchDto: NewsSearchDto): Observable<PagedResultDto<NewsArticleDto>> {
    // Si no se especifica idioma, usar el preferido del usuario
    if (!searchDto.language) {
      searchDto.language = this.getUserPreferredLanguage();
    }
    
    return this.http.post<PagedResultDto<NewsArticleDto>>(`${this.baseUrl}/search`, searchDto)
      .pipe(catchError(this.handleError<PagedResultDto<NewsArticleDto>>('searchNews', { totalCount: 0, items: [] })));
  }

  // Obtener titulares principales (ahora usa idioma preferido por defecto)
  getTopHeadlines(
    category?: string,
    country?: string,
    language?: string, // ✅ MODIFICADO: Ahora es opcional
    page: number = 1,
    pageSize: number = 20
  ): Observable<PagedResultDto<NewsArticleDto>> {
    // Si no se especifica idioma, usar el preferido del usuario
    const userLanguage = language || this.getUserPreferredLanguage();
    
    let params = new HttpParams()
      .set('language', userLanguage)
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (category) params = params.set('category', category);
    if (country) params = params.set('country', country);

    return this.http.get<PagedResultDto<NewsArticleDto>>(`${this.baseUrl}/get-top-headlines`, { params })
      .pipe(catchError(this.handleError<PagedResultDto<NewsArticleDto>>('getTopHeadlines', { totalCount: 0, items: [] })));
  }

  // Obtener noticias más recientes (ahora usa idioma preferido por defecto)
  getLatestNews(count: number = 10, languageCode?: string): Observable<NewsArticleDto[]> {
    // Si no se especifica idioma, usar el preferido del usuario
    const userLanguage = languageCode || this.getUserPreferredLanguage();
    
    let params = new HttpParams()
      .set('count', count.toString())
      .set('languageCode', userLanguage);

    return this.http.get<NewsArticleDto[]>(`${this.baseUrl}/get-latest`, { params })
      .pipe(catchError(this.handleError<NewsArticleDto[]>('getLatestNews', [])));
  }

  // Obtener fuentes de noticias (ahora usa idioma preferido por defecto)
  getSources(language?: string, country?: string): Observable<NewsSourceDto[]> {
    // Si no se especifica idioma, usar el preferido del usuario
    const userLanguage = language || this.getUserPreferredLanguage();
    
    let params = new HttpParams().set('language', userLanguage);
    if (country) params = params.set('country', country);

    return this.http.get<NewsSourceDto[]>(`${this.baseUrl}/get-sources`, { params })
      .pipe(catchError(this.handleError<NewsSourceDto[]>('getSources', [])));
  }

  // Obtener noticias de fuentes específicas (ahora usa idioma preferido por defecto)
  getNewsFromSources(
    sources: string,
    language?: string, // ✅ MODIFICADO: Ahora es opcional
    page: number = 1,
    pageSize: number = 20
  ): Observable<PagedResultDto<NewsArticleDto>> {
    // Si no se especifica idioma, usar el preferido del usuario
    const userLanguage = language || this.getUserPreferredLanguage();
    
    const params = new HttpParams()
      .set('sources', sources)
      .set('language', userLanguage)
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResultDto<NewsArticleDto>>(`${this.baseUrl}/get-from-sources`, { params })
      .pipe(catchError(this.handleError<PagedResultDto<NewsArticleDto>>('getNewsFromSources', { totalCount: 0, items: [] })));
  }

  // Buscar noticias localmente (ahora usa idioma preferido por defecto)
  searchLocalNews(
    searchText: string,
    languageCode?: string,
    skipCount: number = 0,
    maxResultCount: number = 10
  ): Observable<PagedResultDto<NewsArticleDto>> {
    // Si no se especifica idioma, usar el preferido del usuario
    const userLanguage = languageCode || this.getUserPreferredLanguage();
    
    let params = new HttpParams()
      .set('searchText', searchText)
      .set('languageCode', userLanguage)
      .set('skipCount', skipCount.toString())
      .set('maxResultCount', maxResultCount.toString());

    return this.http.get<PagedResultDto<NewsArticleDto>>(`${this.baseUrl}/search-local`, { params })
      .pipe(catchError(this.handleError<PagedResultDto<NewsArticleDto>>('searchLocalNews', { totalCount: 0, items: [] })));
  }

  // ✅ NUEVOS MÉTODOS: Métodos específicos con idioma del usuario
  
  // Obtener noticias en el idioma preferido del usuario
  getPersonalizedNews(count: number = 20): Observable<NewsArticleDto[]> {
    const userLanguage = this.getUserPreferredLanguage();
    return this.getLatestNews(count, userLanguage);
  }

  // Buscar noticias personalizadas
  searchPersonalizedNews(query: string, category?: string): Observable<PagedResultDto<NewsArticleDto>> {
    const userLanguage = this.getUserPreferredLanguage();
    
    const searchDto: NewsSearchDto = {
      query,
      category,
      language: userLanguage
    };
    
    return this.searchNews(searchDto);
  }

  // Obtener titulares personalizados
  getPersonalizedHeadlines(category?: string, country?: string): Observable<PagedResultDto<NewsArticleDto>> {
    const userLanguage = this.getUserPreferredLanguage();
    return this.getTopHeadlines(category, country, userLanguage);
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
