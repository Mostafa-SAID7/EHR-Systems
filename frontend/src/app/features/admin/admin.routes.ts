import { Routes } from '@angular/router';

/**
 * Admin Feature Routes
 */
export const adminRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/admin-dashboard-page/admin-dashboard-page.component').then(
        (m) => m.AdminDashboardPageComponent
      ),
    data: { title: 'Administration', breadcrumb: 'Admin' },
  },
  {
    path: 'users',
    loadComponent: () =>
      import('./pages/user-management-page/user-management-page.component').then(
        (m) => m.UserManagementPageComponent
      ),
    data: { title: 'User Management', breadcrumb: 'Users' },
  },
  {
    path: 'roles',
    loadComponent: () =>
      import('./pages/role-management-page/role-management-page.component').then(
        (m) => m.RoleManagementPageComponent
      ),
    data: { title: 'Role Management', breadcrumb: 'Roles' },
  },
  {
    path: 'settings',
    loadComponent: () =>
      import('./pages/settings-page/settings-page.component').then(
        (m) => m.SettingsPageComponent
      ),
    data: { title: 'System Settings', breadcrumb: 'Settings' },
  },
  {
    path: 'audit-logs',
    loadComponent: () =>
      import('./pages/audit-logs-page/audit-logs-page.component').then(
        (m) => m.AuditLogsPageComponent
      ),
    data: { title: 'Audit Logs', breadcrumb: 'Audit Logs' },
  },
];
