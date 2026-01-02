import { ChangeDetectionStrategy, Component, computed, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CategoryService, ProductService } from '../../../core/services';
import { LoadingSpinnerComponent, ProductCardComponent } from '../../../shared/components';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, ProductCardComponent, LoadingSpinnerComponent],
  templateUrl: './home.html',
  styleUrl: './home.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomeComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);

  readonly products = this.productService.products;
  readonly loading = this.productService.loading;
  readonly categories = this.categoryService.categories;
  readonly initialLoading = computed(() => {
    return this.loading() && this.products().length === 0;
  });

  ngOnInit(): void {
    this.loadFeaturedProducts();
    this.categoryService.loadCategories().subscribe();
  }

  private loadFeaturedProducts(): void {
    this.productService.loadProducts({
      pageNumber: 1,
      pageSize: 8,
      sortBy: 'createdAt',
      sortDirection: 'desc'
    }).subscribe();
  }

  onAddToCart(product: unknown): void {
    console.log('Sepete eklendi:', product);
    // TODO: Cart service entegrasyonu
  }
}