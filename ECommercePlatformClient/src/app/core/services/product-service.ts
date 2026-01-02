import { Injectable, signal, computed } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { BaseService } from './base-service';
import { 
  ApiResponse, 
  PaginatedResult, 
  PaginationParams,
  Product, 
  ProductListItem 
} from '../models';

@Injectable({
  providedIn: 'root',
})
export class ProductService extends BaseService<ProductListItem> {
  // Signals for state management
  private readonly _products = signal<ProductListItem[]>([]);
  private readonly _selectedProduct = signal<Product | null>(null);
  private readonly _loading = signal<boolean>(false);
  private readonly _error = signal<string | null>(null);
  private readonly _pagination = signal<Omit<PaginatedResult<unknown>, 'items'>>({
    pageNumber: 1,
    pageSize: 12,
    totalCount: 0,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false
  });

  // Public readonly signals
  readonly products = this._products.asReadonly();
  readonly selectedProduct = this._selectedProduct.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly pagination = this._pagination.asReadonly();

  // Computed signals
  readonly hasProducts = computed(() => this._products().length > 0);
  readonly isEmpty = computed(() => !this._loading() && this._products().length === 0);
  readonly totalItems = computed(() => this._pagination().totalCount);

  constructor() {
    super('products');
  }

  loadProducts(params: PaginationParams): Observable<ApiResponse<PaginatedResult<ProductListItem>>> {
    this._loading.set(true);
    this._error.set(null);

    return this.getAll(params).pipe(
      tap({
        next: (response) => {
          if (response.isSuccessful) {
            this._products.set(response.data.items);
            this._pagination.set({
              pageNumber: response.data.pageNumber,
              pageSize: response.data.pageSize,
              totalCount: response.data.totalCount,
              totalPages: response.data.totalPages,
              hasNextPage: response.data.hasNextPage,
              hasPreviousPage: response.data.hasPreviousPage
            });
          } else {
            this._error.set(response.errorMessages?.join(', ') || 'Ürünler yüklenemedi');
          }
          this._loading.set(false);
        },
        error: (err) => {
          this._error.set(err.message || 'Bir hata oluştu');
          this._loading.set(false);
        }
      })
    );
  }

  getProductDetail(id: string): Observable<ApiResponse<Product>> {
    return this.http.get<ApiResponse<Product>>(`${this.apiUrl}/${id}`);
  }

  loadProductDetail(id: string): Observable<ApiResponse<Product>> {
    this._loading.set(true);
    this._error.set(null);
    this._selectedProduct.set(null);

    return this.getProductDetail(id).pipe(
      tap({
        next: (response) => {
          if (response.isSuccessful) {
            this._selectedProduct.set(response.data);
          } else {
            this._error.set(response.errorMessages?.join(', ') || 'Ürün detayı yüklenemedi');
          }
          this._loading.set(false);
        },
        error: (err) => {
          this._error.set(err.message || 'Bir hata oluştu');
          this._loading.set(false);
        }
      })
    );
  }

  uploadImage(productId: string, file: File, isMain: boolean): Observable<ApiResponse<string>> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('isMain', String(isMain));
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/${productId}/images`, formData);
  }

  clearSelectedProduct(): void {
    this._selectedProduct.set(null);
  }

  clearError(): void {
    this._error.set(null);
  }
}