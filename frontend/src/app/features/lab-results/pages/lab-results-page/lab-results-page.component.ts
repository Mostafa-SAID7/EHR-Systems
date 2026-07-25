import { Component, OnInit, ChangeDetectionStrategy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LabStatusStripComponent, LabStatusStat } from '../../components/lab-status-strip/lab-status-strip.component';
import { LabResultCardsComponent, LabResult } from '../../components/lab-result-cards/lab-result-cards.component';

@Component({
  selector: 'app-lab-results-page',
  standalone: true,
  imports: [CommonModule, RouterModule, LabStatusStripComponent, LabResultCardsComponent],
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

      <!-- ── Status strip (subcomponent) ─────────── -->
      <app-lab-status-strip [stats]="statusSummary"></app-lab-status-strip>

      <!-- ── Category filter ──────────────────────── -->
      <div class="filter-bar">
        <button *ngFor="let cat of categories; trackBy: trackByValue"
          (click)="setCategory(cat)"
          [class]="activeCategory() === cat ? 'filter-pill-active' : 'filter-pill'">
          {{ cat }}
        </button>
      </div>

      <!-- ── Result cards (subcomponent) ─────────── -->
      <app-lab-result-cards [results]="filteredResults()"></app-lab-result-cards>

    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabResultsPageComponent implements OnInit {
  readonly activeCategory = signal('All');

  categories = ['All', 'Hematology', 'Chemistry', 'Microbiology', 'Cardiology', 'Endocrinology'];

  statusSummary: LabStatusStat[] = [
    { label: 'Normal',   count: 18, icon: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z',                                                                                              iconClass: 'icon-box-primary' },
    { label: 'High',     count: 4,  icon: 'M5 15l7-7 7 7',                                                                                                                               iconClass: 'icon-box-red' },
    { label: 'Low',      count: 2,  icon: 'M19 9l-7 7-7-7',                                                                                                                              iconClass: 'icon-box-blue' },
    { label: 'Critical', count: 1,  icon: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z',     iconClass: 'icon-box-red' },
    { label: 'Pending',  count: 3,  icon: 'M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z',                                                                                                iconClass: 'icon-box-amber' },
  ];

  private readonly _results: LabResult[] = [
    { id: '1', patient: 'Sarah Johnson',  initials: 'SJ', test: 'HbA1c',          category: 'Endocrinology', date: new Date(2026, 6, 23), value: '7.2',  unit: '%',      range: '4.0–5.6',  status: 'High',    color: '#16a34a' },
    { id: '2', patient: 'Michael Chen',   initials: 'MC', test: 'WBC Count',      category: 'Hematology',    date: new Date(2026, 6, 22), value: '9.8',  unit: 'K/µL',   range: '4.5–11.0', status: 'Normal',  color: '#2563eb' },
    { id: '3', patient: 'Emma Williams',  initials: 'EW', test: 'Serum Cortisol', category: 'Endocrinology', date: new Date(2026, 6, 21), value: '28.4', unit: 'µg/dL',  range: '6.0–23.0', status: 'Critical',color: '#7c3aed' },
    { id: '4', patient: 'Robert Davis',   initials: 'RD', test: 'Troponin I',     category: 'Cardiology',    date: new Date(2026, 6, 20), value: '0.04', unit: 'ng/mL',  range: '< 0.04',   status: 'High',    color: '#dc2626' },
    { id: '5', patient: 'Linda Martinez', initials: 'LM', test: 'TSH',            category: 'Endocrinology', date: new Date(2026, 6, 19), value: '4.1',  unit: 'mIU/L',  range: '0.4–4.0',  status: 'High',    color: '#0d9488' },
    { id: '6', patient: 'James Wilson',   initials: 'JW', test: 'eGFR',           category: 'Chemistry',     date: new Date(2026, 6, 18), value: '48',   unit: 'mL/min', range: '> 60',     status: 'Low',     color: '#d97706' },
  ];

  /** Computed — recalculates only when activeCategory signal changes */
  readonly filteredResults = computed(() => {
    const cat = this.activeCategory();
    return cat === 'All' ? this._results : this._results.filter(r => r.category === cat);
  });

  setCategory(cat: string): void { this.activeCategory.set(cat); }

  trackByValue(_: number, val: string): string { return val; }

  ngOnInit(): void {}
}
