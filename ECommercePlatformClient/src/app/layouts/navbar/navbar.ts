import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
  signal,
  ChangeDetectorRef,
  DestroyRef,
  HostListener,
  effect
} from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services';
import { CartService } from '../../core/services/cart-service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavbarComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly cartService = inject(CartService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly isMenuOpen = signal(false);
  readonly isSearchOpen = signal(false);
  readonly isScrolled = signal(false);

  readonly isAuthenticated = this.authService.isAuthenticated;
  readonly userFullName = this.authService.userFullName;
  readonly cartItemCount = this.cartService.itemCount;

   constructor() {
    // ÇÖZÜM: Signal değiştiğinde Navbar'ı render etmeye zorluyoruz.
    effect(() => {
      // Bağımlılıkları kaydediyoruz
      const count = this.cartItemCount();
      const auth = this.isAuthenticated();
      // UI güncellemesini tetikle
      this.cdr.detectChanges();
    });
  }

  ngOnInit(): void {
    if (this.isAuthenticated()) {
      this.loadBasketData();
    }
  }

  private loadBasketData(): void {
    this.cartService.loadBasket()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.cdr.detectChanges());
  }

  @HostListener('window:scroll')
  onScroll(): void {
    const scrolled = window.scrollY > 50;
    if (this.isScrolled() !== scrolled) {
      this.isScrolled.set(scrolled);
      this.cdr.detectChanges(); 
    }
  }

  toggleMenu(): void {
    this.isMenuOpen.update(v => !v);
    document.body.style.overflow = this.isMenuOpen() ? 'hidden' : '';
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
    this.cartService.clearBasket();
    this.closeMenu();
    this.cdr.detectChanges();
  }
}