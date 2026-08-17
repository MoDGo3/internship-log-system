import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { catchError, switchMap, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getAccessToken();

  // Clone request to add Authorization header if token exists
  let authReq = req;
  if (token) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 Unauthorized errors (Token expired)
      if (error.status === 401 && token) {
        // Prevent infinite loop if refresh token call itself returns 401
        if (req.url.includes('/refresh')) {
          authService.logout();
          return throwError(() => error);
        }

        // Call refresh token API
        return authService.refreshTokens().pipe(
          switchMap((res) => {
            // If successful, retry original request with new token
            const newAuthReq = req.clone({
              setHeaders: {
                Authorization: `Bearer ${res.accessToken}`
              }
            });
            return next(newAuthReq);
          }),
          catchError((refreshErr) => {
            // Refresh token failed or expired
            authService.logout();
            return throwError(() => refreshErr);
          })
        );
      }
      return throwError(() => error);
    })
  );
};
