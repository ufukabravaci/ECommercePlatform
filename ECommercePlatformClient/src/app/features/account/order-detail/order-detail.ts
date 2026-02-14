import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrderService } from '../../../core/services/order-service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './order-detail.html',
  styleUrls: ['./order-detail.scss'],
})
export class OrderDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private orderService = inject(OrderService);

  order = this.orderService.currentOrder;
  loading = this.orderService.loading;
  error = this.orderService.error;

  ngOnInit(): void {
    // Component her yüklendiğinde temiz sayfa ile başla
    this.orderService.clearError();
    this.orderService.clearCurrentOrder();

    const orderNumber = this.route.snapshot.paramMap.get('orderNumber');
    if (orderNumber) {
      this.orderService.loadOrderDetail(orderNumber).subscribe();
    }
  }

  // Durum Renkleri (Orders listesindekiyle aynı mantık)
  getStatusClass(status: string): string {
    switch (status) {
      case 'Pending': return 'warning';
      case 'Confirmed': return 'info';
      case 'Shipped': return 'primary';
      case 'Delivered': return 'success';
      case 'Cancelled': return 'danger';
      case 'Refunded': return 'secondary';
      default: return 'light';
    }
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'Pending': return 'Sipariş Alındı';
      case 'Confirmed': return 'Onaylandı';
      case 'Shipped': return 'Kargoya Verildi';
      case 'Delivered': return 'Teslim Edildi';
      case 'Cancelled': return 'İptal Edildi';
      case 'Refunded': return 'İade Edildi';
      default: return status;
    }
  }
}