import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProductListItem } from '../../../core/models';
import { formatCurrency } from '../../../core/utils/helper';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './product-card.html',
  styleUrl: './product-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductCardComponent {
  readonly product = input.required<ProductListItem>();
  readonly addToCart = output<ProductListItem>();

  readonly formattedPrice = computed(() =>
    formatCurrency(this.product().priceAmount, this.product().currencyCode)
  );
  readonly isOutOfStock = computed(() => this.product()?.stock <= 0)
  readonly stockStatus = computed(() => {
    const stock = this.product()?.stock ?? 0;

    if (stock <= 0) {
      return { label: 'Stokta Yok', class: 'bg-danger' };
    }
    if (stock <= 5) {
      return { label: `Son ${stock} Adet`, class: 'bg-warning text-dark' };
    }
    return null;
  });

  onAddToCart(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    if (this.product().stock > 0) {
      this.addToCart.emit(this.product());
    }
  }

  normalizeImageUrl(url: string | null): string | null {
  if (!url) return null;
  // Zaten absolute ise hiç elleme
  if (url.startsWith('http://') || url.startsWith('https://')) {
    return url;
  }
  // Eski kayıtlar için
   const baseUrl = environment.apiUrl.replace('/api', '');
  return `${baseUrl}${url}`;
}
}
