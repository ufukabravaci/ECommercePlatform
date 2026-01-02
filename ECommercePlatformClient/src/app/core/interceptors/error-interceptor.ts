import { HttpErrorResponse, HttpInterceptorFn } from "@angular/common/http";
import { inject } from "@angular/core";
import { Router } from "@angular/router";
import { catchError, throwError } from "rxjs";

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'Bir hata oluştu';

      if (error.error instanceof ErrorEvent) {
        errorMessage = error.error.message;
      } else {
        switch (error.status) {
          case 401:
            localStorage.removeItem('access_token');
            localStorage.removeItem('refresh_token');
            router.navigate(['/auth/login']);
            errorMessage = 'Oturum süreniz doldu';
            break;
          case 403:
            errorMessage = 'Bu işlem için yetkiniz yok';
            break;
          case 404:
            errorMessage = 'Kaynak bulunamadı';
            break;
          case 500:
            errorMessage = 'Sunucu hatası';
            break;
          default:
            if (error.error?.errorMessages?.length > 0) {
              errorMessage = error.error.errorMessages.join(', ');
            }
        }
      }

      console.error('HTTP Error:', error);
      return throwError(() => ({ message: errorMessage, status: error.status, original: error }));
    })
  );
};
