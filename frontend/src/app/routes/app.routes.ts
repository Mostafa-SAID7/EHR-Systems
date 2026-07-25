import { Routes } from '@angular/router';
import { MainLayoutComponent } from '../layouts/main-layout/main-layout.component';
import { authGuard } from '../core/guards/auth.guard';
import { roleGuard } from '../core/guards/role.guard';

export const appRoutes: Routes = [
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
  
  // Auth Routes (Public) — lazy-loaded so the brand panel & auth components
  // are excluded from the initial main.js bundle
  {
    path: 'auth',
    loadComponent: () =>
      import('../layouts/auth-layout/auth-layout.component').then(
        (m) => m.AuthLayoutComponent
      ),
    children: [
      {
        path: 'login',
        loadComponent: () =>
          import('../features/auth/pages/login-page/login-page.component').then(
            (m) => m.LoginPageComponent
          ),
      },
      {
        path: 'register',
        loadComponent: () =>
          import('../features/auth/pages/register-page/register-page.component').then(
            (m) => m.RegisterPageComponent
          ),
      },
      {
        path: 'forgot-password',
        loadComponent: () =>
          import('../features/auth/pages/forgot-password-page/forgot-password-page.component').then(
            (m) => m.ForgotPasswordPageComponent
          ),
      },
      {
        path: 'reset-password/:token',
        loadComponent: () =>
          import('../features/auth/pages/reset-password-page/reset-password-page.component').then(
            (m) => m.ResetPasswordPageComponent
          ),
      },
    ],
  },
  
  // App Routes (Protected)
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
      
      // Patients
      {
        path: 'patients',
        canActivate: [roleGuard],
        data: { roles: ['doctor', 'nurse', 'admin'] },
        children: [
          {
            path: '',
            loadComponent: () =>
              import('../features/patients/pages/patient-list-page/patient-list-page.component').then(
                (m) => m.PatientListPageComponent
              ),
            data: { title: 'Patients' },
          },
          {
            path: 'search',
            loadComponent: () =>
              import('../features/patients/pages/patient-search-page/patient-search-page.component').then(
                (m) => m.PatientSearchPageComponent
              ),
            data: { title: 'Patient Search' },
          },
          {
            path: ':id',
            loadComponent: () =>
              import('../features/patients/pages/patient-detail-page/patient-detail-page.component').then(
                (m) => m.PatientDetailPageComponent
              ),
            data: { title: 'Patient Details' },
          },
          {
            path: ':id/timeline',
            loadComponent: () =>
              import('../features/patients/pages/patient-timeline-page/patient-timeline-page.component').then(
                (m) => m.PatientTimelinePageComponent
              ),
            data: { title: 'Patient Timeline' },
          },
        ],
      },
      
      // Appointments
      {
        path: 'appointments',
        canActivate: [roleGuard],
        data: { roles: ['doctor', 'nurse', 'admin', 'receptionist'] },
        children: [
          {
            path: '',
            loadComponent: () =>
              import('../features/appointments/pages/appointment-list-page/appointment-list-page.component').then(
                (m) => m.AppointmentListPageComponent
              ),
            data: { title: 'Appointments' },
          },
          {
            path: 'schedule',
            loadComponent: () =>
              import('../features/appointments/pages/appointment-schedule-page/appointment-schedule-page.component').then(
                (m) => m.AppointmentSchedulePageComponent
              ),
            data: { title: 'Schedule Appointment' },
          },
          {
            path: ':id',
            loadComponent: () =>
              import('../features/appointments/pages/appointment-detail-page/appointment-detail-page.component').then(
                (m) => m.AppointmentDetailPageComponent
              ),
            data: { title: 'Appointment Details' },
          },
        ],
      },
      
      // Medical Records
      {
        path: 'medical-records',
        canActivate: [roleGuard],
        data: { roles: ['doctor', 'nurse', 'admin'] },
        children: [
          {
            path: '',
            loadComponent: () =>
              import('../features/medical-records/pages/medical-records-page/medical-records-page.component').then(
                (m) => m.MedicalRecordsPageComponent
              ),
            data: { title: 'Medical Records' },
          },
          {
            path: ':id',
            loadComponent: () =>
              import('../features/medical-records/pages/record-detail-page/record-detail-page.component').then(
                (m) => m.RecordDetailPageComponent
              ),
            data: { title: 'Record Details' },
          },
        ],
      },
      
      // Prescriptions
      {
        path: 'prescriptions',
        canActivate: [roleGuard],
        data: { roles: ['doctor', 'pharmacist', 'admin'] },
        children: [
          {
            path: '',
            loadComponent: () =>
              import('../features/prescriptions/pages/prescription-list-page/prescription-list-page.component').then(
                (m) => m.PrescriptionListPageComponent
              ),
            data: { title: 'Prescriptions' },
          },
          {
            path: 'new',
            loadComponent: () =>
              import('../features/prescriptions/pages/prescription-create-page/prescription-create-page.component').then(
                (m) => m.PrescriptionCreatePageComponent
              ),
            data: { title: 'New Prescription' },
          },
          {
            path: ':id',
            loadComponent: () =>
              import('../features/prescriptions/pages/prescription-detail-page/prescription-detail-page.component').then(
                (m) => m.PrescriptionDetailPageComponent
              ),
            data: { title: 'Prescription Details' },
          },
        ],
      },
      
      // Lab Results
      {
        path: 'lab-results',
        canActivate: [roleGuard],
        data: { roles: ['doctor', 'nurse', 'lab-tech', 'admin'] },
        children: [
          {
            path: '',
            loadComponent: () =>
              import('../features/lab-results/pages/lab-results-page/lab-results-page.component').then(
                (m) => m.LabResultsPageComponent
              ),
            data: { title: 'Lab Results' },
          },
          {
            path: ':id',
            loadComponent: () =>
              import('../features/lab-results/pages/lab-result-detail-page/lab-result-detail-page.component').then(
                (m) => m.LabResultDetailPageComponent
              ),
            data: { title: 'Lab Result Details' },
          },
        ],
      },
      
      // Billing
      {
        path: 'billing',
        canActivate: [roleGuard],
        data: { roles: ['admin', 'billing-officer'] },
        children: [
          {
            path: '',
            loadComponent: () =>
              import('../features/billing/pages/billing-page/billing-page.component').then(
                (m) => m.BillingPageComponent
              ),
            data: { title: 'Billing & Claims' },
          },
          {
            path: 'invoices',
            loadComponent: () =>
              import('../features/billing/pages/invoice-list-page/invoice-list-page.component').then(
                (m) => m.InvoiceListPageComponent
              ),
            data: { title: 'Invoices' },
          },
        ],
      },
      
      // Reports & Analytics
      {
        path: 'reports',
        canActivate: [roleGuard],
        data: { roles: ['admin', 'doctor', 'manager'] },
        children: [
          {
            path: '',
            loadComponent: () =>
              import('../features/reports-analytics/pages/reports-page/reports-page.component').then(
                (m) => m.ReportsPageComponent
              ),
            data: { title: 'Reports & Analytics' },
          },
          {
            path: 'population-health',
            loadComponent: () =>
              import('../features/reports-analytics/pages/population-health-page/population-health-page.component').then(
                (m) => m.PopulationHealthPageComponent
              ),
            data: { title: 'Population Health' },
          },
          {
            path: 'compliance',
            loadComponent: () =>
              import('../features/reports-analytics/pages/compliance-page/compliance-page.component').then(
                (m) => m.CompliancePageComponent
              ),
            data: { title: 'Compliance Reports' },
          },
        ],
      },
      
      // Admin
      {
        path: 'admin',
        canActivate: [roleGuard],
        data: { roles: ['admin'] },
        children: [
          {
            path: '',
            loadComponent: () =>
              import('../features/admin/pages/admin-dashboard-page/admin-dashboard-page.component').then(
                (m) => m.AdminDashboardPageComponent
              ),
            data: { title: 'Administration' },
          },
          {
            path: 'users',
            loadComponent: () =>
              import('../features/admin/pages/user-management-page/user-management-page.component').then(
                (m) => m.UserManagementPageComponent
              ),
            data: { title: 'User Management' },
          },
          {
            path: 'roles',
            loadComponent: () =>
              import('../features/admin/pages/role-management-page/role-management-page.component').then(
                (m) => m.RoleManagementPageComponent
              ),
            data: { title: 'Role Management' },
          },
          {
            path: 'settings',
            loadComponent: () =>
              import('../features/admin/pages/settings-page/settings-page.component').then(
                (m) => m.SettingsPageComponent
              ),
            data: { title: 'System Settings' },
          },
          {
            path: 'audit-logs',
            loadComponent: () =>
              import('../features/admin/pages/audit-logs-page/audit-logs-page.component').then(
                (m) => m.AuditLogsPageComponent
              ),
            data: { title: 'Audit Logs' },
          },
        ],
      },
    ],
  },
  
  // Wildcard route for 404
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
