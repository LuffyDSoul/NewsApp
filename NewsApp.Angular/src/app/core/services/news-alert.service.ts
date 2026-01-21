import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface NewsAlertListDto {
  id: string;
  userId: string;
  name: string;
  description?: string;
  categories: string;
  languageCode: string;
  keyword?: string;
  isActive: boolean;
  lastCheckedAt?: Date;
  lastNewsFoundAt?: Date;
  creationTime: Date;
  creatorId?: string;
  lastModificationTime?: Date;
  lastModifierId?: string;
}

export interface CreateNewsAlertListDto {
  name: string;
  description?: string;
  categories: string;
  languageCode: string;
  keyword?: string;
  isActive: boolean;
}

export interface UpdateNewsAlertListDto {
  name: string;
  description?: string;
  categories: string;
  languageCode: string;
  keyword?: string;
  isActive: boolean;
}

export interface NewsAlertNotificationDto {
  id: string;
  userId: string;
  newsAlertListId: string;
  alertListName: string;
  category: string;
  languageCode: string;
  newArticlesCount: number;
  isRead: boolean;
  emailSent: boolean;
  emailSentAt?: Date;
  newestArticleDate: Date;
  creationTime: Date;
  articleUrls: string;
}

@Injectable({
  providedIn: 'root'
})
export class NewsAlertService {
  private readonly baseUrl = `${environment.apiUrl}/news-alerts`;
  
  // Subject to notify components when notifications should be refreshed
  private notificationsRefresh$ = new Subject<void>();
  
  // Observable for components to subscribe to
  onNotificationsRefresh = this.notificationsRefresh$.asObservable();

  constructor(private http: HttpClient) {}

  // Method to trigger refresh
  refreshNotifications(): void {
    this.notificationsRefresh$.next();
  }

  // Obtener mis alertas
  getMyAlerts(isActive?: boolean): Observable<NewsAlertListDto[]> {
    let params = new HttpParams();
    if (isActive !== undefined) {
      params = params.set('isActive', isActive.toString());
    }
    return this.http.get<NewsAlertListDto[]>(this.baseUrl, { params });
  }

  // Obtener una alerta específica
  getAlert(id: string): Observable<NewsAlertListDto> {
    return this.http.get<NewsAlertListDto>(`${this.baseUrl}/${id}`);
  }

  // Crear alerta
  createAlert(input: CreateNewsAlertListDto): Observable<NewsAlertListDto> {
    return this.http.post<NewsAlertListDto>(this.baseUrl, input);
  }

  // Actualizar alerta
  updateAlert(id: string, input: UpdateNewsAlertListDto): Observable<NewsAlertListDto> {
    return this.http.put<NewsAlertListDto>(`${this.baseUrl}/${id}`, input);
  }

  // Eliminar alerta
  deleteAlert(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  // Obtener mis notificaciones
  getMyNotifications(unreadOnly?: boolean, maxCount: number = 50): Observable<NewsAlertNotificationDto[]> {
    let params = new HttpParams().set('maxCount', maxCount.toString());
    if (unreadOnly !== undefined) {
      params = params.set('unreadOnly', unreadOnly.toString());
    }
    return this.http.get<NewsAlertNotificationDto[]>(`${this.baseUrl}/notifications`, { params });
  }

  // Obtener conteo de no leídas
  getUnreadCount(): Observable<number> {
    return this.http.get<number>(`${this.baseUrl}/notifications/unread-count`);
  }

  // Marcar notificación como leída
  markAsRead(id: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/notifications/${id}/read`, {});
  }

  // Marcar todas como leídas
  markAllAsRead(): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/notifications/read-all`, {});
  }
}
