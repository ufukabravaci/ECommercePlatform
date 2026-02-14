import {
  Component,
  inject,
  OnInit,
  OnDestroy,
  signal,
  computed,
  ChangeDetectionStrategy,
  DestroyRef,
  ChangeDetectorRef,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProductService } from '../../../core/services';
import type { ProductImage } from '../../../core/models';
import { LoadingSpinnerComponent } from '../../../shared/components';
import { formatCurrency } from '../../../core/utils/helper';
import { environment } from '../../../../environments/environment';
import { CartService } from '../../../core/services/cart-service';
import { CommonModule } from '@angular/common';
import { ProductReviewsComponent } from '../product-reviews/product-reviews';
import { ReviewService } from '../../../core/services/review-service';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [RouterLink, LoadingSpinnerComponent, CommonModule, RouterLink, ProductReviewsComponent],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductDetailComponent implements OnInit, OnDestroy {
  private readonly productService = inject(ProductService);
  private readonly cartService = inject(CartService);
  private readonly reviewService = inject(ReviewService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly product = this.productService.selectedProduct;
  readonly loading = this.productService.loading;
  readonly error = this.productService.error;
  readonly reviewCount = signal<number>(0);

  readonly selectedImage = signal<ProductImage | null>(null);
  readonly quantity = signal<number>(1);
  readonly activeTab = signal<'description' | 'details' | 'reviews'>('description');

  readonly formattedPrice = computed(() => {
    const p = this.product();
    return p ? formatCurrency(p.priceAmount, p.currencyCode) : '';
  });

  readonly stockStatus = computed(() => {
    const stock = this.product()?.stock ?? 0;
    if (stock <= 0) return { text: 'Stokta Yok', class: 'text-danger' };
    if (stock <= 5) return { text: `Son ${stock} Adet!`, class: 'text-warning' };
    return { text: 'Stokta Var', class: 'text-success' };
  });

  readonly isOutOfStock = computed(() => (this.product()?.stock ?? 0) <= 0);

  ngOnInit(): void {
    this.route.paramMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        const productId = params.get('id');
        if (productId) {
          this.loadProduct(productId);
        } else {
          this.router.navigate(['/products']);
        }
      });
  }

  ngOnDestroy(): void {
    this.productService.clearSelectedProduct();
  }

  loadProduct(id: string): void {
    this.quantity.set(1);
    this.activeTab.set('description');

    this.productService.loadProductDetail(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(response => {
        if (response.isSuccessful) {
          const p = response.data;
          const mainImage = p.images.find(img => img.isMain);
          this.selectedImage.set(mainImage || p.images[0] || null);
          this.loadReviewCount(id);
        }
        this.cdr.detectChanges();
      });
  }
  

  selectImage(image: ProductImage): void {
    this.selectedImage.set(image);
  }

  incrementQuantity(): void {
    const product = this.product();
    if (product && this.quantity() < product.stock) {
      this.quantity.update(q => q + 1);
    }
  }

  decrementQuantity(): void {
    if (this.quantity() > 1) {
      this.quantity.update(q => q - 1);
    }
  }

  updateQuantity(event: Event): void {
    const value = parseInt((event.target as HTMLInputElement).value, 10);
    const product = this.product();
    if (!product) return;

    if (isNaN(value) || value < 1) {
      this.quantity.set(1);
    } else if (value > product.stock) {
      this.quantity.set(product.stock);
    } else {
      this.quantity.set(value);
    }
  }

  addToCart(): void {
    const product = this.product();
    if (!product || product.stock <= 0) return;

    this.cartService.addToCart({
      productId: product.id,
      productName: product.name,
      priceAmount: product.priceAmount,
      priceCurrency: product.currencyCode,
      quantity: this.quantity(),
      imageUrl: product.mainImageUrl ?? undefined,
    })
    .pipe(takeUntilDestroyed(this.destroyRef))
    .subscribe(() => this.cdr.detectChanges());
  }

  setActiveTab(tab: 'description' | 'details' | 'reviews'): void {
    this.activeTab.set(tab);
  }

  normalizeImageUrl(url: string | null): string | null {
    if (!url) return null;
    if (url.startsWith('http')) return url;
    const baseUrl = environment.apiUrl.replace('/api', '');
    return `${baseUrl}${url}`;
  }
  addToWishlist(): void {
    const p = this.product();
    if (!p) return;

    // Şimdilik sadece konsola basalım veya bir uyarı verelim
    console.log(`${p.name} favorilere eklendi!`);
    
    // İleride buraya WishlistService eklendiğinde:
    // this.wishlistService.addToWishlist(p.id).subscribe(...)
    
    alert('Ürün favorilerinize eklendi! (Wishlist servisi yakında)');
  }

  private loadReviewCount(productId: string) {
    this.reviewService.getProductReviewCount(productId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(count => {
        this.reviewCount.set(count);
        this.cdr.detectChanges();
      });
  }
}