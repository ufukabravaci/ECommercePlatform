import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { CartService } from '../../core/services/cart-service';
import { OrderService } from '../../core/services/order-service';
import { CreateOrderRequest } from '../../core/models/checkout';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './checkout.html', // Dosya ismin checkout.component.html ise burayı düzelt
  styleUrls: ['./checkout.scss'] // Dosya ismin checkout.component.scss ise burayı düzelt
})
export class CheckoutComponent implements OnInit {
  private fb = inject(FormBuilder);
  private orderService = inject(OrderService);
  private router = inject(Router);
  public cartService = inject(CartService);

  loading = this.orderService.loading;
  error = this.orderService.error;

  checkoutForm = this.fb.group({
    // Adres
    city: ['', [Validators.required, Validators.minLength(2)]],
    district: ['', [Validators.required, Validators.minLength(2)]],
    street: ['', [Validators.required]],
    zipCode: ['', [Validators.required, Validators.pattern(/^\d{5}$/)]],
    fullAddress: ['', [Validators.required, Validators.minLength(10)]],
    // Kart
    cardHolderName: ['', [Validators.required, Validators.minLength(3)]],
    // 16 hane + 3 boşluk = 19 karakter
    cardNumber: ['', [Validators.required, Validators.pattern(/^\d{4} \d{4} \d{4} \d{4}$/)]], 
    // MM/YY formatı (slash dahil)
    expiryDate: ['', [Validators.required, Validators.pattern(/^(0[1-9]|1[0-2])\/([0-9]{2})$/)]], 
    cvv: ['', [Validators.required, Validators.pattern(/^\d{3,4}$/)]]
  });

  // Component açıldığında hatayı temizle
  ngOnInit(): void {
    this.orderService.clearError();
  }

  get totalAmount() {
    return this.cartService.totalAmount();
  }

  // --- FORMATLAMA METOTLARI ---

  // Kart Numarası: 0000 0000 0000 0000
  formatCardNumber(event: any) {
    let input = event.target.value.replace(/\D/g, ''); // Sadece rakamları al
    if (input.length > 16) input = input.substring(0, 16); // Max 16 hane

    // Her 4 hanede bir boşluk ekle
    const formatted = input.match(/.{1,4}/g)?.join(' ') || '';
    
    // Form control değerini güncelle
    this.checkoutForm.patchValue({ cardNumber: formatted }, { emitEvent: false });
  }

  // Son Kullanma Tarihi: MM/YY
  formatExpiryDate(event: any) {
    let input = event.target.value.replace(/\D/g, ''); // Sadece rakamları al
    if (input.length > 4) input = input.substring(0, 4); // Max 4 hane (AAYY)

    if (input.length >= 2) {
      // 2. karakterden sonra / ekle
      input = input.substring(0, 2) + '/' + input.substring(2);
    }

    this.checkoutForm.patchValue({ expiryDate: input }, { emitEvent: false });
  }
  
  // Sadece rakam girilmesine izin ver (CVV ve ZipCode için)
  allowOnlyNumbers(event: any) {
    const input = event.target.value.replace(/\D/g, '');
    const controlName = event.target.getAttribute('formControlName');
    if(controlName) {
         this.checkoutForm.get(controlName)?.setValue(input, { emitEvent: false });
    }
  }

  onSubmit() {
    if (this.checkoutForm.invalid) {
      this.checkoutForm.markAllAsTouched();
      return;
    }

    const cartItems = this.cartService.basket()?.items ?? [];
    if (cartItems.length === 0) {
      alert("Sepetiniz boş! Lütfen ürün ekleyiniz.");
      this.router.navigate(['/products']);
      return;
    }

    const formVal = this.checkoutForm.getRawValue();
    const request: CreateOrderRequest = {
      items: cartItems.map(i => ({
        productId: i.productId,
        quantity: i.quantity
      })),
      city: formVal.city!,
      district: formVal.district!,
      street: formVal.street!,
      zipCode: formVal.zipCode!,
      fullAddress: formVal.fullAddress!
    };

    this.orderService.createOrder(request).subscribe({
      next: (res) => {
        if (res.isSuccessful) {
          this.router.navigate(['/account/orders']);
        }
      }
    });
  }
}