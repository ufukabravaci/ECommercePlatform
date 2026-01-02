import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="wishlist-page py-5">
      <div class="container">
        <h1 class="h3 mb-4"><i class="bi bi-heart me-2"></i>Favorilerim</h1>
        <div class="text-center py-5">
          <i class="bi bi-heart display-1 text-muted"></i>
          <p class="text-muted mt-3">Favori listeniz boş.</p>
          <a routerLink="/products" class="btn btn-primary">Ürünleri İncele</a>
        </div>
      </div>
    </div>
  `
})
export class WishlistComponent {}