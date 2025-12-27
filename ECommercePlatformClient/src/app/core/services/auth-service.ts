import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response';
import { RegisterRequest, ConfirmEmailRequest, LoginRequest, LoginResponse } from '../models/auth';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/auth`;

  // Müşteri kaydı
  register(request: RegisterRequest): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/register`, request);
  }

  // Email Doğrulama
  confirmEmail(request: ConfirmEmailRequest): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/confirm-email`, request);
  }

  // Login
  login(request: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    const payload = {
      ...request,
      companyId: environment.defaultTenantId // Storefront için sabit ID
    };
    return this.http.post<ApiResponse<LoginResponse>>(`${this.apiUrl}/login`, payload);
  }
}
