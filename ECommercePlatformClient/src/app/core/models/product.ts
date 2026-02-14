// --- ENUMS & TYPES ---
export type CurrencyCode = 'TRY' | 'USD' | 'EUR';

// --- DTOs (Backend Karşılıkları) ---

export interface ProductImage {
  id: string;
  imageUrl: string;
  isMain: boolean;
}

// Backend: ProductDto
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
  mainImageUrl: string | null;
  brandId: string;      // EKLENDİ
  brandName: string;    // EKLENDİ
  images: ProductImage[];
  
  // Frontend Helpers (Opsiyonel, map ederken eklenebilir)
  isNew?: boolean;       // Backend dönmüyor, frontend'de hesaplanacaksa optional
  oldPrice?: number;     // Backend dönmüyor, indirim varsa buraya eklenebilir
}