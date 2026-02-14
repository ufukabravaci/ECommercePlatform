import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core'; // signal eklendi
import { RouterLink } from '@angular/router';
import { OrderService } from '../../../core/services/order-service';
import { CommonModule } from '@angular/common';
import { PaginationParams } from '../../../core/models';
import { PaginationComponent } from '../../../shared/components/pagination/pagination'; // Import Eklendi

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, RouterLink, PaginationComponent], // Component Eklendi
  templateUrl: './orders.html',
  styleUrls: ['./orders.scss']
})
export class OrdersComponent implements OnInit {
  private orderService = inject(OrderService);

  // Signals
  orders = this.orderService.myOrders;
  loading = this.orderService.loading;
  error = this.orderService.error;
  hasOrders = this.orderService.hasOrders;
  pagination = this.orderService.pagination; // Pagination signal eklendi

  // Local State
  currentPage = signal(1);

  ngOnInit() {
    this.loadOrders(1);
  }

  loadOrders(page: number) {
    this.currentPage.set(page);
    
    const params: PaginationParams = {
      pageNumber: page,
      pageSize: 10,
      sortBy: 'OrderDate',
      sortDirection: 'desc'
    };
    
    this.orderService.loadMyOrders(params).subscribe();
  }
  
  // Pagination Event
  onPageChange(page: number): void {
    this.loadOrders(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  // ... (getStatusClass ve getStatusLabel aynı kalacak) ...
  getStatusClass(status: string): string { /* ... */ return ''; }
  getStatusLabel(status: string): string { /* ... */ return ''; }
}
