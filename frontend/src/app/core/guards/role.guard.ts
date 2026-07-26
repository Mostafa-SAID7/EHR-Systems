import { inject } from '@angular/core';
import { Router, CanActivateFn, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

/**
 * Role Guard — protects routes based on user roles
 */
export const roleGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  _state: RouterStateSnapshot,
): boolean => {
  const authService           = inject(AuthService);
  const router                = inject(Router);
  const notificationService   = inject(NotificationService);

  const requiredRoles = route.data['roles'] as string[] | undefined;

  if (!requiredRoles?.length) return true;

  const user = authService.getCurrentUser();
  if (!user) { router.navigate(['/auth/login']); return false; }

  const hasRole = requiredRoles.some(role =>
    user.roles?.some(r => r.name.toLowerCase() === role.toLowerCase())
  );

  if (hasRole) return true;

  notificationService.error('Access Denied', 'You do not have permission to access this resource.');
  router.navigate(['/dashboard']);
  return false;
};
