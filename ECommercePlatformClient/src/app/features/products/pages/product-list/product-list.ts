import { Component, inject, OnInit, signal, DestroyRef, ChangeDetectionStrategy, computed } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';
import { ProductService, CategoryService } from '../../../../core/services';
import { PaginationParams, ProductListItem, Category } from '../../../../core/models';
import { 
  ProductCardComponent, 
  PaginationComponent, 
  LoadingSpinnerComponent 
} from '../../../../shared/components';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    RouterModule,
    FormsModule,
    ProductCardComponent,
    PaginationComponent,
    LoadingSpinnerComponent,
  ],
  templateUrl: './product-list.html',
  styleUrl: './product-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductListComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  // Service signals
  readonly products = this.productService.products;
  readonly loading = this.productService.loading;
  readonly error = this.productService.error;
  readonly pagination = this.productService.pagination;
  readonly hasProducts = this.productService.hasProducts;
  readonly isEmpty = this.productService.isEmpty;
  readonly categories = this.categoryService.categories;

  // Local state
  readonly searchQuery = signal<string>('');
  readonly selectedCategory = signal<string>('');
  readonly pageSize = signal<number>(12);
  readonly sortBy = signal<string>('');
  readonly viewMode = signal<'grid' | 'list'>('grid');

  private readonly searchSubject = new Subject<string>();

  ngOnInit(): void {
    this.initializeFromQueryParams();
    this.setupSearchDebounce();
    this.loadCategories();
    this.loadProducts();
  }

  private initializeFromQueryParams(): void {
    this.route.queryParams.pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(params => {
      this.searchQuery.set(params['search'] || '');
      this.selectedCategory.set(params['category'] || '');
      this.pageSize.set(Number(params['pageSize']) || 12);
      this.sortBy.set(params['sort'] || '');
    });
  }

  private setupSearchDebounce(): void {
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => {
      this.loadProducts(1);
    });
  }

  private loadCategories(): void {
    if (this.categories().length === 0) {
      this.categoryService.loadCategories().pipe(
        takeUntilDestroyed(this.destroyRef)
      ).subscribe();
    }
  }

  loadProducts(page: number = 1): void {
    const [sortField, sortDir] = this.sortBy().split('-');

    const params: PaginationParams = {
      pageNumber: page,
      pageSize: this.pageSize(),
      search: this.searchQuery().trim(),
      sortBy: sortField || undefined,
      sortDirection: (sortDir as 'asc' | 'desc') || undefined,
      categoryId: this.selectedCategory() || undefined
    };

    this.updateQueryParams(params);

    this.productService.loadProducts(params).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe();
  }

  onSearchChange(value: string): void {
    this.searchQuery.set(value);
    this.searchSubject.next(value);
  }

  clearSearch(): void {
    this.searchQuery.set('');
    this.loadProducts(1);
  }

  onCategoryChange(categoryId: string): void {
    this.selectedCategory.set(categoryId);
    this.loadProducts(1);
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.loadProducts(1);
  }

  onSortChange(sort: string): void {
    this.sortBy.set(sort);
    this.loadProducts(1);
  }

  onPageChange(page: number): void {
    this.loadProducts(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  toggleViewMode(): void {
    this.viewMode.update(mode => mode === 'grid' ? 'list' : 'grid');
  }

  onAddToCart(product: ProductListItem): void {
    console.log('Sepete eklendi:', product);
    // TODO: Cart service entegrasyonu
  }

  clearFilters(): void {
    this.searchQuery.set('');
    this.selectedCategory.set('');
    this.sortBy.set('');
    this.pageSize.set(12);
    this.loadProducts(1);
  }

  readonly currentCategoryName = computed(() => {
    const id = this.selectedCategory();
    if (!id) return 'Tüm Ürünler';
    const category = this.categories().find(c => c.id === id);
    return category?.name || 'Tüm Ürünler';
  });

  private updateQueryParams(params: PaginationParams): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        page: params.pageNumber > 1 ? params.pageNumber : null,
        pageSize: params.pageSize !== 12 ? params.pageSize : null,
        search: params.search || null,
        sort: this.sortBy() || null,
        category: this.selectedCategory() || null
      },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }
}