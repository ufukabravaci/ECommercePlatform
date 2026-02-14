import { Injectable, signal } from '@angular/core';
import { Observable, tap, finalize, map } from 'rxjs';
import { BaseService } from './base-service';
import { ApiResponse, PageResult, PaginationParams } from '../models';
import { CreateReviewRequest, ReviewDto } from '../models/review';

@Injectable({
  providedIn: 'root'
})
export class ReviewService extends BaseService<ReviewDto> {
  
  // State Signals
  private readonly _reviews = signal<ReviewDto[]>([]);
  private readonly _loading = signal<boolean>(false);
  private readonly _pagination = signal<Omit<PageResult<any>, 'items'>>({
     pageNumber: 1, pageSize: 10, totalCount: 0, totalPages: 0, hasNextPage: false, hasPreviousPage: false 
  });

  // Public Signals
  readonly reviews = this._reviews.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly pagination = this._pagination.asReadonly();

  constructor() {
    super('reviews'); // api/reviews
  }

  // Ürüne ait yorumları getir
  loadProductReviews(productId: string, params: PaginationParams): Observable<ApiResponse<PageResult<ReviewDto>>> {
    this._loading.set(true);

    // Varsayım: GET /api/reviews/product/{productId}
    const httpParams = this.buildParams(params);

    return this.http.get<ApiResponse<PageResult<ReviewDto>>>(`${this.apiUrl}/product/${productId}`, { params: httpParams }).pipe(
      tap({
        next: (response) => {
          if (response.isSuccessful) {
            this._reviews.set(response.data.items);
            const { items, ...meta } = response.data;
            this._pagination.set(meta);
          }
        }
      }),
      finalize(() => this._loading.set(false))
    );
  }

  // Yorum Ekle
  createReview(request: CreateReviewRequest): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(this.apiUrl, request);
  }

  deleteReview(id: string): Observable<ApiResponse<string>> {
    return this.http.delete<ApiResponse<string>>(`${this.apiUrl}/${id}`);
  }

  // Sadece toplam sayıyı öğrenmek için
getProductReviewCount(productId: string): Observable<number> {
  const params: PaginationParams = { pageNumber: 1, pageSize: 0 }; // Sadece meta veri için
  const httpParams = this.buildParams(params);

  return this.http.get<ApiResponse<PageResult<ReviewDto>>>(`${this.apiUrl}/product/${productId}`, { params: httpParams })
    .pipe(
      map(res => res.data?.totalCount || 0) // Sadece sayıyı dön
    );
}
}
