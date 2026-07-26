import { Routes } from '@angular/router';

/**
 * Appointments Feature Routes
 */
export const appointmentsRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/appointment-list-page/appointment-list-page.component').then(
        (m) => m.AppointmentListPageComponent
      ),
    data: { title: 'Appointments', breadcrumb: 'Appointments' },
  },
  {
    path: 'schedule',
    loadComponent: () =>
      import('./pages/appointment-schedule-page/appointment-schedule-page.component').then(
        (m) => m.AppointmentSchedulePageComponent
      ),
    data: { title: 'Schedule Appointment', breadcrumb: 'Schedule' },
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./pages/appointment-detail-page/appointment-detail-page.component').then(
        (m) => m.AppointmentDetailPageComponent
      ),
    data: { title: 'Appointment Details', breadcrumb: 'Details' },
  },
];
