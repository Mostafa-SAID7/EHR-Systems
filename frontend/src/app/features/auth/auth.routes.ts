import { Routes } from '@angular/router';

/**
 * Authentication Feature Routes
 */
export const authRoutes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login-page/login-page.component').then(
        (m) => m.LoginPageComponent
      ),
    data: { title: 'Sign In' },
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./pages/register-page/register-page.component').then(
        (m) => m.RegisterPageComponent
      ),
    data: { title: 'Register Account' },
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./pages/forgot-password-page/forgot-password-page.component').then(
        (m) => m.ForgotPasswordPageComponent
      ),
    data: { title: 'Forgot Password' },
  },
  {
    path: 'reset-password/:token',
    loadComponent: () =>
      import('./pages/reset-password-page/reset-password-page.component').then(
        (m) => m.ResetPasswordPageComponent
      ),
    data: { title: 'Reset Password' },
  },
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
];
