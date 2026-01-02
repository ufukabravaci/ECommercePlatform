import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Category, ApiResponse, CategoryTreeNode } from '../models';

@Injectable({
  providedIn: 'root',
})
export class CategoryService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/categories`;

  // Signals
  private readonly _categories = signal<Category[]>([]);
  private readonly _loading = signal<boolean>(false);
  private readonly _error = signal<string | null>(null);

  // Public readonly signals
  readonly categories = this._categories.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();

  // Computed: Tree structure
  readonly categoryTree = computed(() => this.buildTree(this._categories()));
  
  // Computed: Flat list for dropdowns
  readonly categoriesFlat = computed(() => this._categories());

  loadCategories(): Observable<ApiResponse<Category[]>> {
    this._loading.set(true);
    this._error.set(null);

    return this.http.get<ApiResponse<Category[]>>(this.apiUrl).pipe(
      tap({
        next: (response) => {
          if (response.isSuccessful) {
            this._categories.set(response.data);
          } else {
            this._error.set(response.errorMessages?.join(', ') || 'Kategoriler yüklenemedi');
          }
          this._loading.set(false);
        },
        error: (err) => {
          this._error.set(err.message || 'Bir hata oluştu');
          this._loading.set(false);
        }
      })
    );
  }

  getCategoryById(id: string): Category | undefined {
    return this._categories().find(c => c.id === id);
  }

  private buildTree(categories: Category[]): CategoryTreeNode[] {
    const map = new Map<string, CategoryTreeNode>();
    const roots: CategoryTreeNode[] = [];

    // First pass: create nodes
    categories.forEach(cat => {
      map.set(cat.id, { 
        ...cat, 
        level: 0, 
        expanded: false, 
        children: [] 
      });
    });

    // Second pass: build tree
    categories.forEach(cat => {
      const node = map.get(cat.id)!;
      if (cat.parentId && map.has(cat.parentId)) {
        const parent = map.get(cat.parentId)!;
        parent.children = parent.children || [];
        parent.children.push(node);
        node.level = parent.level + 1;
      } else {
        roots.push(node);
      }
    });

    return roots;
  }
}