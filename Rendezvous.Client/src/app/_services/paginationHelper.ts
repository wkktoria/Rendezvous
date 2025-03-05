import { HttpParams, HttpResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { PaginatedResult } from '../_models/pagination';

export function setPaginationHeaders(pageNumber: number, pageSize: number) {
  let httpParams = new HttpParams();

  if (pageNumber && pageSize) {
    httpParams = httpParams.append('pageNumber', pageNumber);
    httpParams = httpParams.append('pageSize', pageSize);
  }

  return httpParams;
}

export function setPaginatedResponse<T>(
  response: HttpResponse<T>,
  paginatedResultSignal: ReturnType<typeof signal<PaginatedResult<T> | null>>
) {
  paginatedResultSignal.set({
    items: response.body as T,
    pagination: JSON.parse(response.headers.get('Pagination')!),
  });
}
