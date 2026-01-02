import type { CurrencyCode } from '../models';

export function formatCurrency(amount: number, currencyCode: CurrencyCode): string {
  const localeMap: Record<CurrencyCode, string> = {
    'TRY': 'tr-TR',
    'USD': 'en-US',
    'EUR': 'de-DE'
  };

  return new Intl.NumberFormat(localeMap[currencyCode] || 'tr-TR', {
    style: 'currency',
    currency: currencyCode
  }).format(amount);
}

export function formatDate(date: string | Date): string {
  return new Intl.DateTimeFormat('tr-TR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  }).format(new Date(date));
}

export function truncateText(text: string, maxLength: number): string {
  if (text.length <= maxLength) return text;
  return text.substring(0, maxLength) + '...';
}

export function generateSlug(text: string): string {
  return text
    .toLowerCase()
    .replace(/[çÇ]/g, 'c')
    .replace(/[ğĞ]/g, 'g')
    .replace(/[ıİ]/g, 'i')
    .replace(/[öÖ]/g, 'o')
    .replace(/[şŞ]/g, 's')
    .replace(/[üÜ]/g, 'u')
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '');
}