import { Component, input, output, computed, ChangeDetectionStrategy } from '@angular/core';

export interface PaginationData {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

@Component({
  selector: 'app-pagination',
  standalone: true,
  templateUrl: './pagination.html',
  styleUrl: './pagination.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PaginationComponent {
  readonly pagination = input.required<PaginationData>();
  readonly pageChange = output<number>();

  readonly visiblePages = computed(() => {
    const current = this.pagination().pageNumber;
    const total = this.pagination().totalPages;
    const delta = 2;
    const pages: number[] = [];
    // Always show first page
    pages.push(1);
    // Calculate range around current page
    const start = Math.max(2, current - delta);
    const end = Math.min(total - 1, current + delta);
    // Add ellipsis indicator (-1) if needed
    if (start > 2) {
      pages.push(-1);
    }
    // Add pages in range
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    // Add ellipsis before last page if needed
    if (end < total - 1) {
      pages.push(-1);
    }
    // Always show last page if more than 1 page
    if (total > 1) {
      pages.push(total);
    }
    return pages;
  });

  readonly showPagination = computed(() => this.pagination().totalPages > 1);

  goToPage(page: number): void {
    const { pageNumber, totalPages } = this.pagination();
    if (page >= 1 && page <= totalPages && page !== pageNumber) {
      this.pageChange.emit(page);
    }
  }

  goToFirst(): void {
    this.goToPage(1);
  }

  goToLast(): void {
    this.goToPage(this.pagination().totalPages);
  }

  goToPrevious(): void {
    this.goToPage(this.pagination().pageNumber - 1);
  }

  goToNext(): void {
    this.goToPage(this.pagination().pageNumber + 1);
  }
}