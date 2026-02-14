import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Product } from '../../../core/models'; // ProductListItem -> Product oldu
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [CommonModule, RouterLink], // CommonModule eklendi (ngClass, currency vb için)
  templateUrl: './product-card.html',
  styleUrl: './product-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductCardComponent {
  // Input Signal (ProductListItem -> Product)
  readonly product = input.required<Product>();
  
  // Output Event
  readonly addToCart = output<Product>();

  // Computed Values
  readonly isOutOfStock = computed(() => (this.product().stock ?? 0) <= 0);

  readonly stockStatus = computed(() => {
    const stock = this.product().stock ?? 0;
    if (stock <= 0) {
      return { label: 'TÜKENDİ', class: 'badge-danger' };
    }
    if (stock <= 5) {
      return { label: `SON ${stock} ADET`, class: 'badge-warning' };
    }
    // İndirim varsa onu gösterelim, yoksa YENİ etiketi vs. (Sizin badge mantığına göre)
    return null; 
  });

  // Action Method
  onAddToCart(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    if (!this.isOutOfStock()) {
      this.addToCart.emit(this.product());
    }
  }

  // Image URL Helper
  get imageUrl(): string {
    const url = this.product().mainImageUrl;
    // 1. Resim yoksa placeholder
    if (!url) return 'assets/images/placeholder.jpg';
    // 2. Tam URL ise (http/https) dokunma (CDN vb.)
    if (url.startsWith('http')) return url;
    // 3. Base64 ise dokunma
    if (url.startsWith('data:image')) return url;
    const apiUrl = environment.apiUrl; // "http://localhost:5000/api"
    const baseUrl = apiUrl.replace('/api', ''); // "http://localhost:5000"
    // Eğer url "/" ile başlamıyorsa ekle
    const normalizedUrl = url.startsWith('/') ? url : `/${url}`;
    return `${baseUrl}${normalizedUrl}`;
  }
}
