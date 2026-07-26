import { Routes } from '@angular/router';

/**
 * Billing Feature Routes
 */
export const billingRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/billing-page/billing-page.component').then(
        (m) => m.BillingPageComponent
      ),
    data: { title: 'Billing & Claims', breadcrumb: 'Billing' },
  },
  {
    path: 'invoices',
    loadComponent: () =>
      import('./pages/invoice-list-page/invoice-list-page.component').then(
        (m) => m.InvoiceListPageComponent
      ),
    data: { title: 'Invoices', breadcrumb: 'Invoices' },
  },
];
