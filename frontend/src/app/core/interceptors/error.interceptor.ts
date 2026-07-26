import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { Router } from '@angular/router';
import { NotificationService } from '../services/notification.service';

/** Auth routes — 401s here must NOT trigger a redirect loop. */
const AUTH_ROUTES = ['/auth/login', '/auth/register', '/auth/forgot-password', '/auth/reset-password'];

/**
 * Error Interceptor — handles HTTP errors globally with user-friendly
 * notifications and context-aware 401 redirect logic.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router             = inject(Router);
  const notificationService = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const isAuthRoute = AUTH_ROUTES.some(r => req.url.includes(r));
      const message     = error.error?.message || error.error?.title || error.statusText || 'An unexpected error occurred';

      if (error.error instanceof ErrorEvent) {
        // Client-side / network error — show generic message
        notificationService.error('Network Error', error.error.message);
        return throwError(() => error);
      }

      switch (error.status) {
        case 401:
          if (!isAuthRoute) {
            // Session expired — redirect preserving the intended URL
            notificationService.error('Session Expired', 'Please sign in again.');
            router.navigate(['/auth/login'], {
              queryParams: { returnUrl: router.url },
            });
          }
          break;

        case 403:
          notificationService.error('Access Denied', 'You do not have permission to perform this action.');
          break;

        case 404:
          // Suppress 404 toasts for background resource checks
          if (!req.url.includes('/health')) {
            notificationService.error('Not Found', 'The requested resource was not found.');
          }
          break;

        case 408:
        case 503:
          notificationService.error('Service Unavailable', 'The server is temporarily unavailable. Please try again.');
          break;

        case 409:
          notificationService.error('Conflict', message);
          break;

        case 422:
          notificationService.error('Validation Error', message);
          break;

        case 500:
          notificationService.error('Server Error', 'An internal error occurred. Our team has been notified.');
          break;

        default:
          // Only show toast for non-auth-route errors to avoid redundant messages
          if (!isAuthRoute) {
            notificationService.error('Error', message);
          }
      }

      return throwError(() => error);
    })
  );
};
