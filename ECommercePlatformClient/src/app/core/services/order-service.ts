import { Injectable, signal, computed, inject } from '@angular/core';
import { Observable, tap, finalize } from 'rxjs';
import { BaseService } from './base-service';
import { ApiResponse, PageResult, PaginationParams } from '../models';
import { CreateOrderRequest, OrderDetail, OrderListItem } from '../models/checkout';
import { CartService } from './cart-service';

@Injectable({
  providedIn: 'root'
})
export class OrderService extends BaseService<OrderListItem> {
  private cartService = inject(CartService);

  // --- State Signals ---
  private readonly _myOrders = signal<OrderListItem[]>([]);
  private readonly _currentOrder = signal<OrderDetail | null>(null);
  private readonly _loading = signal<boolean>(false);
  private readonly _error = signal<string | null>(null);

  // Pagination State (BaseService yapısı için gerekli)
  private readonly _pagination = signal<Omit<PageResult<any>, 'items'>>({
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false
  });

  // --- Public Readonly Signals ---
  readonly myOrders = this._myOrders.asReadonly();
  readonly currentOrder = this._currentOrder.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly pagination = this._pagination.asReadonly();

  readonly hasOrders = computed(() => this._myOrders().length > 0);

  constructor() {
    super('orders'); // Base URL: /api/orders
  }

  // --- API METHODS ---

  createOrder(request: CreateOrderRequest): Observable<ApiResponse<string>> {
    this._loading.set(true);
    this._error.set(null);

    return this.http.post<ApiResponse<string>>(this.apiUrl, request).pipe(
      tap({
        next: (response) => {
          if (response.isSuccessful) {
            this.cartService.clearBasket().subscribe(); 
          } else {
            this._error.set(response.errorMessages?.join(', ') || 'Sipariş oluşturulamadı');
          }
        },
        error: (err) => {
           this._loading.set(false);
           this._error.set(err.message || 'Bir hata oluştu');
        }
      }),
      finalize(() => this._loading.set(false))
    );
  }

  // PageResult yapısına uygun loadMyOrders
  loadMyOrders(params: PaginationParams): Observable<ApiResponse<PageResult<OrderListItem>>> {
    this._loading.set(true);
    this._error.set(null);

    const httpParams = this.buildParams(params);

    return this.http.get<ApiResponse<PageResult<OrderListItem>>>(`${this.apiUrl}/my-orders`, { params: httpParams }).pipe(
      tap({
        next: (response) => {
          this._loading.set(false);
          if (response.isSuccessful) {
            this._myOrders.set(response.data.items);
            
            // Meta verileri ayırıp pagination signal'ine ata
            const { items, ...meta } = response.data;
            this._pagination.set(meta);

          } else {
            this._error.set(response.errorMessages?.join(', ') || 'Siparişler yüklenemedi');
          }
        },
        error: (err) => {
           this._loading.set(false);
           this._error.set(err.message || 'Bir hata oluştu');
        }
      })
    );
  }

  loadOrderDetail(orderNumber: string): Observable<ApiResponse<OrderDetail>> {
    this._loading.set(true);
    this._error.set(null);
    this._currentOrder.set(null);

    return this.http.get<ApiResponse<OrderDetail>>(`${this.apiUrl}/${orderNumber}`).pipe(
      tap({
        next: (response) => {
          if (response.isSuccessful && response.data) {
            this._currentOrder.set(response.data);
          } else {
            this._error.set(response.errorMessages?.join(', ') || 'Sipariş detayı bulunamadı');
          }
        },
        error: (err) => {
             this._loading.set(false);
             this._error.set(err.message || 'Bir hata oluştu');
        }
      }),
      finalize(() => this._loading.set(false))
    );
  }

  clearCurrentOrder() {
    this._currentOrder.set(null);
  }

  clearError() {
    this._error.set(null);
  }
}
