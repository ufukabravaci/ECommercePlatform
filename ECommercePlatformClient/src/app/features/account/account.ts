import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-account',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet, CommonModule], // RouterLinkActive ekledim
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './account.html', // HTML dosyasına referans
  styleUrls: ['./account.scss'] // SCSS dosyasına referans
})
export class AccountComponent {}
