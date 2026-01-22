import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  
  constructor(private authService: AuthService) {}
  
  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // Obtener token
    const token = this.authService.getToken();
    
    // Agregar token a requests si está disponible
    if (token && this.shouldAddToken(request.url)) {
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }
    
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          // Token expirado o inválido, redirect a login
          this.authService.redirectToLogin();
        }
        return throwError(() => error);
      })
    );
  }
  
  private shouldAddToken(url: string): boolean {
    // No agregar token a requests de login
    return !url.includes('/connect/token') && !url.includes('/account/register');
  }
}
