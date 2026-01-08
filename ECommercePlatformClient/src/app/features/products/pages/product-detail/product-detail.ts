// import { Component, inject, OnInit, signal, DestroyRef, ChangeDetectionStrategy } from '@angular/core';
// import { ActivatedRoute, Router, RouterLink } from '@angular/router';
// import { CurrencyPipe } from '@angular/common';
// import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
// import { ProductService } from '../../../../core/services';
// import { Product, ProductImage } from '../../../../core/models';
// import { LoadingSpinnerComponent } from '../../../../shared/components';
// import { formatCurrency } from '../../../../core/utils/helper';
// import { environment } from '../../../../../environments/environment';


// @Component({
//   selector: 'app-product-detail',
//   standalone: true,
//   imports: [RouterLink, LoadingSpinnerComponent], //CurrencyPipe
//   templateUrl: './product-detail.html',
//   styleUrl: './product-detail.scss',
//   changeDetection: ChangeDetectionStrategy.OnPush
// })
// export class ProductDetailComponent implements OnInit {
//   private readonly productService = inject(ProductService);
//   private readonly route = inject(ActivatedRoute);
//   private readonly router = inject(Router);
//   private readonly destroyRef = inject(DestroyRef);

//   // Service signals
//   readonly product = this.productService.selectedProduct;
//   readonly loading = this.productService.loading;
//   readonly error = this.productService.error;

//   // Local state
//   readonly selectedImage = signal<ProductImage | null>(null);
//   readonly quantity = signal<number>(1);
//   readonly activeTab = signal<'description' | 'details' | 'reviews'>('description');

//   ngOnInit(): void {
//     this.route.paramMap.pipe(
//       takeUntilDestroyed(this.destroyRef)
//     ).subscribe(params => {
//       const productId = params.get('id');
//       if (productId) {
//         this.loadProduct(productId);
//       } else {
//         this.router.navigate(['/products']);
//       }
//     });
//   }

//   private loadProduct(id: string): void {
//     this.productService.loadProductDetail(id).pipe(
//       takeUntilDestroyed(this.destroyRef)
//     ).subscribe({
//       next: (response) => {
//         if (response.isSuccessful && response.data) {
//           // Set main image as selected
//           const mainImage = response.data.images.find(img => img.isMain);
//           this.selectedImage.set(mainImage || response.data.images[0] || null);
//         }
//       }
//     });
//   }

//   selectImage(image: ProductImage): void {
//     this.selectedImage.set(image);
//   }

//   incrementQuantity(): void {
//     const product = this.product();
//     if (product && this.quantity() < product.stock) {
//       this.quantity.update(q => q + 1);
//     }
//   }

//   decrementQuantity(): void {
//     if (this.quantity() > 1) {
//       this.quantity.update(q => q - 1);
//     }
//   }

//   updateQuantity(event: Event): void {
//     const value = parseInt((event.target as HTMLInputElement).value, 10);
//     const product = this.product();
//     if (product) {
//       if (value < 1) {
//         this.quantity.set(1);
//       } else if (value > product.stock) {
//         this.quantity.set(product.stock);
//       } else {
//         this.quantity.set(value);
//       }
//     }
//   }

//   addToCart(): void {
//     const product = this.product();
//     if (product && product.stock > 0) {
//       console.log('Sepete eklendi:', { product, quantity: this.quantity() });
//       // TODO: Cart service entegrasyonu
//     }
//   }

//   addToWishlist(): void {
//     const product = this.product();
//     if (product) {
//       console.log('Favorilere eklendi:', product);
//       // TODO: Wishlist service entegrasyonu
//     }
//   }

//   setActiveTab(tab: 'description' | 'details' | 'reviews'): void {
//     this.activeTab.set(tab);
//   }

//   getFormattedPrice(product: Product): string {
//     return formatCurrency(product.priceAmount, product.currencyCode);
//   }

//   getStockStatus(stock: number): { text: string; class: string } {
//     if (stock <= 0) {
//       return { text: 'Stokta Yok', class: 'text-danger' };
//     }
//     if (stock <= 5) {
//       return { text: `Son ${stock} Adet!`, class: 'text-warning' };
//     }
//     return { text: 'Stokta Var', class: 'text-success' };
//   }
//   normalizeImageUrl(url: string | null): string | null {
//     if (!url) return null;
//     // Zaten absolute ise hiç elleme
//     if (url.startsWith('http://') || url.startsWith('https://')) {
//       return url;
//     }
//     // Eski kayıtlar için
//      const baseUrl = environment.apiUrl.replace('/api', '');
//     return `${baseUrl}${url}`;
//   }
// }

import { Component, inject, OnInit, signal, computed, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { ProductService } from '../../../../core/services';
import type { Product, ProductImage } from '../../../../core/models';
import { LoadingSpinnerComponent } from '../../../../shared/components';
import { formatCurrency } from '../../../../core/utils/helper';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [RouterLink, LoadingSpinnerComponent],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductDetailComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  // Service signals
  readonly product = this.productService.selectedProduct;
  readonly loading = this.productService.loading;
  readonly error = this.productService.error;

  // Local state
  readonly selectedImage = signal<ProductImage | null>(null);
  readonly quantity = signal<number>(1);
  readonly activeTab = signal<'description' | 'details' | 'reviews'>('description');

  // Computed
  readonly formattedPrice = computed(() => {
    const p = this.product();
    return p ? formatCurrency(p.priceAmount, p.currencyCode) : '';
  });

  readonly stockStatus = computed(() => {
    const stock = this.product()?.stock ?? 0;
    if (stock <= 0) {
      return { text: 'Stokta Yok', class: 'text-danger' };
    }
    if (stock <= 5) {
      return { text: `Son ${stock} Adet!`, class: 'text-warning' };
    }
    return { text: 'Stokta Var', class: 'text-success' };
  });

  readonly isOutOfStock = computed(() => (this.product()?.stock ?? 0) <= 0);

  ngOnInit(): void {
    this.route.paramMap.pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(params => {
      const productId = params.get('id');
      if (productId) {
        this.loadProduct(productId);
      } else {
        this.router.navigate(['/products']);
      }
    });
  }

  loadProduct(id: string): void {
    this.quantity.set(1);
    this.activeTab.set('description');
    
    this.productService.loadProductDetail(id).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (response) => {
        if (response.isSuccessful && response.data) {
          const mainImage = response.data.images.find(img => img.isMain);
          this.selectedImage.set(mainImage || response.data.images[0] || null);
        }
      }
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
    if (product) {
      if (isNaN(value) || value < 1) {
        this.quantity.set(1);
      } else if (value > product.stock) {
        this.quantity.set(product.stock);
      } else {
        this.quantity.set(value);
      }
    }
  }

  addToCart(): void {
    const product = this.product();
    if (product && product.stock > 0) {
      console.log('Sepete eklendi:', { product, quantity: this.quantity() });
      // TODO: Cart service entegrasyonu
    }
  }

  addToWishlist(): void {
    const product = this.product();
    if (product) {
      console.log('Favorilere eklendi:', product);
      // TODO: Wishlist service entegrasyonu
    }
  }

  setActiveTab(tab: 'description' | 'details' | 'reviews'): void {
    this.activeTab.set(tab);
  }

  normalizeImageUrl(url: string | null): string | null {
    if (!url) return null;
    // Zaten absolute ise hiç elleme
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url;
    }
    // Eski kayıtlar için
     const baseUrl = environment.apiUrl.replace('/api', '');
    return `${baseUrl}${url}`;
  }
}