import { Routes } from '@angular/router';
import { MainLayoutComponent } from '../layouts/main-layout/main-layout.component';
import { authGuard } from '../core/guards/auth.guard';
import { roleGuard } from '../core/guards/role.guard';

export const appRoutes: Routes = [
  // Root / Home
  {
    path: '',
    loadComponent: () =>
      import('../features/home/pages/home-page/home-page.component').then(
        (m) => m.HomePageComponent
      ),
    pathMatch: 'full',
  },
  {
    path: 'home',
    loadComponent: () =>
      import('../features/home/pages/home-page/home-page.component').then(
        (m) => m.HomePageComponent
      ),
  },

  // Auth Routes (Public - Lazy Loaded)
  {
    path: 'auth',
    loadComponent: () =>
      import('../layouts/auth-layout/auth-layout.component').then(
        (m) => m.AuthLayoutComponent
      ),
    loadChildren: () =>
      import('../features/auth/auth.routes').then(
        (m) => m.authRoutes
      ),
  },

  // App Routes (Protected - Main Layout)
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      // Dashboard
      {
        path: 'dashboard',
        loadComponent: () =>
          import('../features/dashboard/pages/dashboard-page/dashboard-page.component').then(
            (m) => m.DashboardPageComponent
          ),
        data: { title: 'Dashboard' },
      },

      // Patients Feature Routes
      {
        path: 'patients',
        canActivate: [roleGuard],
        data: { roles: ['doctor', 'nurse', 'admin'] },
        loadChildren: () =>
          import('../features/patients/patients.routes').then(
            (m) => m.patientsRoutes
          ),
      },

      // Appointments Feature Routes
      {
        path: 'appointments',
        canActivate: [roleGuard],
        data: { roles: ['doctor', 'nurse', 'admin', 'receptionist'] },
        loadChildren: () =>
          import('../features/appointments/appointments.routes').then(
            (m) => m.appointmentsRoutes
          ),
      },

      // Medical Records Feature Routes
      {
        path: 'medical-records',
        canActivate: [roleGuard],
        data: { roles: ['doctor', 'nurse', 'admin'] },
        loadChildren: () =>
          import('../features/medical-records/medical-records.routes').then(
            (m) => m.medicalRecordsRoutes
          ),
      },

      // Prescriptions Feature Routes
      {
        path: 'prescriptions',
        canActivate: [roleGuard],
        data: { roles: ['doctor', 'pharmacist', 'admin'] },
        loadChildren: () =>
          import('../features/prescriptions/prescriptions.routes').then(
            (m) => m.prescriptionsRoutes
          ),
      },

      // Lab Results Feature Routes
      {
        path: 'lab-results',
        canActivate: [roleGuard],
        data: { roles: ['doctor', 'nurse', 'lab-tech', 'admin'] },
        loadChildren: () =>
          import('../features/lab-results/lab-results.routes').then(
            (m) => m.labResultsRoutes
          ),
      },

      // Billing Feature Routes
      {
        path: 'billing',
        canActivate: [roleGuard],
        data: { roles: ['admin', 'billing-officer'] },
        loadChildren: () =>
          import('../features/billing/billing.routes').then(
            (m) => m.billingRoutes
          ),
      },

      // Reports & Analytics Feature Routes
      {
        path: 'reports',
        canActivate: [roleGuard],
        data: { roles: ['admin', 'doctor', 'manager'] },
        loadChildren: () =>
          import('../features/reports-analytics/reports-analytics.routes').then(
            (m) => m.reportsAnalyticsRoutes
          ),
      },

      // Admin Feature Routes
      {
        path: 'admin',
        canActivate: [roleGuard],
        data: { roles: ['admin'] },
        loadChildren: () =>
          import('../features/admin/admin.routes').then(
            (m) => m.adminRoutes
          ),
      },
    ],
  },

  // 404 Not Found Page
  {
    path: '404',
    loadComponent: () =>
      import('../features/not-found/pages/not-found-page/not-found-page.component').then(
        (m) => m.NotFoundPageComponent
      ),
    data: { title: '404 - Page Not Found' },
  },

  // Wildcard route for 404
  {
    path: '**',
    redirectTo: '404',
  },
];
