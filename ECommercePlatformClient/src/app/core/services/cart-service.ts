import { Injectable, computed, signal } from '@angular/core';
import { Observable, finalize, tap } from 'rxjs';
import { BaseService } from './base-service'; 
import { ApiResponse } from '../models/api-response';

export interface BasketItem {
  productId: string;
  productName: string;
  priceAmount: number;
  priceCurrency: string;
  quantity: number;
  imageUrl?: string | null;
}

export interface CustomerBasket {
  customerId: string;
  items: BasketItem[];
}

@Injectable({
  providedIn: 'root',
})
export class CartService extends BaseService<CustomerBasket> {
  // State Signals
  private readonly _basket = signal<CustomerBasket | null>(null);
  private readonly _loading = signal<boolean>(false);
  private readonly _error = signal<string | null>(null);

  // Public Signals
  readonly basket = this._basket.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();

  // Derived (Computed) Signals
  readonly itemCount = computed(() =>
    (this._basket()?.items ?? []).reduce((sum, item) => sum + item.quantity, 0)
  );

  readonly isEmpty = computed(
    () => !this._basket() || (this._basket()!.items?.length ?? 0) === 0
  );

  readonly totalAmount = computed(() =>
    (this._basket()?.items ?? []).reduce(
      (sum, item) => sum + item.priceAmount * item.quantity,
      0
    )
  );

  constructor() {
    super('baskets');
    // Servis başladığında sepeti yüklemeye gerek yok, 
    // bunu Navbar veya Cart componenti gerektiğinde çağıracak.
  }

  // --- Helpers ---
  private setLoading(value: boolean): void {
    this._loading.set(value);
  }

  // State güncelleme mantığı
  private applyBasketResult(result: ApiResponse<CustomerBasket>): void {
    if (result.isSuccessful && result.data) {
      this._basket.set({
        customerId: result.data.customerId,
        items: [...(result.data.items ?? [])], // Yeni referans
      });
      this._error.set(null);
    } else {
      this._error.set(
        result.errorMessages?.join(', ') || 'Sepet güncellenemedi'
      );
    }
  }

  // ---- API METHODS ----

  // 1) Get Basket
  loadBasket(): Observable<ApiResponse<CustomerBasket>> {
    this.setLoading(true);
    this._error.set(null);

    return this.http.get<ApiResponse<CustomerBasket>>(`${this.apiUrl}/`).pipe(
      tap({
        next: (res) => this.applyBasketResult(res),
        error: (err) => this._error.set(this.handleError(err))
      }),
      finalize(() => this.setLoading(false))
    );
  }

  // Ortak güncelleme metodu
  private postUpdatedItems(items: BasketItem[]): Observable<ApiResponse<CustomerBasket>> {
    this.setLoading(true);
    this._error.set(null);

    // Optimistik Update: Servis tarafında sepeti hemen güncelleyebiliriz
    // Ama Backend'den hata gelirse geri almak zor olabilir. 
    // Şimdilik sadece Backend dönüşünü bekliyoruz.

    const body = { items };

    return this.http.post<ApiResponse<CustomerBasket>>(`${this.apiUrl}/`, body).pipe(
      tap({
        next: (res) => this.applyBasketResult(res),
        error: (err) => this._error.set(this.handleError(err))
      }),
      finalize(() => this.setLoading(false))
    );
  }

  // 2) Add Item
  addToCart(input: {
    productId: string;
    productName: string;
    priceAmount: number;
    priceCurrency: string;
    quantity: number;
    imageUrl?: string | null;
  }): Observable<ApiResponse<CustomerBasket>> {
    const current = this._basket() ?? { customerId: '', items: [] };
    const items = current.items ?? [];
    const existing = items.find((i) => i.productId === input.productId);

    let updatedItems: BasketItem[];

    if (existing) {
      updatedItems = items.map((i) =>
        i.productId === input.productId
          ? { ...i, quantity: i.quantity + input.quantity }
          : i
      );
    } else {
      updatedItems = [
        ...items,
        {
          productId: input.productId,
          productName: input.productName,
          priceAmount: input.priceAmount,
          priceCurrency: input.priceCurrency,
          quantity: input.quantity,
          imageUrl: input.imageUrl ?? null,
        },
      ];
    }

    return this.postUpdatedItems(updatedItems);
  }

  // 3) Update Quantity
  updateItemQuantity(productId: string, quantity: number): Observable<ApiResponse<CustomerBasket>> | null {
    if (quantity < 1) return null;

    const current = this._basket();
    if (!current || !current.items) return null;

    const updatedItems = current.items.map((i) =>
      i.productId === productId ? { ...i, quantity } : i
    );

    return this.postUpdatedItems(updatedItems);
  }

  // 4) Remove Item
  removeItem(productId: string): Observable<ApiResponse<CustomerBasket>> | null {
    const current = this._basket();
    if (!current || !current.items) return null;

    const updatedItems = current.items.filter((i) => i.productId !== productId);
    return this.postUpdatedItems(updatedItems);
  }

  // 5) Clear Basket
  clearBasket(): Observable<ApiResponse<string>> {
    this.setLoading(true);
    this._error.set(null);

    return this.http.delete<ApiResponse<string>>(`${this.apiUrl}/`).pipe(
      tap({
        next: (res) => {
          if (res.isSuccessful) {
            // Sepeti boşalt
            this._basket.set({ customerId: this._basket()?.customerId ?? '', items: [] });
          } else {
            this._error.set(res.errorMessages?.join(', ') || 'Sepet temizlenemedi');
          }
        },
        error: (err) => this._error.set(this.handleError(err))
      }),
      finalize(() => this.setLoading(false))
    );
  }
}
