import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CategoryService } from '../../core/services';
import { LoadingSpinnerComponent } from '../../shared/components';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [RouterLink, LoadingSpinnerComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="categories-page py-5">
      <div class="container">
        <h1 class="h3 mb-4"><i class="bi bi-tags me-2"></i>Kategoriler</h1>
        
        @if (categoryService.loading()) {
          <app-loading-spinner />
        } @else {
          <div class="row g-4">
            @for (category of categoryService.categories(); track category.id) {
              <div class="col-6 col-md-4 col-lg-3">
                <a 
                  [routerLink]="['/products']" 
                  [queryParams]="{ category: category.id }"
                  class="card h-100 text-decoration-none text-center p-4 category-card"
                >
                  <i class="bi bi-tag display-4 text-primary mb-3"></i>
                  <h5 class="card-title mb-0">{{ category.name }}</h5>
                </a>
              </div>
            } @empty {
              <div class="col-12 text-center py-5">
                <p class="text-muted">Henüz kategori bulunmuyor.</p>
              </div>
            }
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .category-card {
      transition: all 0.2s ease;
      border: 1px solid #e9ecef;
      &:hover {
        transform: translateY(-5px);
        box-shadow: 0 10px 30px rgba(0,0,0,0.1);
        border-color: var(--bs-primary);
      }
    }
  `]
})
export class CategoriesComponent implements OnInit {
  readonly categoryService = inject(CategoryService);

  ngOnInit(): void {
    if (this.categoryService.categories().length === 0) {
      this.categoryService.loadCategories().subscribe();
    }
  }
}