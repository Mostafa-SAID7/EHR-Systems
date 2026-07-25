import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PrescriptionStatsStripComponent, PrescriptionStat } from '../../components/prescription-stats-strip/prescription-stats-strip.component';

interface Prescription {
  id: string;
  patient: string;
  initials: string;
  drug: string;
  dosage: string;
  frequency: string;
  prescribedBy: string;
  date: Date;
  refills: number;
  status: 'Active' | 'Expired' | 'Discontinued' | 'Pending';
  color: string;
}

@Component({
  selector: 'app-prescription-list-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    PrescriptionStatsStripComponent
  ],
  template: `
    <div class="space-y-6 stagger">

      <!-- ── Header ───────────────────────────────────── -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="heading-xl">Prescriptions</h1>
          <p class="body-text mt-1">Manage and track all patient e-prescriptions</p>
        </div>
        <div class="flex items-center gap-2 shrink-0">
          <button class="btn-secondary btn-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/>
            </svg>
            Export
          </button>
          <a routerLink="/prescriptions/new" class="btn-primary btn-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
            </svg>
            New e-Rx
          </a>
        </div>
      </div>

      <!-- ── Stats strip subcomponent ───────────────────── -->
      <app-prescription-stats-strip
        [stats]="stats"
      ></app-prescription-stats-strip>

      <!-- ── Search + filter ───────────────────────────── -->
      <div class="flex flex-col sm:flex-row gap-3">
        <div class="relative flex-1">
          <div class="absolute inset-y-0 left-0 flex items-center pl-3.5 pointer-events-none">
            <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
            </svg>
          </div>
          <input type="text" [(ngModel)]="searchQuery" placeholder="Search by patient name or drug…" class="input-icon w-full"/>
        </div>
        <div class="flex items-center gap-2 flex-wrap">
          <button *ngFor="let f of filters"
            (click)="activeFilter = f"
            [class]="activeFilter === f ? 'filter-pill-active' : 'filter-pill'">{{ f }}</button>
        </div>
      </div>

      <!-- ── Prescriptions table ──────────────────────── -->
      <div class="card p-0 overflow-hidden">
        <div class="card-header">
          <h2 class="heading-sm">All Prescriptions</h2>
          <span class="badge-primary">{{ filtered().length }} results</span>
        </div>
        <div class="overflow-x-auto">
          <table class="table-base">
            <thead>
              <tr>
                <th>Patient</th>
                <th>Medication</th>
                <th>Dosage &amp; Frequency</th>
                <th>Prescribed By</th>
                <th>Date</th>
                <th>Refills Left</th>
                <th>Status</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let rx of filtered()"
                class="hover:bg-primary-50/30 dark:hover:bg-primary-900/10 transition-colors cursor-pointer">
                <td>
                  <div class="flex items-center gap-2.5">
                    <div class="avatar-custom-md" [style.background]="rx.color">{{ rx.initials }}</div>
                    <span class="font-medium text-gray-900 dark:text-white">{{ rx.patient }}</span>
                  </div>
                </td>
                <td class="font-semibold text-gray-900 dark:text-white">{{ rx.drug }}</td>
                <td class="text-sm text-gray-600 dark:text-gray-400">{{ rx.dosage }} &middot; {{ rx.frequency }}</td>
                <td class="text-xs text-gray-500 dark:text-gray-400">{{ rx.prescribedBy }}</td>
                <td class="text-xs text-gray-500 dark:text-gray-400">{{ rx.date | date:'MMM d, y' }}</td>
                <td>
                  <span class="text-sm font-semibold"
                    [ngClass]="rx.refills > 0 ? 'text-primary-600 dark:text-primary-400' : 'text-red-500 dark:text-red-400'">
                    {{ rx.refills }}
                  </span>
                </td>
                <td>
                  <span [ngClass]="statusClass(rx.status)" class="badge">{{ rx.status }}</span>
                </td>
                <td>
                  <div class="flex items-center gap-1">
                    <a [routerLink]="['/prescriptions', rx.id]" class="btn-icon-sm" title="View details">
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/>
                      </svg>
                    </a>
                    <button class="btn-icon-sm" title="Refill">
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
                      </svg>
                    </button>
                  </div>
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
export class PrescriptionListPageComponent implements OnInit {
  searchQuery = '';
  activeFilter = 'All';
  filters = ['All', 'Active', 'Pending', 'Expired', 'Discontinued'];

  stats: PrescriptionStat[] = [
    { label: 'Total e-Rx',    value: '284',  icon: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z', iconClass: 'icon-box-primary' },
    { label: 'Active',        value: '198',  icon: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z', iconClass: 'icon-box-teal' },
    { label: 'Pending Refill',value: '12',   icon: 'M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z', iconClass: 'icon-box-amber' },
    { label: 'Sent Today',    value: '24',   icon: 'M12 19l9 2-9-18-9 18 9-2zm0 0v-8', iconClass: 'icon-box-primary' },
  ];

  prescriptions: Prescription[] = [
    { id: '1', patient: 'Sarah Johnson',  initials: 'SJ', drug: 'Metformin 1000mg',     dosage: '1000mg', frequency: 'Twice daily',   prescribedBy: 'Dr. Patel',   date: new Date(2026, 5, 15), refills: 3, status: 'Active',       color: 'linear-gradient(135deg,#16a34a,#15803d)' },
    { id: '2', patient: 'Sarah Johnson',  initials: 'SJ', drug: 'Lisinopril 10mg',      dosage: '10mg',   frequency: 'Once daily',     prescribedBy: 'Dr. Patel',   date: new Date(2026, 3, 10), refills: 5, status: 'Active',       color: 'linear-gradient(135deg,#16a34a,#15803d)' },
    { id: '3', patient: 'Michael Chen',   initials: 'MC', drug: 'Albuterol Inhaler',    dosage: '90mcg',  frequency: 'As needed',      prescribedBy: 'Dr. Smith',   date: new Date(2026, 6, 1),  refills: 2, status: 'Active',       color: 'linear-gradient(135deg,#0d9488,#0f766e)' },
    { id: '4', patient: 'Robert Davis',   initials: 'RD', drug: 'Carvedilol 6.25mg',    dosage: '6.25mg', frequency: 'Twice daily',     prescribedBy: 'Dr. Garcia',  date: new Date(2026, 6, 22), refills: 0, status: 'Pending',      color: 'linear-gradient(135deg,#dc2626,#b91c1c)' },
    { id: '5', patient: 'Linda Martinez', initials: 'LM', drug: 'Levothyroxine 75mcg',  dosage: '75mcg',  frequency: 'Once daily AM',   prescribedBy: 'Dr. Patel',   date: new Date(2026, 6, 19), refills: 2, status: 'Active',       color: 'linear-gradient(135deg,#d97706,#b45309)' },
    { id: '6', patient: 'Emma Williams',  initials: 'EW', drug: 'Sumatriptan 100mg',    dosage: '100mg',  frequency: 'As needed',      prescribedBy: 'Dr. Patel',   date: new Date(2025, 11, 1), refills: 0, status: 'Expired',      color: 'linear-gradient(135deg,#7c3aed,#6d28d9)' },
    { id: '7', patient: 'James Wilson',   initials: 'JW', drug: 'Tiotropium Inhaler',   dosage: '18mcg',  frequency: 'Once daily',     prescribedBy: 'Dr. Smith',   date: new Date(2026, 2, 10), refills: 1, status: 'Active',       color: 'linear-gradient(135deg,#16a34a,#4ade80)' },
    { id: '8', patient: 'Robert Davis',   initials: 'RD', drug: 'Furosemide 40mg',      dosage: '40mg',   frequency: 'Once daily',     prescribedBy: 'Dr. Garcia',  date: new Date(2025, 8, 1),  refills: 0, status: 'Discontinued', color: 'linear-gradient(135deg,#dc2626,#b91c1c)' },
  ];

  filtered(): Prescription[] {
    let list = this.prescriptions;
    if (this.activeFilter !== 'All') list = list.filter(rx => rx.status === this.activeFilter);
    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase();
      list = list.filter(rx => rx.patient.toLowerCase().includes(q) || rx.drug.toLowerCase().includes(q));
    }
    return list;
  }

  statusClass(s: string): string {
    return s === 'Active' ? 'badge-success' : s === 'Pending' ? 'badge-warning' : s === 'Expired' ? 'badge-danger' : 'badge-neutral';
  }

  ngOnInit(): void {}
}
