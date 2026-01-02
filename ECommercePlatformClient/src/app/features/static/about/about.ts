import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-about',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="about-page py-5">
      <div class="container">
        <h1 class="h3 mb-4">Hakkımızda</h1>
        <div class="card p-4">
          <p>E-Ticaret platformumuza hoş geldiniz. Müşteri memnuniyeti odaklı hizmet anlayışımızla sizlere en iyi alışveriş deneyimini sunmayı hedefliyoruz.</p>
        </div>
      </div>
    </div>
  `
})
export class AboutComponent {}