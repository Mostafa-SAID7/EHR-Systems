import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BillingStatsStripComponent, BillingStat } from '../../components/billing-stats-strip/billing-stats-strip.component';

interface Invoice {
  id: string;
  patient: string;
  initials: string;
  service: string;
  date: Date;
  amount: number;
  paid: number;
  status: 'Paid' | 'Pending' | 'Overdue' | 'Partial';
  color: string;
}

@Component({
  selector: 'app-billing-page',
  standalone: true,
  imports: [CommonModule, RouterModule, BillingStatsStripComponent, DecimalPipe],
  template: `
    <div class="space-y-6 stagger">

      <!-- ── Header ───────────────────────────────── -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="heading-xl">Billing</h1>
          <p class="body-text mt-1">Revenue and invoices overview</p>
        </div>
        <div class="flex items-center gap-2 shrink-0">
          <button class="btn-secondary btn-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/>
            </svg>
            Export
          </button>
          <button class="btn-primary btn-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
            </svg>
            New Invoice
          </button>
        </div>
      </div>

      <!-- ── Revenue stats subcomponent ──────────────── -->
      <app-billing-stats-strip
        [stats]="revenueStats"
      ></app-billing-stats-strip>

      <!-- ── Recent invoices + Summary ───────────── -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

        <!-- Invoices table -->
        <div class="lg:col-span-2 card p-0 overflow-hidden">
          <div class="card-header">
            <h2 class="heading-sm">Recent Invoices</h2>
            <button class="link-primary text-xs">View all &rarr;</button>
          </div>
          <div class="overflow-x-auto">
            <table class="table-base">
              <thead>
                <tr>
                  <th>Invoice</th>
                  <th>Patient</th>
                  <th>Service</th>
                  <th>Amount</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let inv of recentInvoices">
                  <td class="font-mono font-medium text-xs text-gray-500">#{{ inv.id }}</td>
                  <td>
                    <div class="flex items-center gap-2.5">
                      <div class="avatar-custom-sm" [style.background]="inv.color">{{ inv.initials }}</div>
                      <span class="font-medium text-gray-900 dark:text-white">{{ inv.patient }}</span>
                    </div>
                  </td>
                  <td class="text-xs text-gray-500 dark:text-gray-400">{{ inv.service }}</td>
                  <td class="font-semibold text-gray-900 dark:text-white tabular-nums">\${{ inv.amount }}</td>
                  <td>
                    <span [ngClass]="statusClass(inv.status)" class="badge">{{ inv.status }}</span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Billing summary / Breakdown -->
        <div class="space-y-4">
          <div class="card">
            <h2 class="heading-sm mb-4">Payer Breakdown</h2>
            <div class="space-y-3">
              <div *ngFor="let p of payerBreakdown">
                <div class="flex items-center justify-between text-xs mb-1">
                  <span class="text-gray-600 dark:text-gray-400 font-medium">{{ p.label }}</span>
                  <span class="font-semibold text-gray-900 dark:text-white tabular-nums">{{ p.pct }}%</span>
                </div>
                <div class="progress-bar h-1.5">
                  <div class="progress-fill" [style.width.%]="p.pct"></div>
                </div>
              </div>
            </div>
          </div>

          <div class="card-green p-4">
            <div class="flex items-start justify-between">
              <div>
                <p class="text-xs font-semibold text-primary-700 dark:text-primary-300">Clean Claims Rate</p>
                <p class="text-3xl font-extrabold text-primary-800 dark:text-primary-200 mt-1">98.4%</p>
                <p class="text-2xs text-primary-600 dark:text-primary-400 mt-1">↑ +2.1% vs last month</p>
              </div>
              <div class="icon-box-lg bg-white/60 dark:bg-primary-900/40 text-primary-700 dark:text-primary-300">
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
                </svg>
              </div>
            </div>
          </div>
        </div>

      </div>

    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BillingPageComponent implements OnInit {
  revenueStats: BillingStat[] = [
    { label: 'Total Billed (MTD)', value: '$124,850', change: '+12.4% vs last month', positive: true,  icon: 'M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z', iconClass: 'icon-box-primary' },
    { label: 'Collected (MTD)',    value: '$98,420',  change: '+8.2% collection rate', positive: true,  icon: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z', iconClass: 'icon-box-teal' },
    { label: 'Outstanding A/R',    value: '$26,430',  change: '-4.1% aged >30 days',   positive: true,  icon: 'M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z', iconClass: 'icon-box-amber' },
    { label: 'Claims Denied',      value: '1.6%',     change: '-0.4% denial rate',     positive: true,  icon: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z', iconClass: 'icon-box-red' },
  ];

  recentInvoices: Invoice[] = [
    { id: '1084', patient: 'Sarah Johnson',  initials: 'SJ', service: 'Annual Physical + Labs', date: new Date(2026, 6, 23), amount: 350, paid: 350, status: 'Paid',    color: 'linear-gradient(135deg,#16a34a,#15803d)' },
    { id: '1083', patient: 'Michael Chen',   initials: 'MC', service: 'Pulmonary Consult',     date: new Date(2026, 6, 22), amount: 220, paid: 0,   status: 'Pending', color: 'linear-gradient(135deg,#0d9488,#0f766e)' },
    { id: '1082', patient: 'Robert Davis',   initials: 'RD', service: 'Cardiology Follow-up',   date: new Date(2026, 6, 20), amount: 180, paid: 90,  status: 'Partial', color: 'linear-gradient(135deg,#d97706,#b45309)' },
    { id: '1081', patient: 'Linda Martinez', initials: 'LM', service: 'Lab Diagnostic Panel',   date: new Date(2026, 5, 28), amount: 145, paid: 0,   status: 'Overdue', color: 'linear-gradient(135deg,#dc2626,#b91c1c)' },
    { id: '1080', patient: 'Emma Williams',  initials: 'EW', service: 'Neurology Evaluation',   date: new Date(2026, 6, 18), amount: 410, paid: 410, status: 'Paid',    color: 'linear-gradient(135deg,#7c3aed,#6d28d9)' },
  ];

  payerBreakdown = [
    { label: 'Commercial / Private', pct: 52 },
    { label: 'Medicare',             pct: 28 },
    { label: 'Medicaid',             pct: 12 },
    { label: 'Self-Pay / Patient',   pct: 8 },
  ];

  statusClass(s: string): string {
    return s === 'Paid' ? 'badge-success' : s === 'Pending' ? 'badge-warning' : s === 'Partial' ? 'badge-info' : 'badge-danger';
  }

  ngOnInit(): void {}
}
