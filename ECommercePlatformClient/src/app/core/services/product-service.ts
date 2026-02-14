import { Injectable, signal, computed } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { BaseService } from './base-service';
import {
  ApiResponse,
  PageResult,
  PaginationParams,
  Product
} from '../models';

@Injectable({
  providedIn: 'root',
})
export class ProductService extends BaseService<Product> {
  // Signals
  private readonly _products = signal<Product[]>([]);
  private readonly _selectedProduct = signal<Product | null>(null);
  private readonly _loading = signal<boolean>(false);
  private readonly _error = signal<string | null>(null);
  
  // Pagination State
  private readonly _pagination = signal<Omit<PageResult<Product>, 'items'>>({
    pageNumber: 1,
    pageSize: 12,
    totalCount: 0,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false
  });

  // Public Signals
  readonly products = this._products.asReadonly();
  readonly selectedProduct = this._selectedProduct.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly pagination = this._pagination.asReadonly();
  readonly hasProducts = computed(() => this._products().length > 0);

  constructor() {
    super('products'); // api/products
  }

  // Override etmeden BaseService metodunu kullanabiliriz ama state (signal) güncellemek istiyoruz.
  loadProducts(params: PaginationParams): Observable<ApiResponse<PageResult<Product>>> {
    this._loading.set(true);
    this._error.set(null);

    // BaseService'deki getAll metodunu çağırıyoruz
    return this.getAll(params).pipe(
      tap({
        next: (response) => {
          this._loading.set(false);
          if (response.isSuccessful) {
            this._products.set(response.data.items);
            
            // Meta verileri ayır
            const { items, ...meta } = response.data;
            this._pagination.set(meta);
          } else {
            this._error.set(response.errorMessages?.join(', ') || 'Ürünler yüklenemedi');
          }
        },
        error: (err: any) => { // Error tipi any veya HttpErrorResponse
          this._loading.set(false);
          this._error.set(err.message || 'Bir hata oluştu');
        }
      })
    );
  }

  loadProductDetail(id: string): Observable<ApiResponse<Product>> {
    this._loading.set(true);
    this._selectedProduct.set(null);
    this._error.set(null);

    // BaseService'deki getById metodunu çağırıyoruz
    return this.getById(id).pipe(
      tap({
        next: (response) => {
          this._loading.set(false);
          if (response.isSuccessful) {
            this._selectedProduct.set(response.data);
          } else {
            this._error.set(response.errorMessages?.join(', ') || 'Ürün detayı bulunamadı');
          }
        },
        error: (err: any) => {
          this._loading.set(false);
          this._error.set(err.message || 'Bağlantı hatası');
        }
      })
    );
  }

  clearSelectedProduct(): void {
    this._selectedProduct.set(null);
  }
}
