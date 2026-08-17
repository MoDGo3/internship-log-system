import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  // Roles expected are passed via route data: { data: { roles: ['admin'] } }
  const expectedRoles = route.data?.['roles'] as Array<string>;

  if (!authService.isAuthenticated()) {
    router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }

  if (expectedRoles && expectedRoles.length > 0) {
    const hasRole = authService.hasRole(expectedRoles);
    if (!hasRole) {
      // User doesn't have required role, redirect to forbidden/dashboard
      alert('คุณไม่มีสิทธิ์เข้าถึงหน้านี้');
      router.navigate(['/dashboard']);
      return false;
    }
  }

  return true;
};
