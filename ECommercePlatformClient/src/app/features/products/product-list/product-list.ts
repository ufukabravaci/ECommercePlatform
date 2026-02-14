import {
  Component,
  inject,
  OnInit,
  signal,
  DestroyRef,
  ChangeDetectionStrategy,
  computed,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';

import { ProductService, CategoryService, CartService } from '../../../core/services';
import { PaginationParams, Product } from '../../../core/models';
import {
  ProductCardComponent,
  PaginationComponent,
  LoadingSpinnerComponent
} from '../../../shared/components';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    ProductCardComponent,
    LoadingSpinnerComponent,
    PaginationComponent
],
  templateUrl: './product-list.html',
  styleUrl: './product-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductListComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);
  private readonly cartService = inject(CartService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  
  // Service Signals
  readonly products = this.productService.products;
  readonly loading = this.productService.loading;
  readonly error = this.productService.error;
  readonly pagination = this.productService.pagination;
  readonly categories = this.categoryService.categories;

  // Local State Signals
  readonly searchQuery = signal<string>('');
  readonly selectedCategory = signal<string>('');
  readonly pageSize = signal<number>(12);
  readonly sortBy = signal<string>('newest');
  readonly viewMode = signal<'grid' | 'list'>('grid');

  // Helpers
  readonly hasProducts = computed(() => this.products().length > 0);
  readonly isEmpty = computed(() => !this.loading() && this.products().length === 0);
  
  readonly currentCategoryName = computed(() => {
    const id = this.selectedCategory();
    if (!id) return 'Tüm Ürünler';
    const cat = this.categories().find(c => c.id === id);
    return cat ? cat.name : 'Tüm Ürünler';
  });

  private readonly searchSubject = new Subject<string>();

  ngOnInit(): void {
    // 1. Kategorileri Yükle
    this.categoryService.loadCategories().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();

    // 2. URL Parametrelerini Dinle
    this.route.queryParams
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        this.searchQuery.set(params['search'] || '');
        this.selectedCategory.set(params['category'] || '');
        this.pageSize.set(Number(params['pageSize']) || 12);
        this.sortBy.set(params['sort'] || 'newest');
        
        // Ürünleri yükle
        this.loadProducts(Number(params['page']) || 1);
      });

    // 3. Arama Debounce
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(query => {
      this.updateQueryParams({ search: query || null, page: 1 });
    });
  }

  // --- ACTIONS ---

  loadProducts(page: number): void {
    // Sorting Mapping (Backend formatına çevir)
    const [field, dir] = this.sortBy().split('-'); 
    let sortField = 'createdat'; 
    
    if (field === 'price') sortField = 'price';
    if (field === 'name') sortField = 'name';

    const params: PaginationParams = {
      pageNumber: page,
      pageSize: this.pageSize(),
      search: this.searchQuery(),
      categoryId: this.selectedCategory() || undefined,
      sortBy: sortField,
      sortDirection: (dir as 'asc' | 'desc') || 'desc'
    };

    this.productService.loadProducts(params)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe();
  }

  // Event Handlers
  onSearchChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchQuery.set(value);
    this.searchSubject.next(value);
  }

  clearSearch(): void {
    this.searchQuery.set('');
    this.updateQueryParams({ search: null, page: 1 });
  }

  onCategoryChange(categoryId: string): void {
    // Eğer aynı kategoriye tıklandıysa kaldır (opsiyonel)
    if (this.selectedCategory() === categoryId) {
      this.clearCategory();
    } else {
      this.selectedCategory.set(categoryId);
      this.updateQueryParams({ category: categoryId, page: 1 });
    }
  }

  clearCategory(): void {
    this.selectedCategory.set('');
    this.updateQueryParams({ category: null, page: 1 });
  }

  onSortChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.sortBy.set(value);
    this.updateQueryParams({ sort: value, page: 1 });
  }

  onPageSizeChange(event: Event): void {
    const value = Number((event.target as HTMLSelectElement).value);
    this.pageSize.set(value);
    this.updateQueryParams({ pageSize: value, page: 1 });
  }

  onPageChange(page: number): void {
    this.updateQueryParams({ page });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  setViewMode(mode: 'grid' | 'list'): void {
    this.viewMode.set(mode);
  }

  toggleFilters(): void {
    // Mobile filter toggle logic
  }

  clearAllFilters(): void {
    this.searchQuery.set('');
    this.selectedCategory.set('');
    this.sortBy.set('newest');
    
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {}
    });
  }

  // Cart Action
  onAddToCart(product: Product): void {
    if ((product.stock ?? 0) <= 0) return;

    this.cartService.addToCart({
      productId: product.id,
      productName: product.name,
      priceAmount: product.priceAmount,
      priceCurrency: product.currencyCode,
      quantity: 1,
      imageUrl: product.mainImageUrl ?? undefined,
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => console.log('Sepete eklendi'),
      error: (err) => console.error('Hata', err)
    });
  }

  // Helper
  private updateQueryParams(params: any): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: params,
      queryParamsHandling: 'merge', // Mevcut parametreleri koru, yenileri ekle/güncelle
    });
  }
}
