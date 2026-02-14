import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CategoryService } from '../../core/services';
import { LoadingSpinnerComponent } from '../../shared/components';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [RouterLink, LoadingSpinnerComponent],
  templateUrl: './categories.html',
  styleUrl: './categories.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CategoriesComponent implements OnInit {
  readonly categoryService = inject(CategoryService);

  ngOnInit(): void {
    if (this.categoryService.categories().length === 0) {
      this.categoryService.loadCategories().subscribe();
    }
  }
}
