import { computed, inject, Injectable, signal } from '@angular/core';
import {
  ApiResponse,
  ConfirmEmailRequest,
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  UserInfo,
  RefreshTokenResponse,
} from '../models';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly apiUrl = `${environment.apiUrl}/auth`;

  // Signals
  private readonly _isAuthenticated = signal<boolean>(this.hasValidToken());
  private readonly _currentUser = signal<UserInfo | null>(this.getUserFromStorage());
  private readonly _loading = signal<boolean>(false);

  // Public readonly signals
  readonly isAuthenticated = this._isAuthenticated.asReadonly();
  readonly currentUser = this._currentUser.asReadonly();
  readonly currentUserId = computed(() => this.currentUser()?.id);
  readonly loading = this._loading.asReadonly();

  // Computed
  readonly userFullName = computed(() => {
    const user = this._currentUser();
    return user ? `${user.firstName} ${user.lastName}` : '';
  });

  register(request: RegisterRequest): Observable<ApiResponse<string>> {
    this._loading.set(true);
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/register`, request).pipe(
      tap({
        next: () => this._loading.set(false),
        error: () => this._loading.set(false),
      })
    );
  }

  confirmEmail(request: ConfirmEmailRequest): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/confirm-email`, request);
  }

  login(request: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    this._loading.set(true);

    return this.http.post<ApiResponse<LoginResponse>>(`${this.apiUrl}/login`, request).pipe(
      tap({
        next: (response) => {
          if (response.isSuccessful && response.data) {
            this.handleAuthentication(response.data.accessToken!, response.data.refreshToken!);
          }
          this._loading.set(false);
        },
        error: () => this._loading.set(false),
      })
    );
  }

  refreshToken(): Observable<ApiResponse<RefreshTokenResponse>> {
    const refreshToken = localStorage.getItem('refresh_token');

    if (!refreshToken) {
      this.logout();
      return throwError(() => new Error('No refresh token found'));
    }

    return this.http
      .post<ApiResponse<RefreshTokenResponse>>(`${this.apiUrl}/refresh-token`, {
        refreshToken: refreshToken,
      })
      .pipe(
        tap((response) => {
          if (response.isSuccessful && response.data) {
            this.handleAuthentication(response.data.accessToken, response.data.refreshToken);
          }
        })
      );
  }

  logout(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('user_info');

    this._isAuthenticated.set(false);
    this._currentUser.set(null);
    this.router.navigate(['/']);
  }

  // --- Yardımcı Metotlar ---

  private handleAuthentication(accessToken: string, refreshToken: string): void {
    this.setTokens(accessToken, refreshToken);

    // Token'dan kullanıcı bilgisini çöz
    const user = this.decodeToken(accessToken);
    this._currentUser.set(user);
    this._isAuthenticated.set(true);

    // Kullanıcı bilgisini sakla (Sayfa yenilendiğinde gitmesin)
    if (user) {
      localStorage.setItem('user_info', JSON.stringify(user));
    }
  }

  private setTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem('access_token', accessToken);
    localStorage.setItem('refresh_token', refreshToken);
  }

  private hasValidToken(): boolean {
    const token = localStorage.getItem('access_token');
    if (!token) return false;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp * 1000 > Date.now();
    } catch {
      return false;
    }
  }

  private getUserFromStorage(): UserInfo | null {
    const userStr = localStorage.getItem('user_info');
    if (!userStr) return null;
    try {
      return JSON.parse(userStr);
    } catch {
      return null;
    }
  }

  // JWT Token Decode (Angular tarafında external library kullanmadan)
  private decodeToken(token: string): UserInfo | null {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));

      return {
        id: payload.sub, // tek kaynak
        email: payload.email,
        firstName: payload.given_name || '',
        lastName: payload.family_name || '',
        companyId: payload.companyId,
        roles: Array.isArray(payload.role) ? payload.role : payload.role ? [payload.role] : [],
      };
    } catch {
      return null;
    }
  }
}
