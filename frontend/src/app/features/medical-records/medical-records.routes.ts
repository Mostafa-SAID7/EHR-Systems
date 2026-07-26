import { Routes } from '@angular/router';

/**
 * Medical Records Feature Routes
 */
export const medicalRecordsRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/medical-records-page/medical-records-page.component').then(
        (m) => m.MedicalRecordsPageComponent
      ),
    data: { title: 'Medical Records', breadcrumb: 'Medical Records' },
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./pages/record-detail-page/record-detail-page.component').then(
        (m) => m.RecordDetailPageComponent
      ),
    data: { title: 'Record Details', breadcrumb: 'Details' },
  },
];
