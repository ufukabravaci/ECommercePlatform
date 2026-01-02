import { HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from "@angular/common/http";
import { inject } from "@angular/core";
import { Router } from "@angular/router";
import { BehaviorSubject, catchError, throwError, switchMap, filter, take, Observable } from "rxjs";
import { AuthService } from "../services/auth-service";


let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> => {
  const authService = inject(AuthService);
  
  // 🔥 KRİTİK KONTROL: Eğer giden istek Refresh Token isteği ise, araya girme!
  // Aksi takdirde refresh isteği de 401 alırsa sonsuz döngüye girer.
  if (req.url.includes('/auth/refresh-token')) {
    return next(req);
  }

  const token = localStorage.getItem('access_token');
  let authReq = req;
  
  if (token) {
    authReq = addToken(req, token);
  }

  return next(authReq).pipe(
    catchError((error) => {
      // 401 hatası yakalandıysa ve istek refresh-token isteği değilse
      if (error instanceof HttpErrorResponse && error.status === 401) {
        return handle401Error(authReq, next, authService);
      }
      return throwError(() => error);
    })
  );
};

// --- Yardımcı Fonksiyonlar ---

function addToken(request: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return request.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });
}

function handle401Error(request: HttpRequest<unknown>, next: HttpHandlerFn, authService: AuthService): Observable<HttpEvent<unknown>> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    return authService.refreshToken().pipe(
      switchMap((response) => {
        isRefreshing = false;
        
        if (response.isSuccessful && response.data) {
          // Yeni token geldi, bekleyen diğer isteklere haber ver
          refreshTokenSubject.next(response.data.accessToken);
          // İlk başarısız olan isteği yeni token ile tekrarla
          return next(addToken(request, response.data.accessToken));
        }

        // Token yenileme başarısız (Refresh token da ölmüş)
        authService.logout();
        return throwError(() => new Error('Refresh token failed'));
      }),
      catchError((err) => {
        isRefreshing = false;
        authService.logout();
        return throwError(() => err);
      })
    );
  } else {
    // Zaten yenileme işlemi sürüyor, kuyruğa gir ve token bekle
    return refreshTokenSubject.pipe(
      filter(token => token != null),
      take(1),
      switchMap(token => {
        return next(addToken(request, token!));
      })
    );
  }
}