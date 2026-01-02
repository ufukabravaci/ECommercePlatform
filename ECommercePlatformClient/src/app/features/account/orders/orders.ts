import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-orders',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  template: `
    <div class="orders-page py-5">
      <div class="container">
        <h1 class="h3 mb-4"><i class="bi bi-bag me-2"></i>Siparişlerim</h1>
        <div class="text-center py-5">
          <i class="bi bi-bag-x display-1 text-muted"></i>
          <p class="text-muted mt-3">Henüz siparişiniz bulunmuyor.</p>
          <a routerLink="/products" class="btn btn-primary">Alışverişe Başla</a>
        </div>
      </div>
    </div>
  `
})
export class OrdersComponent {}