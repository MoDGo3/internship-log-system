import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { catchError, switchMap, throwError, Observable, share, finalize, tap } from 'rxjs';
import { AuthResponse, ApiResponse } from '../../models/user.model';

// ป้องกัน Multiple Refresh: เก็บ Observable ตัวเดียวไว้ใช้ร่วมกัน
let refreshInProgress$: Observable<ApiResponse<AuthResponse>> | null = null;

/** endpoint ที่ไม่ต้องใช้ access token — ห้าม refresh/retry (กัน loop และกัน logout ตอนกรอกรหัสผิด) */
const ANONYMOUS_AUTH_PATHS = ['/auth/login', '/auth/register', '/auth/refresh', '/auth/logout',
                              '/auth/forgot-password', '/auth/reset-password'];

function isAnonymousAuthRequest(url: string): boolean {
  return ANONYMOUS_AUTH_PATHS.some(path => url.includes(path));
}

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
  if (isAnonymousAuthRequest(req.url)) {
    return next(req);
  }

  const authService = inject(AuthService);
  const token = authService.getAccessToken();

  // Token หมดอายุแล้ว — ไม่ต้องส่งไป backend ให้ refresh ก่อนเลย
  if (token && isJwtExpired(token)) {
    return refreshAndRetry(req, next, authService);
  }

  const authReq = token ? withBearer(req, token) : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) =>
      error.status === 401 ? refreshAndRetry(req, next, authService) : throwError(() => error)
    )
  );
};

function withBearer(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}

/** Refresh token (ผ่าน cookie) แล้ว retry request เดิม — ทุก request ที่รออยู่ใช้ผล refresh ครั้งเดียวกัน */
function refreshAndRetry(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService
): Observable<any> {
  if (!refreshInProgress$) {
    refreshInProgress$ = authService.refreshTokens().pipe(
      // refresh ไม่สำเร็จ = session หมดอายุจริง → ออกจากระบบ (ทำครั้งเดียวแม้มีหลาย request รออยู่)
      tap({ error: () => authService.logout() }),
      finalize(() => (refreshInProgress$ = null)),
      share()
    );
  }

  // error จาก request ที่ retry แล้ว (เช่น 403/404) ส่งต่อให้ผู้เรียกตามปกติ ไม่ logout
  return refreshInProgress$.pipe(
    switchMap(res => next(withBearer(req, res.data.accessToken)))
  );
}
