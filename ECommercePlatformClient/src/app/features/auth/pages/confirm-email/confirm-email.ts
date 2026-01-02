import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services';

@Component({
  selector: 'app-confirm-email',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="confirm-email-page min-vh-100 d-flex align-items-center py-5 bg-light">
      <div class="container">
        <div class="row justify-content-center">
          <div class="col-md-6 col-lg-5">
            <div class="card shadow-sm text-center p-5">
              @if (loading()) {
                <div class="spinner-border text-primary mx-auto mb-3" role="status"></div>
                <p>E-posta doğrulanıyor...</p>
              } @else if (success()) {
                <i class="bi bi-check-circle-fill text-success display-1 mb-3"></i>
                <h4>E-posta Doğrulandı!</h4>
                <p class="text-muted">Hesabınız aktifleştirildi.</p>
                <a routerLink="/auth/login" class="btn btn-primary">Giriş Yap</a>
              } @else {
                <i class="bi bi-x-circle-fill text-danger display-1 mb-3"></i>
                <h4>Doğrulama Başarısız</h4>
                <p class="text-muted">{{ error() }}</p>
                <a routerLink="/" class="btn btn-outline-primary">Ana Sayfaya Dön</a>
              }
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class ConfirmEmailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly authService = inject(AuthService);

  readonly loading = signal(true);
  readonly success = signal(false);
  readonly error = signal<string>('');

  ngOnInit(): void {
    const email = this.route.snapshot.queryParamMap.get('email');
    const token = this.route.snapshot.queryParamMap.get('token');

    if (email && token) {
      this.authService.confirmEmail({ email, token }).subscribe({
        next: (res) => {
          this.loading.set(false);
          this.success.set(res.isSuccessful);
          if (!res.isSuccessful) {
            this.error.set(res.errorMessages?.join(', ') || 'Doğrulama başarısız');
          }
        },
        error: (err) => {
          this.loading.set(false);
          this.error.set(err.message || 'Bir hata oluştu');
        }
      });
    } else {
      this.loading.set(false);
      this.error.set('Geçersiz doğrulama linki');
    }
  }
}