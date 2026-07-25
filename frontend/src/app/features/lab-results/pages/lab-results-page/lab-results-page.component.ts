import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

interface LabResult {
  id: string;
  patient: string;
  initials: string;
  test: string;
  category: string;
  date: Date;
  value: string;
  unit: string;
  range: string;
  status: 'Normal' | 'High' | 'Low' | 'Critical' | 'Pending';
  color: string;
}

@Component({
  selector: 'app-lab-results-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="space-y-6 stagger">

      <!-- ── Header ──────────────────────────────── -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="heading-xl">Lab Results</h1>
          <p class="body-text mt-1">Review and manage patient laboratory results</p>
        </div>
        <div class="flex items-center gap-2 shrink-0">
          <button class="btn-secondary btn-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z"/>
            </svg>
            Filter
          </button>
          <button class="btn-primary btn-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
            </svg>
            New Order
          </button>
        </div>
      </div>

      <!-- ── Status overview ──────────────────────── -->
      <div class="grid grid-cols-2 sm:grid-cols-5 gap-3">
        <div *ngFor="let s of statusSummary"
          class="card p-3 flex items-center gap-2.5 cursor-pointer
                 hover:shadow-card-hover hover:-translate-y-0.5 transition-all duration-200">
          <div class="w-8 h-8 rounded-xl shrink-0 flex items-center justify-center"
            [ngClass]="s.iconClass">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75" [attr.d]="s.icon"/>
            </svg>
          </div>
          <div>
            <p class="text-base font-bold text-gray-900 dark:text-white tabular-nums">{{ s.count }}</p>
            <p class="text-2xs text-gray-500 dark:text-gray-400 font-medium">{{ s.label }}</p>
          </div>
        </div>
      </div>

      <!-- ── Category filter ──────────────────────── -->
      <div class="flex gap-2 flex-wrap">
        <button *ngFor="let cat of categories"
          (click)="activeCategory = cat"
          [class]="activeCategory === cat ? 'filter-pill-active' : 'filter-pill'">
          {{ cat }}
        </button>
      </div>

      <!-- ── Results grid ──────────────────────────── -->
      <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
        <div *ngFor="let r of filteredResults()"
          class="card-hover group">
          <div class="flex items-start justify-between gap-3 mb-3">
            <div class="flex items-center gap-2.5">
              <div class="w-9 h-9 rounded-xl shrink-0 flex items-center justify-center
                          text-white text-xs font-bold shadow-sm"
                [style.background]="r.color">
                {{ r.initials }}
              </div>
              <div class="min-w-0">
                <p class="text-sm font-semibold text-gray-900 dark:text-white truncate">{{ r.patient }}</p>
                <p class="text-2xs text-gray-500 dark:text-gray-400">{{ r.date | date:'MMM d, y' }}</p>
              </div>
            </div>
            <span [ngClass]="getResultStatusClass(r.status)" class="badge shrink-0">{{ r.status }}</span>
          </div>

          <div class="card-green p-3 mb-3">
            <p class="text-xs font-semibold text-gray-700 dark:text-gray-300 mb-2">{{ r.test }}</p>
            <div class="flex items-end justify-between">
              <div>
                <span [ngClass]="getValueClass(r.status)"
                  class="text-2xl font-bold tabular-nums">{{ r.value }}</span>
                <span class="text-xs text-gray-500 dark:text-gray-400 ml-1.5">{{ r.unit }}</span>
              </div>
              <span class="text-xs text-gray-400 dark:text-gray-500">
                Ref: {{ r.range }}
              </span>
            </div>
          </div>

          <div class="flex items-center justify-between">
            <span class="badge-neutral text-2xs">{{ r.category }}</span>
            <button class="text-xs font-semibold text-primary-600 hover:text-primary-700
                           dark:text-primary-400 transition-colors flex items-center gap-0.5">
              Details
              <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
              </svg>
            </button>
          </div>
        </div>
      </div>

    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabResultsPageComponent implements OnInit {
  activeCategory = 'All';
  categories = ['All', 'Hematology', 'Chemistry', 'Microbiology', 'Cardiology', 'Endocrinology'];

  statusSummary = [
    { label: 'Normal',   count: 18, icon: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z', iconClass: 'icon-box-primary' },
    { label: 'High',     count: 4,  icon: 'M5 15l7-7 7 7', iconClass: 'icon-box-red' },
    { label: 'Low',      count: 2,  icon: 'M19 9l-7 7-7-7', iconClass: 'icon-box-blue' },
    { label: 'Critical', count: 1,  icon: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z', iconClass: 'icon-box-red' },
    { label: 'Pending',  count: 3,  icon: 'M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z', iconClass: 'icon-box-amber' },
  ];

  results: LabResult[] = [
    { id: '1', patient: 'Sarah Johnson',  initials: 'SJ', test: 'HbA1c',          category: 'Endocrinology', date: new Date(2026, 6, 23), value: '7.2',  unit: '%',      range: '4.0–5.6',    status: 'High',    color: '#16a34a' },
    { id: '2', patient: 'Michael Chen',   initials: 'MC', test: 'WBC Count',      category: 'Hematology',    date: new Date(2026, 6, 22), value: '9.8',  unit: 'K/µL',   range: '4.5–11.0',   status: 'Normal',  color: '#2563eb' },
    { id: '3', patient: 'Emma Williams',  initials: 'EW', test: 'Serum Cortisol', category: 'Endocrinology', date: new Date(2026, 6, 21), value: '28.4', unit: 'µg/dL',  range: '6.0–23.0',   status: 'Critical',color: '#7c3aed' },
    { id: '4', patient: 'Robert Davis',   initials: 'RD', test: 'Troponin I',     category: 'Cardiology',    date: new Date(2026, 6, 20), value: '0.04', unit: 'ng/mL',  range: '< 0.04',     status: 'High',    color: '#dc2626' },
    { id: '5', patient: 'Linda Martinez', initials: 'LM', test: 'TSH',            category: 'Endocrinology', date: new Date(2026, 6, 19), value: '4.1',  unit: 'mIU/L',  range: '0.4–4.0',    status: 'High',    color: '#0d9488' },
    { id: '6', patient: 'James Wilson',   initials: 'JW', test: 'eGFR',           category: 'Chemistry',     date: new Date(2026, 6, 18), value: '48',   unit: 'mL/min', range: '> 60',       status: 'Low',     color: '#d97706' },
  ];

  filteredResults(): LabResult[] {
    if (this.activeCategory === 'All') return this.results;
    return this.results.filter(r => r.category === this.activeCategory);
  }

  getResultStatusClass(status: string): string {
    const map: Record<string, string> = {
      'Normal':   'badge-success',
      'High':     'badge-danger',
      'Low':      'badge-info',
      'Critical': 'badge-danger',
      'Pending':  'badge-warning',
    };
    return map[status] || 'badge-neutral';
  }

  getValueClass(status: string): string {
    if (status === 'Critical' || status === 'High') return 'text-red-600 dark:text-red-400';
    if (status === 'Low') return 'text-blue-600 dark:text-blue-400';
    return 'text-primary-700 dark:text-primary-300';
  }

  ngOnInit(): void {}
}
