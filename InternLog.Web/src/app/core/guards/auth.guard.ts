import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from '../auth/auth.service';

/**
 * ต้อง login ก่อนเข้าหน้า — ถ้า access token หมดอายุ (เช่น เปิดเว็บใหม่)
 * จะลองต่ออายุด้วย refresh token ใน httpOnly cookie ก่อนพาไปหน้า login
 */
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.ensureSession().pipe(
    map(ok => ok || router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } }))
  );
};
