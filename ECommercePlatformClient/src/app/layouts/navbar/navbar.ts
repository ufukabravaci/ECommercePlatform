import { Component, inject, signal, ChangeDetectionStrategy, HostListener } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NavbarComponent {
  private readonly authService = inject(AuthService);

  readonly isMenuOpen = signal(false);
  readonly isSearchOpen = signal(false);
  readonly isScrolled = signal(false);
  readonly isAuthenticated = this.authService.isAuthenticated;
  readonly userFullName = this.authService.userFullName;

  @HostListener('window:scroll')
  onScroll(): void {
    this.isScrolled.set(window.scrollY > 50);
  }

  toggleMenu(): void {
    this.isMenuOpen.update(v => !v);
    if (this.isMenuOpen()) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
  }

  closeMenu(): void {
    this.isMenuOpen.set(false);
    document.body.style.overflow = '';
  }

  toggleSearch(): void {
    this.isSearchOpen.update(v => !v);
  }

  logout(): void {
    this.authService.logout();
    this.closeMenu();
  }
}