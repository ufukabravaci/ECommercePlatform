export interface BasketItem {
  productId: string;
  productName: string;
  priceAmount: number;
  priceCurrency: string;
  quantity: number;
  imageUrl?: string;
}

export interface CustomerBasket {
  customerId: string;
  items: BasketItem[];
  totalAmount: number; // Backend'den hesaplanmış gelir
}

// Add to cart için input
export interface AddToCartInput {
  productId: string;
  productName: string;
  priceAmount: number;
  priceCurrency: string;
  quantity: number;
  imageUrl?: string;
}