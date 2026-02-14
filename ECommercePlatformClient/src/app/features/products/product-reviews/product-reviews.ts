import { CommonModule } from "@angular/common";
import { Component, OnInit, input, inject, signal } from "@angular/core";
import { ReactiveFormsModule, FormBuilder, Validators } from "@angular/forms";
import { PaginationParams } from "../../../core/models";
import { AuthService } from "../../../core/services";
import { ReviewService } from "../../../core/services/review-service";
import { PaginationComponent } from "../../../shared/components";

@Component({
  selector: 'app-product-reviews',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PaginationComponent],
  templateUrl: './product-reviews.html',
  styleUrls: ['./product-reviews.scss']
})
export class ProductReviewsComponent implements OnInit {
  productId = input.required<string>(); // Product ID input
  private reviewService = inject(ReviewService);
  private authService = inject(AuthService);
  private fb = inject(FormBuilder);
  readonly currentUserId = this.authService.currentUserId;
  readonly isDeleting = signal<string | null>(null);

  reviews = this.reviewService.reviews;
  loading = this.reviewService.loading;
  pagination = this.reviewService.pagination;
  isLoggedIn = this.authService.isAuthenticated;

  showForm = signal(false);
  isSubmitting = signal(false);

  // Yorum Formu
  reviewForm = this.fb.group({
    rating: [5, [Validators.required, Validators.min(1), Validators.max(5)]],
    comment: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(1000)]]
  });

  ngOnInit() {
    this.loadReviews(1);
  }

  loadReviews(page: number) {
    const params: PaginationParams = {
      pageNumber: page,
      pageSize: 5, // Sayfa başı 5 yorum
      sortBy: 'CreatedAt',
      sortDirection: 'desc'
    };
    this.reviewService.loadProductReviews(this.productId(), params).subscribe();
  }

  toggleForm() {
    this.showForm.update(v => !v);
  }

  submitReview() {
    if (this.reviewForm.invalid) return;

    this.isSubmitting.set(true);
    const formValue = this.reviewForm.value;

    this.reviewService.createReview({
      productId: this.productId(),
      rating: formValue.rating!,
      comment: formValue.comment!
    }).subscribe({
      next: (res) => {
        if (res.isSuccessful) {
          this.reviewForm.reset({ rating: 5, comment: '' });
          this.showForm.set(false);
          this.loadReviews(1); // Listeyi yenile
          // Toast message: Yorumunuz alındı (Onay bekliyor olabilir)
        }
      },
      complete: () => this.isSubmitting.set(false)
    });
  }

  deleteReview(reviewId: string) {
    if (!confirm('Bu yorumu silmek istediğinize emin misiniz?')) return;

    this.isDeleting.set(reviewId);

    this.reviewService.deleteReview(reviewId).subscribe({
      next: (res) => {
        if (res.isSuccessful) {
          // Başarılı ise listeyi yenile
          // (Veya optimistik olarak listeden silebiliriz ama pagination bozulabilir, reload en temizi)
          this.loadReviews(this.pagination().pageNumber);
          
          // Toast mesajı eklenebilir
        } else {
          alert(res.errorMessages?.[0] || 'Silme işlemi başarısız.');
        }
      },
      error: () => alert('Bir hata oluştu.'),
      complete: () => this.isDeleting.set(null)
    });
  }
  
  // Yıldız Helper
  getStars(rating: number): number[] {
    return Array(5).fill(0).map((_, i) => i + 1); // [1,2,3,4,5]
  }
}