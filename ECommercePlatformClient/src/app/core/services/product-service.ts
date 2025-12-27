import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api-response';
import { ProductListItem, Product } from '../models/product';
import { BaseService } from './base-service';

@Injectable({
  providedIn: 'root',
})
export class ProductService extends BaseService<ProductListItem> {
  // Signals State (Loading, Error, Data) burada tanımlı olacak...
  
  constructor() { super(inject(HttpClient), 'products'); }

  // API: GET /api/products/{id} -> Detay döner (List item'dan farklı)
  getProductDetail(id: string): Observable<ApiResponse<Product>> {
    return this.http.get<ApiResponse<Product>>(`${this.apiUrl}/${id}`);
  }

  // API: POST /api/products/{id}/images (Multipart)
  uploadImage(productId: string, file: File, isMain: boolean): Observable<ApiResponse<string>> {
    const formData = new FormData();
    formData.append('file', file);       // API: [FromForm] IFormFile file
    formData.append('isMain', String(isMain)); // API: [FromForm] bool isMain
    
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/${productId}/images`, formData);
  }
}