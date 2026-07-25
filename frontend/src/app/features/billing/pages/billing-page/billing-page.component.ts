import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

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
  imports: [CommonModule, RouterModule],
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

      <!-- ── Revenue stats ────────────────────────── -->
      <div class="grid-stats">
        <div *ngFor="let s of revenueStats; let i = index"
          class="stat-card animate-count-up"
          [style.animation-delay]="i * 70 + 'ms'">
          <div class="flex items-start justify-between gap-2">
            <div class="min-w-0">
              <p class="stat-label">{{ s.label }}</p>
              <p class="stat-value mt-1.5">{{ s.value }}</p>
            </div>
            <div [ngClass]="s.iconClass" class="icon-box-lg shrink-0">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75" [attr.d]="s.icon"/>
              </svg>
            </div>
          </div>
          <div class="mt-3" [ngClass]="s.positive ? 'stat-change positive' : 'stat-change negative'">
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5"
                [attr.d]="s.positive ? 'M5 15l7-7 7 7' : 'M19 9l-7 7-7-7'"/>
            </svg>
            <span>{{ s.change }}</span>
          </div>
        </div>
      </div>

      <!-- ── Collection rate ──────────────────────── -->
      <div class="card">
        <div class="flex items-center justify-between mb-4">
          <div>
            <h2 class="heading-sm">Collection Rate</h2>
            <p class="body-text mt-0.5">Monthly payment collection progress</p>
          </div>
          <span class="text-2xl font-bold text-primary-600 dark:text-primary-400">87.4%</span>
        </div>
        <div class="progress-bar">
          <div class="progress-fill" style="width: 87.4%"></div>
        </div>
        <div class="flex items-center justify-between mt-3 text-xs text-gray-500 dark:text-gray-400">
          <span>Collected: <span class="font-semibold text-primary-600">$42,800</span></span>
          <span>Target: $49,000</span>
        </div>
      </div>

      <!-- ── Invoices table ────────────────────────── -->
      <div class="card p-0 overflow-hidden">
        <div class="card-header">
          <h2 class="heading-sm">Recent Invoices</h2>
          <div class="flex items-center gap-2">
            <span *ngFor="let f of invoiceFilters"
              class="badge-neutral cursor-pointer hover:badge-primary transition-all duration-150">{{ f }}</span>
          </div>
        </div>

        <div class="overflow-x-auto">
          <table class="table-base">
            <thead>
              <tr>
                <th>Patient</th>
                <th>Service</th>
                <th>Date</th>
                <th>Amount</th>
                <th>Balance</th>
                <th>Status</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let inv of invoices">
                <td>
                  <div class="flex items-center gap-2.5">
                    <div class="avatar-custom-md" [style.background]="inv.color">
                      {{ inv.initials }}
                    </div>
                    <span class="font-medium text-gray-900 dark:text-white">{{ inv.patient }}</span>
                  </div>
                </td>
                <td class="text-gray-600 dark:text-gray-400">{{ inv.service }}</td>
                <td class="text-gray-500 dark:text-gray-400 text-xs">{{ inv.date | date:'MMM d, y' }}</td>
                <td class="font-semibold text-gray-900 dark:text-white">\${{ inv.amount | number:'1.2-2' }}</td>
                <td>
                  <span [class]="inv.paid >= inv.amount ? 'text-primary-600 dark:text-primary-400 font-semibold' : 'text-red-500 dark:text-red-400 font-semibold'">
                    \${{ (inv.amount - inv.paid) | number:'1.2-2' }}
                  </span>
                </td>
                <td>
                  <span [ngClass]="getInvoiceStatusClass(inv.status)" class="badge">{{ inv.status }}</span>
                </td>
                <td>
                  <button class="btn-icon-sm">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                        d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z"/>
                    </svg>
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BillingPageComponent implements OnInit {
  invoiceFilters = ['All', 'Pending', 'Overdue'];

  revenueStats = [
    { label: 'Total Revenue',    value: '$49,200', change: '+8.2% vs last month', positive: true,  icon: 'M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z', iconClass: 'icon-box-primary' },
    { label: 'Collected',        value: '$42,800', change: '+5.1% vs last month', positive: true,  icon: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z', iconClass: 'icon-box-teal' },
    { label: 'Outstanding',      value: '$6,400',  change: '-12% vs last month',  positive: true,  icon: 'M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z', iconClass: 'icon-box-amber' },
    { label: 'Overdue',          value: '$1,820',  change: '+2 invoices',         positive: false, icon: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z', iconClass: 'icon-box-red' },
  ];

  invoices: Invoice[] = [
    { id: 'INV-001', patient: 'Sarah Johnson',  initials: 'SJ', service: 'General Checkup',    date: new Date(2026, 6, 20), amount: 250,   paid: 250,   status: 'Paid',    color: '#16a34a' },
    { id: 'INV-002', patient: 'Michael Chen',   initials: 'MC', service: 'Pulmonology Consult', date: new Date(2026, 6, 18), amount: 480,   paid: 0,     status: 'Pending', color: '#2563eb' },
    { id: 'INV-003', patient: 'Emma Williams',  initials: 'EW', service: 'Neurology Review',    date: new Date(2026, 6, 15), amount: 320,   paid: 160,   status: 'Partial', color: '#7c3aed' },
    { id: 'INV-004', patient: 'Robert Davis',   initials: 'RD', service: 'Cardiology + Echo',   date: new Date(2026, 5, 30), amount: 1200,  paid: 0,     status: 'Overdue', color: '#dc2626' },
    { id: 'INV-005', patient: 'Linda Martinez', initials: 'LM', service: 'Annual Physical',     date: new Date(2026, 6, 22), amount: 180,   paid: 180,   status: 'Paid',    color: '#0d9488' },
    { id: 'INV-006', patient: 'James Wilson',   initials: 'JW', service: 'COPD Follow-up',      date: new Date(2026, 6, 10), amount: 290,   paid: 0,     status: 'Overdue', color: '#d97706' },
  ];

  getInvoiceStatusClass(status: string): string {
    const map: Record<string, string> = {
      'Paid':    'badge-success',
      'Pending': 'badge-warning',
      'Overdue': 'badge-danger',
      'Partial': 'badge-info',
    };
    return map[status] || 'badge-neutral';
  }

  ngOnInit(): void {}
}
