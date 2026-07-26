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
  templateUrl: './billing-page.component.html',
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
