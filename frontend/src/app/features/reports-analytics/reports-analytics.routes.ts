import { Routes } from '@angular/router';

/**
 * Reports & Analytics Feature Routes
 */
export const reportsAnalyticsRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/reports-page/reports-page.component').then(
        (m) => m.ReportsPageComponent
      ),
    data: { title: 'Reports & Analytics', breadcrumb: 'Reports' },
  },
  {
    path: 'population-health',
    loadComponent: () =>
      import('./pages/population-health-page/population-health-page.component').then(
        (m) => m.PopulationHealthPageComponent
      ),
    data: { title: 'Population Health', breadcrumb: 'Population Health' },
  },
  {
    path: 'compliance',
    loadComponent: () =>
      import('./pages/compliance-page/compliance-page.component').then(
        (m) => m.CompliancePageComponent
      ),
    data: { title: 'Compliance Reports', breadcrumb: 'Compliance' },
  },
];
