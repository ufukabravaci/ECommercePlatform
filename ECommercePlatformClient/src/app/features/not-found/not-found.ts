import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="not-found-page min-vh-100 d-flex align-items-center justify-content-center bg-light">
      <div class="text-center">
        <h1 class="display-1 fw-bold text-primary">404</h1>
        <h2 class="mb-3">Sayfa Bulunamadı</h2>
        <p class="text-muted mb-4">Aradığınız sayfa mevcut değil veya taşınmış olabilir.</p>
        <div class="d-flex gap-3 justify-content-center">
          <a routerLink="/" class="btn btn-primary">
            <i class="bi bi-house me-2"></i>Ana Sayfa
          </a>
          <a routerLink="/products" class="btn btn-outline-primary">
            <i class="bi bi-grid me-2"></i>Ürünler
          </a>
        </div>
      </div>
    </div>
  `
})
export class NotFoundComponent {}