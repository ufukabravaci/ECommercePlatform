import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaginationParams, ApiResponse, PaginatedResult } from '../models/api-response';

export abstract class BaseService<T> {
  protected readonly http = inject(HttpClient);
  protected readonly apiUrl: string;

  constructor(protected endpoint: string) {
    this.apiUrl = `${environment.apiUrl}/${endpoint}`;
  }

  protected buildParams(params: PaginationParams): HttpParams {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString());

    if (params.search?.trim()) {
      httpParams = httpParams.set('search', params.search.trim());
    }

    if (params.sortBy) {
      httpParams = httpParams.set('sortBy', params.sortBy);
    }

    if (params.sortDirection) {
      httpParams = httpParams.set('sortDirection', params.sortDirection);
    }

    if (params.categoryId) {
      httpParams = httpParams.set('categoryId', params.categoryId);
    }

    return httpParams;
  }

  getAll(params: PaginationParams): Observable<ApiResponse<PaginatedResult<T>>> {
    return this.http.get<ApiResponse<PaginatedResult<T>>>(this.apiUrl, { 
      params: this.buildParams(params) 
    });
  }

  getById(id: string): Observable<ApiResponse<T>> {
    return this.http.get<ApiResponse<T>>(`${this.apiUrl}/${id}`);
  }
}