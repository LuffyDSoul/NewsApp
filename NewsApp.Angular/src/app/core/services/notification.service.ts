import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface NotificationPreferenceDto {
  enableNotifications: boolean;
  enableDailySummary: boolean;
  preferredSummaryHour: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = `${environment.apiUrl}/notifications`;

  constructor(private http: HttpClient) { }

  /**
   * Obtener preferencias de notificación del usuario
   */
  getPreferences(): Observable<NotificationPreferenceDto> {
    return this.http.get<NotificationPreferenceDto>(`${this.apiUrl}/preferences`);
  }

  /**
   * Actualizar preferencias de notificación
   */
  updatePreferences(preferences: NotificationPreferenceDto): Observable<NotificationPreferenceDto> {
    return this.http.put<NotificationPreferenceDto>(`${this.apiUrl}/preferences`, preferences);
  }

  /**
   * Enviar email de prueba al usuario actual
   */
  sendTestNotification(): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/test`, {});
  }
}
