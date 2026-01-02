import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-account',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="account-page py-5">
      <div class="container">
        <h1 class="h3 mb-4"><i class="bi bi-person-circle me-2"></i>Hesabım</h1>
        <div class="row g-4">
          <div class="col-md-4">
            <div class="card">
              <div class="list-group list-group-flush">
                <a routerLink="/account" class="list-group-item list-group-item-action">
                  <i class="bi bi-person me-2"></i>Profil Bilgilerim
                </a>
                <a routerLink="/account/orders" class="list-group-item list-group-item-action">
                  <i class="bi bi-bag me-2"></i>Siparişlerim
                </a>
                <a routerLink="/account/wishlist" class="list-group-item list-group-item-action">
                  <i class="bi bi-heart me-2"></i>Favorilerim
                </a>
              </div>
            </div>
          </div>
          <div class="col-md-8">
            <div class="card p-4">
              <p class="text-muted text-center py-5">Hesap detayları yakında...</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class AccountComponent {}