import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-contact',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="contact-page py-5">
      <div class="container">
        <h1 class="h3 mb-4">İletişim</h1>
        <div class="row g-4">
          <div class="col-md-6">
            <div class="card p-4">
              <h5><i class="bi bi-geo-alt me-2"></i>Adres</h5>
              <p class="text-muted">İstanbul, Türkiye</p>
              <h5><i class="bi bi-telephone me-2"></i>Telefon</h5>
              <p class="text-muted">+90 212 123 45 67</p>
              <h5><i class="bi bi-envelope me-2"></i>E-posta</h5>
              <p class="text-muted">info&#64;eticaret.com</p>
            </div>
          </div>
          <div class="col-md-6">
            <div class="card p-4">
              <h5 class="mb-3">Bize Ulaşın</h5>
              <form>
                <div class="mb-3">
                  <input type="text" class="form-control" placeholder="Adınız" />
                </div>
                <div class="mb-3">
                  <input type="email" class="form-control" placeholder="E-posta" />
                </div>
                <div class="mb-3">
                  <textarea class="form-control" rows="4" placeholder="Mesajınız"></textarea>
                </div>
                <button type="submit" class="btn btn-primary">Gönder</button>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class ContactComponent {}