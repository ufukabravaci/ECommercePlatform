export type OrderStatus = 'Pending' | 'Confirmed' | 'Shipped' | 'Delivered' | 'Cancelled' | 'Refunded';

// Order List (Değişiklik yok, burası zaten çalışıyordu sanırım)
export interface OrderListItem {
  id: string;
  orderNumber: string;
  orderDate: string;
  status: OrderStatus;
  totalAmount: number;
  itemCount: number;
  customerName?: string;
}

// Order Detail Item (Backend JSON'a göre güncellendi)
export interface OrderItemDto {
  productId: string;
  productName: string;
  
  // Backend: priceAmount
  priceAmount: number; 
  // Backend: priceCurrency
  priceCurrency: string;
  
  quantity: number;
  
  // Backend: total
  total: number;
  
  imageUrl?: string;
}

// Order Detail (Backend JSON'a göre güncellendi)
export interface OrderDetail {
  id: string; // id alanı da geliyormuş
  orderNumber: string;
  orderDate: string;
  status: OrderStatus;
  totalAmount: number;
  
  // Backend: shipping... ön ekiyle geliyor
  shippingCity: string;
  shippingDistrict: string;
  shippingStreet: string;
  shippingZipCode: string;
  shippingFullAddress: string;
  
  items: OrderItemDto[];
}


export interface CreateOrderRequest {
  items: { productId: string; quantity: number }[];
  city: string;
  district: string;
  street: string;
  zipCode: string;
  fullAddress: string;
}
