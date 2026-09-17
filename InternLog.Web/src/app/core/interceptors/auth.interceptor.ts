import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { catchError, switchMap, throwError, Observable, share } from 'rxjs';
import { AuthResponse, ApiResponse } from '../../models/user.model';

// ป้องกัน Multiple Refresh: เก็บ Observable ตัวเดียวไว้ใช้ร่วมกัน
let refreshInProgress$: Observable<ApiResponse<AuthResponse>> | null = null;

/** ตรวจสอบ JWT expiry โดยไม่ต้องใช้ library */
function isJwtExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return Date.now() >= payload.exp * 1000;
  } catch {
    return true; // Parse ไม่ได้ ถือว่าหมดอายุ
  }
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getAccessToken();

  // --- ถ้า Token หมดอายุแล้ว อย่าส่งไปให้ Backend เลย ให้ไป Refresh ทันที ---
  if (token && isJwtExpired(token) && !req.url.includes('/refresh') && !req.url.includes('/login')) {
    return doRefreshAndRetry(req, next, authService);
  }

  // Clone request to add Authorization header if token exists
  let authReq = req;
  if (token) {
    authReq = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 Unauthorized errors ที่ Backend ส่งกลับมา
      if (error.status === 401) {
        // ป้องกัน infinite loop: ถ้า /refresh เองได้ 401 ให้ logout ทันที
        if (req.url.includes('/refresh')) {
          authService.logout();
          return throwError(() => error);
        }
        // ลอง Refresh Token
        return doRefreshAndRetry(req, next, authService);
      }
      return throwError(() => error);
    })
  );
};

/** Refresh token แล้ว retry request เดิม — ป้องกัน multiple refresh ด้วย shared observable */
function doRefreshAndRetry(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService
): Observable<any> {
  // ถ้ายังไม่มีการ Refresh อยู่ ให้สร้างใหม่ แล้ว share ไว้
  if (!refreshInProgress$) {
    refreshInProgress$ = authService.refreshTokens().pipe(
      share() // Request เดียว แต่ subscriber ได้ผลพร้อมกันทุกคน
    );
  }

  return refreshInProgress$.pipe(
    switchMap((res) => {
      refreshInProgress$ = null; // รีเซ็ต flag หลัง refresh สำเร็จ
      const newAuthReq = req.clone({
        setHeaders: { Authorization: `Bearer ${res.data.accessToken}` }
      });
      return next(newAuthReq);
    }),
    catchError((refreshErr) => {
      refreshInProgress$ = null; // รีเซ็ต flag หลัง refresh ล้มเหลว
      authService.logout();
      return throwError(() => refreshErr);
    })
  );
}

