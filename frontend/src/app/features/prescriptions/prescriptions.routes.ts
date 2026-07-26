import { Routes } from '@angular/router';

/**
 * Prescriptions Feature Routes
 */
export const prescriptionsRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/prescription-list-page/prescription-list-page.component').then(
        (m) => m.PrescriptionListPageComponent
      ),
    data: { title: 'Prescriptions', breadcrumb: 'Prescriptions' },
  },
  {
    path: 'new',
    loadComponent: () =>
      import('./pages/prescription-create-page/prescription-create-page.component').then(
        (m) => m.PrescriptionCreatePageComponent
      ),
    data: { title: 'New Prescription', breadcrumb: 'New' },
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./pages/prescription-detail-page/prescription-detail-page.component').then(
        (m) => m.PrescriptionDetailPageComponent
      ),
    data: { title: 'Prescription Details', breadcrumb: 'Details' },
  },
];
