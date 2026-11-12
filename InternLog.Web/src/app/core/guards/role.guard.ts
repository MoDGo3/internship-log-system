import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from '../auth/auth.service';

/** จำกัด role ด้วย route data: { data: { roles: ['admin'] } } */
export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const expectedRoles = (route.data?.['roles'] as string[] | undefined) ?? [];

  // guard นี้อาจรันพร้อม authGuard ตอนเปิดหน้าตรง ๆ จึงต้องรอ session เหมือนกัน
  return authService.ensureSession().pipe(
    map(ok => {
      if (!ok) {
        return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
      }
      if (expectedRoles.length > 0 && !authService.hasRole(expectedRoles)) {
        alert('คุณไม่มีสิทธิ์เข้าถึงหน้านี้');
        return router.createUrlTree(['/dashboard']);
      }
      return true;
    })
  );
};
