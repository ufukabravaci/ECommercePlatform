import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

interface CartItem {
  id: string;
  productId: string;
  name: string;
  price: number;
  currencyCode: string;
  quantity: number;
  imageUrl: string | null;
}

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [RouterLink, CurrencyPipe],
  templateUrl: './cart.html',
  styleUrl: './cart.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CartComponent {
  readonly cartItems = signal<CartItem[]>([]);
  readonly loading = signal<boolean>(false);

  readonly isEmpty = computed(() => this.cartItems().length === 0 )
  readonly totalAmount = computed(() => this.cartItems().reduce((sum, item) => sum + (item.price * item.quantity), 0));

  updateQuantity(itemId: string, quantity: number): void {
    if (quantity < 1) return;
    this.cartItems.update(items => 
      items.map(item => 
        item.id === itemId ? { ...item, quantity } : item
      )
    );
  }

  removeItem(itemId: string): void {
    this.cartItems.update(items => items.filter(item => item.id !== itemId));
  }

  clearCart(): void {
    this.cartItems.set([]);
  }
}