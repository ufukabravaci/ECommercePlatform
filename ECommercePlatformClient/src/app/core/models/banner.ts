export interface Banner {
  id: string;
  title: string;
  description: string;
  imageUrl: string;
  targetUrl: string;
  order: number;
  isActive: boolean; // Backend'de varsa
}