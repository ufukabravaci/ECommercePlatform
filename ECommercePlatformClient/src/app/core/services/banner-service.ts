import { Injectable, signal } from '@angular/core';
import { BaseService } from './base-service';
import { Banner, ApiResponse, PageResult, PaginationParams } from '../models';
import { Observable, tap, finalize } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BannerService extends BaseService<Banner> {
  // State Signals
  private readonly _banners = signal<Banner[]>([]);
  private readonly _loading = signal<boolean>(false);
  
  readonly banners = this._banners.asReadonly();
  readonly loading = this._loading.asReadonly();

  constructor() {
    super('banners'); // api/banners
  }

  loadBanners(): Observable<ApiResponse<Banner[]>> {
    this._loading.set(true);
    // backendde pagination yok o yüzden burada direkt http.get kullandık.
    
    return this.http.get<ApiResponse<Banner[]>>(this.apiUrl).pipe(
      tap({
        next: (response) => {
          if (response.isSuccessful) {
            this._banners.set(response.data || []);
          }
        },
        error: (err) => console.error('Banner yüklenirken hata:', err)
      }),
      finalize(() => this._loading.set(false))
    );
  }
}
