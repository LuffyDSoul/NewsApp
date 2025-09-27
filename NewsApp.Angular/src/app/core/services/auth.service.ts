import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { tap, catchError, map } from 'rxjs/operators';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse, RegisterRequest, UserProfile, CurrentUser, TokenInfo } from '../../shared/models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<CurrentUser>({ 
    isAuthenticated: false, 
    roles: [] 
  });
  
  public currentUser$ = this.currentUserSubject.asObservable();
  
  private readonly tokenKey = 'newsapp_token';
  private readonly userKey = 'newsapp_user';

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.loadUserFromStorage();
  }

  // ABP Connect/Token endpoint para OAuth
  login(credentials: LoginRequest): Observable<boolean> {
    const body = new HttpParams()
      .set('grant_type', 'password')
      .set('client_id', 'NewsApp_App')
      .set('username', credentials.userNameOrEmailAddress)
      .set('password', credentials.password)
      .set('scope', 'NewsApp');

    return this.http.post<LoginResponse>(`${environment.authUrl}/connect/token`, body.toString(), {
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded'
      }
    }).pipe(
      tap(response => {
        this.setSession(response);
        // Immediately update the authentication state
        this.updateAuthenticationState();
      }),
      map(() => true),
      catchError(error => {
        console.error('Login error:', error);
        return of(false);
      })
    );
  }

  // New method to immediately update authentication state
  private updateAuthenticationState(): void {
    const tokenData = localStorage.getItem(this.tokenKey);
    if (tokenData && !this.isTokenExpired()) {
      // Create a temporary authenticated state immediately
      const tempUser: CurrentUser = {
        isAuthenticated: true,
        userName: 'Loading...',
        email: 'Loading...',
        roles: []
      };
      this.currentUserSubject.next(tempUser);
      
      // Then load the full user data asynchronously
      this.loadCurrentUser();
    }
  }

  // ABP Account/Register
  register(userData: RegisterRequest): Observable<any> {
    return this.http.post(`${environment.apiUrl}/account/register`, {
      ...userData,
      appName: 'NewsApp'
    }).pipe(
      catchError(error => {
        console.error('Register error:', error);
        throw error;
      })
    );
  }

  // ABP Account/Logout
  logout(): Observable<any> {
    return this.http.post(`${environment.apiUrl}/account/logout`, {}).pipe(
      tap(() => {
        this.clearSession();
      }),
      catchError(() => {
        // Even if logout fails on server, clear local session
        this.clearSession();
        return of(null);
      })
    );
  }

  // ABP Account/My-Profile  
  getCurrentUser(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${environment.apiUrl}/account/my-profile`).pipe(
      catchError(error => {
        console.error('Get current user error:', error);
        this.logout();
        throw error;
      })
    );
  }

  // Cargar información del usuario actual
  private loadCurrentUser(): void {
    if (this.getToken()) {
      this.getCurrentUser().subscribe({
        next: (profile) => {
          const currentUser: CurrentUser = {
            isAuthenticated: true,
            id: profile.id,
            userName: profile.userName,
            email: profile.email,
            name: profile.name,
            surname: profile.surname,
            roles: [] // ABP puede proveer roles en otro endpoint
          };
          this.currentUserSubject.next(currentUser);
          localStorage.setItem(this.userKey, JSON.stringify(currentUser));
        },
        error: () => {
          this.clearSession();
        }
      });
    }
  }

  // Gestión de sesión
  private setSession(authResult: LoginResponse): void {
    const tokenInfo: TokenInfo = {
      access_token: authResult.access_token,
      expires_in: authResult.expires_in,
      token_type: authResult.token_type,
      refresh_token: authResult.refresh_token,
      expires_at: Date.now() + (authResult.expires_in * 1000)
    };
    
    localStorage.setItem(this.tokenKey, JSON.stringify(tokenInfo));
  }

  private clearSession(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.currentUserSubject.next({ 
      isAuthenticated: false, 
      roles: [] 
    });
  }

  private loadUserFromStorage(): void {
    const tokenData = localStorage.getItem(this.tokenKey);
    const userData = localStorage.getItem(this.userKey);
    
    if (tokenData && userData && !this.isTokenExpired()) {
      const user = JSON.parse(userData);
      this.currentUserSubject.next(user);
    } else {
      this.clearSession();
    }
  }

  // Utilidades
  getToken(): string | null {
    const tokenData = localStorage.getItem(this.tokenKey);
    if (tokenData) {
      const token: TokenInfo = JSON.parse(tokenData);
      if (!this.isTokenExpired(token)) {
        return token.access_token;
      }
    }
    return null;
  }

  isAuthenticated(): boolean {
    // Check both the current user state and token validity
    const hasValidToken = this.getToken() !== null;
    const userIsAuthenticated = this.currentUserSubject.value.isAuthenticated;
    
    return hasValidToken && userIsAuthenticated;
  }

  private isTokenExpired(token?: TokenInfo): boolean {
    if (!token) {
      const tokenData = localStorage.getItem(this.tokenKey);
      if (!tokenData) return true;
      token = JSON.parse(tokenData);
    }
    
    // Check if token is still undefined after parsing
    if (!token || !token.expires_at) {
      return true;
    }
    
    return Date.now() >= token.expires_at;
  }

  // Redirect helpers
  redirectToLogin(): void {
    this.router.navigate(['/auth/login']);
  }

  redirectAfterLogin(): void {
    this.router.navigate(['/news']);
  }
}
