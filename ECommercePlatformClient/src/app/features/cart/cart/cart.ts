import { CurrencyPipe, CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
  computed
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { BasketItem, CartService } from '../../../core/services/cart-service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [RouterLink, CurrencyPipe, CommonModule],
  templateUrl: './cart.html',
  styleUrl: './cart.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CartComponent implements OnInit {
  private readonly cartService = inject(CartService);

  // --- Service Signals (View Model) ---
  // Template doğrudan bunları kullanır. Değiştiklerinde otomatik render olur.
  readonly loading = this.cartService.loading;
  readonly error = this.cartService.error;
  readonly totalAmount = this.cartService.totalAmount;
  readonly isEmpty = this.cartService.isEmpty;

  // Items: Servis sinyalinden türetilir.
  readonly items = computed(() => this.cartService.basket()?.items ?? ([] as BasketItem[]));

  ngOnInit(): void {
    // Veriyi yükle - Subscribe oluyoruz ama callback BOMBOŞ.
    // Çünkü servis kendi içindeki signal'i güncelleyecek.
    // Signal güncellenince 'items' computed'ı tetiklenecek -> Template güncellenecek.
    this.cartService.loadBasket().subscribe();
  }

  // --- Actions (Fire and Forget) ---

  updateQuantity(productId: string, quantity: number): void {
    if (quantity < 1) return;
    
    // API isteğini at, sonucu bekleme. Servis halledecek.
    this.cartService.updateItemQuantity(productId, quantity)?.subscribe();
  }

  removeItem(productId: string): void {
    if (!confirm('Ürünü sepetten kaldırmak istediğinize emin misiniz?')) return;

    this.cartService.removeItem(productId)?.subscribe();
  }

  clearCart(): void {
    if (!confirm('Tüm sepeti boşaltmak istediğinize emin misiniz?')) return;

    this.cartService.clearBasket().subscribe();
  }

  // --- UI Helpers ---
  trackByProductId = (_: number, item: BasketItem) => item.productId;

  normalizeImageUrl(url: string | null): string | null {
    if (!url) return null;
    if (url.startsWith('http')) return url;
    
    const cleanUrl = url.startsWith('/') ? url : `/${url}`;
    const baseUrl = environment.apiUrl.replace('/api', '').replace(/\/+$/, '');
    return `${baseUrl}${cleanUrl}`;
  }
}
