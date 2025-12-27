export interface Product {
  id: string;
  name: string;
  sku: string;
  description: string;
  priceAmount: number;
  currencyCode: CurrencyCode;
  stock: number;
  categoryId: string;
  categoryName?: string;
  images: ProductImage[];
  mainImageUrl: string | null; // UI kolaylığı için backend hesaplayıp döner
}

export interface ProductImage {
  id: string;
  imageUrl: string;
  isMain: boolean;
}

export type CurrencyCode = 'TRY' | 'USD' | 'EUR';

export interface ProductListItem {
  id: string;
  name: string;
  sku: string;
  priceAmount: number;
  currencyCode: CurrencyCode;
  stock: number;
  categoryName?: string;
  mainImageUrl: string | null;
  images: ProductImage[];
}

export interface CreateProductRequest {
  name: string;
  description: string;
  sku: string;
  priceAmount: number;
  currencyCode: CurrencyCode;
  stock: number;
  categoryId: string;
}