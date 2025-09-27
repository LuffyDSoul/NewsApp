import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { tap, catchError, map } from 'rxjs/operators';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse, RegisterRequest, UserProfile, CurrentUser, TokenInfo, UpdateProfileRequest, ChangePasswordRequest } from '../../shared/models/auth.model';

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

  // ABP Account/My-Profile - Usar API de ABP original como fallback
  getCurrentUser(): Observable<UserProfile> {
    // Primero intentar con la API personalizada
    return this.http.get<UserProfile>(`${environment.apiUrl}/api/user-profile/my-profile`).pipe(
      catchError(error => {
        console.warn('Custom profile API failed, trying ABP default API:', error);
        
        // Fallback: usar la API de ABP original
        return this.http.get<any>(`${environment.apiUrl}/api/account/my-profile`).pipe(
          map(abpProfile => {
            // Convertir respuesta de ABP al formato esperado
            const storedLanguage = localStorage.getItem('newsapp_preferred_language') || 'en';
            
            return {
              id: abpProfile.id,
              userName: abpProfile.userName || abpProfile.name || 'Unknown',
              email: abpProfile.email || '',
              name: abpProfile.name,
              surname: abpProfile.surname,
              emailConfirmed: abpProfile.emailConfirmed || false,
              phoneNumber: abpProfile.phoneNumber,
              preferredLanguage: storedLanguage // Usar localStorage como fallback
            } as UserProfile;
          }),
          catchError(fallbackError => {
            console.error('Both profile APIs failed:', fallbackError);
            // Si todo falla, al menos mantener el idioma del localStorage
            const storedLanguage = localStorage.getItem('newsapp_preferred_language') || 'en';
            
            // Crear un perfil mínimo usando datos del token o localStorage
            const userData = localStorage.getItem(this.userKey);
            if (userData) {
              const user = JSON.parse(userData);
              return of({
                id: user.id || '',
                userName: user.userName || 'User',
                email: user.email || '',
                name: user.name,
                surname: user.surname,
                emailConfirmed: true,
                phoneNumber: '',
                preferredLanguage: storedLanguage
              } as UserProfile);
            }
            
            throw fallbackError;
          })
        );
      })
    );
  }

  // Cargar información del usuario actual
  private loadCurrentUser(): void {
    if (this.getToken()) {
      console.log('📡 Loading current user profile...');
      
      this.getCurrentUser().subscribe({
        next: (profile: UserProfile) => {
          console.log('📡 Profile loaded successfully:', profile);
          
          const storedLanguage = localStorage.getItem('newsapp_preferred_language');
          
          const currentUser: CurrentUser = {
            isAuthenticated: true,
            id: profile.id,
            userName: profile.userName,
            email: profile.email,
            name: profile.name,
            surname: profile.surname,
            preferredLanguage: profile.preferredLanguage || storedLanguage || 'en', // ✅ Usar localStorage como fallback
            roles: [] // ABP puede proveer roles en otro endpoint
          };
          
          console.log('📡 Setting current user state:', currentUser);
          
          this.currentUserSubject.next(currentUser);
          localStorage.setItem(this.userKey, JSON.stringify(currentUser));
        },
        error: (error: any) => {
          console.error('📡 Error loading user profile:', error);
          
          // ✅ NUEVO: En lugar de limpiar sesión, intentar mantener estado básico
          const tokenData = localStorage.getItem(this.tokenKey);
          const userData = localStorage.getItem(this.userKey);
          
          if (tokenData && userData && !this.isTokenExpired()) {
            // Mantener usuario con datos del localStorage si el token sigue válido
            const user = JSON.parse(userData);
            console.log('📡 Maintaining user from localStorage:', user);
            this.currentUserSubject.next(user);
          } else {
            console.log('📡 Token expired or invalid, clearing session');
            this.clearSession();
          }
        }
      });
    }
  }

  // ✅ NUEVOS MÉTODOS PARA PERFIL DE USUARIO

  // Actualizar perfil de usuario
  updateProfile(profileData: UpdateProfileRequest): Observable<any> {
    // ✅ NUEVO: Logging para debug
    console.log('📡 AuthService.updateProfile called with:', profileData);
    console.log('📡 API URL:', `${environment.apiUrl}/api/user-profile/my-profile`);
    
    // ✅ NUEVO: Actualizar idioma inmediatamente en local como primera prioridad
    if (profileData.preferredLanguage) {
      this.updatePreferredLanguage(profileData.preferredLanguage);
    }
    
    // Intentar actualizar con API personalizada
    return this.http.put(`${environment.apiUrl}/api/user-profile/my-profile`, profileData).pipe(
      tap((response: any) => {
        console.log('📡 Update profile response:', response);
        
        // ✅ NUEVO: Asegurar que el idioma se mantenga aunque el backend falle
        if (profileData.preferredLanguage) {
          this.updatePreferredLanguage(profileData.preferredLanguage);
        }
        
        // Recargar datos del usuario después de actualizar
        this.loadCurrentUser();
      }),
      catchError((error: any) => {
        console.error('📡 Update profile error:', error);
        console.error('📡 Error status:', error.status);
        console.error('📡 Error body:', error.error);
        
        // ✅ NUEVO: Si la API personalizada falla, intentar con API básica de ABP
        const basicProfile = {
          userName: profileData.userName,
          email: profileData.email,
          name: profileData.name,
          surname: profileData.surname
          // Nota: ABP API original no soporta idioma, solo datos básicos
        };
        
        console.log('📡 Trying ABP original profile API as fallback...');
        
        // Fallback: usar API de ABP original (aunque no soporte idioma)
        return this.http.put(`${environment.apiUrl}/api/account/my-profile`, basicProfile).pipe(
          tap(() => {
            console.log('📡 ABP profile API succeeded, language saved locally');
            // El idioma ya se guardó localmente arriba
            this.loadCurrentUser();
          }),
          catchError((fallbackError: any) => {
            console.error('📡 Both profile APIs failed, keeping changes locally:', fallbackError);
            
            // ✅ NUEVO: Aunque todo falle, mantener cambios localmente
            if (profileData.preferredLanguage) {
              console.log('📡 All APIs failed, but keeping language preference locally');
              this.updatePreferredLanguage(profileData.preferredLanguage);
            }
            
            // Simular éxito parcial para UX
            return of({ 
              message: "Profile updated locally (language preference saved, other changes may require backend)" 
            });
          })
        );
      })
    );
  }

  // Cambiar contraseña
  changePassword(passwordData: ChangePasswordRequest): Observable<any> {
    console.log('📡 AuthService.changePassword called');
    
    return this.http.post(`${environment.apiUrl}/api/user-profile/my-profile/change-password`, passwordData).pipe(
      tap((response: any) => {
        console.log('📡 Change password response:', response);
      }),
      catchError((error: any) => {
        console.error('📡 Change password error:', error);
        throw error;
      })
    );
  }

  // Recargar datos del perfil
  refreshProfile(): Observable<UserProfile> {
    console.log('📡 AuthService.refreshProfile called');
    
    return this.getCurrentUser().pipe(
      tap((profile: UserProfile) => {
        console.log('📡 Refreshed profile data:', profile);
        
        const currentUser: CurrentUser = {
          isAuthenticated: true,
          id: profile.id,
          userName: profile.userName,
          email: profile.email,
          name: profile.name,
          surname: profile.surname,
          preferredLanguage: profile.preferredLanguage || 'en', // ✅ NUEVO: Idioma preferido
          roles: this.currentUserSubject.value.roles
        };
        
        console.log('📡 Updated currentUser state:', currentUser);
        
        this.currentUserSubject.next(currentUser);
        localStorage.setItem(this.userKey, JSON.stringify(currentUser));
      }),
      catchError((error: any) => {
        console.error('📡 Refresh profile error:', error);
        throw error;
      })
    );
  }

  // ✅ NUEVO: Obtener idioma preferido del usuario
  getPreferredLanguage(): string {
    const user = this.currentUserSubject.value;
    
    // Intentar obtener el idioma del usuario actual
    if (user.preferredLanguage) {
      return user.preferredLanguage;
    }
    
    // ✅ NUEVO: Fallback a localStorage para solución temporal
    const storedLanguage = localStorage.getItem('newsapp_preferred_language');
    if (storedLanguage) {
      return storedLanguage;
    }
    
    // Fallback final
    return 'en';
  }

  // ✅ NUEVO: Actualizar idioma preferido (versión temporal)
  updatePreferredLanguage(languageCode: string): void {
    const currentUser = this.currentUserSubject.value;
    
    if (currentUser.isAuthenticated) {
      // Actualizar el estado del usuario
      currentUser.preferredLanguage = languageCode;
      this.currentUserSubject.next(currentUser);
      localStorage.setItem(this.userKey, JSON.stringify(currentUser));
      
      // ✅ NUEVO: También guardarlo por separado como fallback
      localStorage.setItem('newsapp_preferred_language', languageCode);
      
      console.log('🌍 Language preference updated locally:', languageCode);
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
    console.log('📡 Loading user from storage...');
    
    const tokenData = localStorage.getItem(this.tokenKey);
    const userData = localStorage.getItem(this.userKey);
    
    if (tokenData && !this.isTokenExpired()) {
      console.log('📡 Valid token found');
      
      if (userData) {
        const user = JSON.parse(userData);
        console.log('📡 User data found in storage:', user);
        this.currentUserSubject.next(user);
        
        // Intentar recargar datos frescos del servidor en segundo plano
        setTimeout(() => {
          this.loadCurrentUser();
        }, 100);
      } else {
        console.log('📡 No user data, loading from server...');
        // Si hay token pero no datos de usuario, cargar del servidor
        this.loadCurrentUser();
      }
    } else {
      console.log('📡 No valid token found, clearing session');
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
