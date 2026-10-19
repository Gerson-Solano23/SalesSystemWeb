import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UtilityService } from './utility.service';

export type RoleName = 'Admin' | 'Supervisor' | 'Employee';

/** First page of each role after login or when it opens a page it cannot use. */
export const HOME_BY_ROLE: Record<RoleName, string> = {
  Admin: '/pages/dashboard',
  Supervisor: '/pages/products',
  Employee: '/pages/sale',
};

/** Requires a session with a non-expired token; otherwise goes to the login page. */
export const authGuard: CanActivateFn = () => {
  const utilityService = inject(UtilityService);
  const router = inject(Router);

  if (utilityService.getUserSession() == null || utilityService.checkTokenExpiration()) {
    utilityService.deleteUserSession();
    return router.createUrlTree(['/login']);
  }
  return true;
};

/**
 * Allows the route only for the roles in route.data.roles; other users are sent to their home page.
 * This is for navigation only: the API enforces the same rules on every request.
 */
export const roleGuard: CanActivateFn = (route) => {
  const utilityService = inject(UtilityService);
  const router = inject(Router);
  const allowed = (route.data?.['roles'] ?? []) as RoleName[];
  const role = utilityService.getUserSession()?.rolDescription as RoleName | undefined;

  if (role && allowed.includes(role)) {
    return true;
  }
  return router.createUrlTree([role ? HOME_BY_ROLE[role] ?? '/login' : '/login']);
};
