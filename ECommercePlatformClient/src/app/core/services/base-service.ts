import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaginationParams, ApiResponse, PaginatedResult } from '../models/api-response';

@Injectable({
  providedIn: 'root',
})
export abstract class BaseService<T> {
  protected readonly apiUrl: string;

  constructor(protected http: HttpClient, protected endpoint: string) {
    this.apiUrl = `${environment.apiUrl}/${endpoint}`;
  }

  // Ortak Pagination Parametre Oluşturucu
  protected buildParams(params: PaginationParams): HttpParams {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString());

    if (params.search?.trim()) httpParams = httpParams.set('search', params.search.trim());
    return httpParams;
  }

  getAll(params: PaginationParams): Observable<ApiResponse<PaginatedResult<T>>> {
    return this.http.get<ApiResponse<PaginatedResult<T>>>(this.apiUrl, { params: this.buildParams(params) });
  }

  getById(id: string): Observable<ApiResponse<T>> {
    return this.http.get<ApiResponse<T>>(`${this.apiUrl}/${id}`);
  }
}