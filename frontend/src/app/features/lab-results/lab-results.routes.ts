import { Routes } from '@angular/router';

/**
 * Lab Results Feature Routes
 */
export const labResultsRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/lab-results-page/lab-results-page.component').then(
        (m) => m.LabResultsPageComponent
      ),
    data: { title: 'Lab Results', breadcrumb: 'Lab Results' },
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./pages/lab-result-detail-page/lab-result-detail-page.component').then(
        (m) => m.LabResultDetailPageComponent
      ),
    data: { title: 'Lab Result Details', breadcrumb: 'Details' },
  },
];
