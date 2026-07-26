import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

/** Public auth endpoints that must NOT receive a Bearer token. */
const PUBLIC_AUTH_ROUTES = [
  '/auth/login',
  '/auth/register',
  '/auth/forgot-password',
  '/auth/reset-password',
  '/auth/external-login',
  '/auth/refresh',
];

/**
 * Auth Interceptor — adds Bearer token to every outgoing request
 * except public authentication endpoints.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthService).getToken();
  const isPublic = PUBLIC_AUTH_ROUTES.some(route => req.url.includes(route));

  if (token && !isPublic) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }

  return next(req);
};
