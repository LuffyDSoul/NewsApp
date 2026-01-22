import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  UserProfile, 
  UpdateUserProfile, 
  ChangePassword, 
  NewsLanguage, 
  ProfileUpdateResult 
} from '../../shared/models/user-profile.model';

@Injectable({
  providedIn: 'root'
})
export class UserProfileService {
  private readonly apiUrl = `${environment.apiUrl}/app/user-profile`;

  constructor(private http: HttpClient) {
    console.log('UserProfileService initialized with apiUrl:', this.apiUrl);
  }

  /**
   * Get current user's profile information
   */
  getMyProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(this.apiUrl);
  }

  /**
   * Update current user's profile information
   */
  updateMyProfile(profile: UpdateUserProfile): Observable<ProfileUpdateResult> {
    return this.http.put<ProfileUpdateResult>(this.apiUrl, profile);
  }

  /**
   * Change current user's password
   */
  changePassword(passwordData: ChangePassword): Observable<ProfileUpdateResult> {
    return this.http.post<ProfileUpdateResult>(`${this.apiUrl}/change-password`, passwordData);
  }

  /**
   * Get available languages for news
   */
  getAvailableNewsLanguages(): Observable<NewsLanguage[]> {
    return this.http.get<NewsLanguage[]>(`${this.apiUrl}/news-languages`);
  }

  /**
   * Update user's preferred news language
   */
  updateNewsLanguage(languageCode: string): Observable<ProfileUpdateResult> {
    return this.http.post<ProfileUpdateResult>(`${this.apiUrl}/news-language`, JSON.stringify(languageCode), {
      headers: {
        'Content-Type': 'application/json'
      }
    });
  }

  /**
   * Send email confirmation to current user
   */
  sendEmailConfirmation(): Observable<ProfileUpdateResult> {
    return this.http.post<ProfileUpdateResult>(`${this.apiUrl}/send-email-confirmation`, {});
  }

  /**
   * Confirm email with token
   */
  confirmEmail(token: string): Observable<ProfileUpdateResult> {
    return this.http.post<ProfileUpdateResult>(`${this.apiUrl}/confirm-email`, JSON.stringify(token), {
      headers: {
        'Content-Type': 'application/json'
      }
    });
  }
}
