import { 
  ChangeDetectionStrategy, 
  ChangeDetectorRef, 
  Component, 
  computed, 
  DestroyRef, 
  inject, 
  OnInit 
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CategoryService, ProductService } from '../../../core/services';
import { LoadingSpinnerComponent, ProductCardComponent } from '../../../shared/components';
import { Product } from '../../../core/models';
import { CartService } from '../../../core/services/cart-service';
import { BannerService } from '../../../core/services/banner-service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, ProductCardComponent, LoadingSpinnerComponent],
  templateUrl: './home.html',
  styleUrl: './home.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);
  private readonly cartService = inject(CartService);
  private readonly bannerService = inject(BannerService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly products = this.productService.products;
  readonly loading = this.productService.loading;
  readonly categories = this.categoryService.categories;
  readonly banners = this.bannerService.banners;
  readonly loadingBanners = this.bannerService.loading;

  readonly initialLoading = computed(
    () => this.loading() && this.products().length === 0
  );

  ngOnInit(): void {
    this.bannerService.loadBanners().subscribe();
    this.loadFeaturedProducts();
    this.loadCategories();
  }

  onAddToCart(product: Product): void {
    if (product.stock <= 0) return;

    this.cartService.addToCart({
      productId: product.id,
      productName: product.name,
      priceAmount: product.priceAmount,
      priceCurrency: product.currencyCode,
      quantity: 1,
      imageUrl: product.mainImageUrl ?? undefined,
    })
    .pipe(takeUntilDestroyed(this.destroyRef))
    .subscribe({
      next: () => {
        console.log('Sepete eklendi');
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
        this.cdr.detectChanges();
      }
    });
  }

  private loadFeaturedProducts(): void {
    this.productService.loadProducts({
      pageNumber: 1,
      pageSize: 8,
      sortBy: 'createdAt',
      sortDirection: 'desc',
    })
    .pipe(takeUntilDestroyed(this.destroyRef))
    .subscribe(() => this.cdr.detectChanges());
  }

  private loadCategories(): void {
    this.categoryService.loadCategories()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.cdr.detectChanges());
  }
  getBannerImageUrl(url: string): string {
    // 1. Resim yoksa placeholder
    if (!url) return 'assets/images/placeholder.jpg';
    // 2. Tam URL ise (http/https) dokunma (CDN vb.)
    if (url.startsWith('http')) return url;
    // 3. Base64 ise dokunma
    if (url.startsWith('data:image')) return url;
    const apiUrl = environment.apiUrl; // "http://localhost:5000/api"
    const baseUrl = apiUrl.replace('/api', ''); // "http://localhost:5000"
    // Eğer url "/" ile başlamıyorsa ekle
    const normalizedUrl = url.startsWith('/') ? url : `/${url}`;
    return `${baseUrl}${normalizedUrl}`;
  }
  isExternalLink(url: string): boolean {
  // Eğer url http:// veya https:// ile başlıyorsa dış bağlantıdır
  return url.startsWith('http://') || url.startsWith('https://');
}
}