import { computed, Injectable, signal } from '@angular/core';
import { BaseService } from './base-service';
import { UserProfile } from '../models/user-profile';
import { ApiResponse } from '../models/api-response';
import { Observable, tap, finalize } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProfileService extends BaseService<UserProfile> {
  // --- State Signals ---
  private readonly _profile = signal<UserProfile | null>(null);
  private readonly _loading = signal<boolean>(false);
  private readonly _error = signal<string | null>(null);

  // --- Public Readonly Signals ---
  readonly profile = this._profile.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();

  // Computed Values
  readonly hasProfile = computed(() => !!this._profile());

  constructor() {
    super('profile'); // endpoint: api/profile
  }

  // --- API METHODS ---

  loadMyProfile(): Observable<ApiResponse<UserProfile>> {
    this._loading.set(true);
    this._error.set(null);

    return this.http.get<ApiResponse<UserProfile>>(this.apiUrl).pipe(
      tap({
        next: (response) => {
          if (response.isSuccessful && response.data) {
            this._profile.set(response.data);
          } else {
            this._error.set(response.errorMessages?.join(', ') || 'Profil bilgileri yüklenemedi.');
          }
        },
        error: (err) => {
          console.error('Profil yüklenirken hata:', err);
          this._error.set(err.message || 'Beklenmeyen bir hata oluştu.');
        }
      }),
      finalize(() => this._loading.set(false))
    );
  }

  updateProfile(data: Partial<UserProfile>): Observable<ApiResponse<string>> {
    this._loading.set(true);
    this._error.set(null);
    
    return this.http.put<ApiResponse<string>>(this.apiUrl, data).pipe(
      tap({
        next: (response) => {
          if (response.isSuccessful) {
            // Başarılı güncelleme sonrası local state'i güncelle
            this._profile.update(current => current ? { ...current, ...data } : null);
          } else {
             this._error.set(response.errorMessages?.join(', ') || 'Güncelleme başarısız.');
          }
        },
        error: (err) => {
          console.error('Profil güncellenirken hata:', err);
          this._error.set(err.message || 'Beklenmeyen bir hata oluştu.');
        }
      }),
      finalize(() => this._loading.set(false))
    );
  }
  
  clearError() {
    this._error.set(null);
  }
}