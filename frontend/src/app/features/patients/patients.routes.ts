import { Routes } from '@angular/router';

/**
 * Patients Feature Routes
 */
export const patientsRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/patient-list-page/patient-list-page.component').then(
        (m) => m.PatientListPageComponent
      ),
    data: { title: 'Patients', breadcrumb: 'Patients' },
  },
  {
    path: 'search',
    loadComponent: () =>
      import('./pages/patient-search-page/patient-search-page.component').then(
        (m) => m.PatientSearchPageComponent
      ),
    data: { title: 'Patient Search', breadcrumb: 'Search' },
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./pages/patient-detail-page/patient-detail-page.component').then(
        (m) => m.PatientDetailPageComponent
      ),
    data: { title: 'Patient Details', breadcrumb: 'Details' },
  },
  {
    path: ':id/timeline',
    loadComponent: () =>
      import('./pages/patient-timeline-page/patient-timeline-page.component').then(
        (m) => m.PatientTimelinePageComponent
      ),
    data: { title: 'Patient Timeline', breadcrumb: 'Timeline' },
  },
];
