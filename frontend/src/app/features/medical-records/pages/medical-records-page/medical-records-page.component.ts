import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

interface MedicalRecord {
  id: string;
  patient: string;
  initials: string;
  type: string;
  category: string;
  date: Date;
  summary: string;
  provider: string;
  status: 'Final' | 'Draft' | 'Amended';
  color: string;
}

@Component({
  selector: 'app-medical-records-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="space-y-6 stagger">

      <!-- ── Header ──────────────────────────────── -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="heading-xl">Medical Records</h1>
          <p class="body-text mt-1">Patient health records and clinical documentation</p>
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
            New Record
          </button>
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

      <!-- ── Stats row ─────────────────────────────── -->
      <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
        <div *ngFor="let s of stats"
          class="card flex items-center gap-3 p-3.5">
          <div [ngClass]="s.iconClass" class="icon-box-md shrink-0">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75" [attr.d]="s.icon"/>
            </svg>
          </div>
          <div>
            <p class="text-base font-bold text-gray-900 dark:text-white tabular-nums">{{ s.value }}</p>
            <p class="text-2xs text-gray-500 dark:text-gray-400 font-medium">{{ s.label }}</p>
          </div>
        </div>
      </div>

      <!-- ── Records list ─────────────────────────── -->
      <div class="space-y-3">
        <div *ngFor="let r of filteredRecords()"
          class="card-hover group flex gap-4">

          <!-- Category icon -->
          <div [ngClass]="getCategoryIcon(r.category).box"
            class="icon-box-lg shrink-0 self-start">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75"
                [attr.d]="getCategoryIcon(r.category).path"/>
            </svg>
          </div>

          <!-- Content -->
          <div class="flex-1 min-w-0">
            <div class="flex items-start justify-between gap-3 mb-2 flex-wrap">
              <div>
                <p class="text-sm font-semibold text-gray-900 dark:text-white">{{ r.type }}</p>
                <p class="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                  {{ r.patient }} &middot; {{ r.provider }} &middot; {{ r.date | date:'MMM d, y' }}
                </p>
              </div>
              <div class="flex items-center gap-2 shrink-0">
                <span [ngClass]="getStatusClass(r.status)" class="badge">{{ r.status }}</span>
                <span class="badge-neutral text-2xs">{{ r.category }}</span>
              </div>
            </div>
            <p class="text-sm text-gray-600 dark:text-gray-400 leading-relaxed">{{ r.summary }}</p>
            <div class="flex items-center gap-3 mt-3">
              <button class="text-xs font-semibold text-primary-600 hover:text-primary-700
                             dark:text-primary-400 transition-colors flex items-center gap-1">
                <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                    d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                    d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/>
                </svg>
                View
              </button>
              <button class="text-xs font-medium text-gray-400 hover:text-gray-600 dark:hover:text-gray-200
                             transition-colors flex items-center gap-1">
                <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                    d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12"/>
                </svg>
                Download
              </button>
            </div>
          </div>
        </div>
      </div>

    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MedicalRecordsPageComponent implements OnInit {
  activeCategory = 'All';
  categories = ['All', 'Clinical Notes', 'Lab Results', 'Imaging', 'Prescriptions', 'Procedures'];

  stats = [
    { value: '342',  label: 'Total Records',     icon: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z', iconClass: 'icon-box-primary' },
    { value: '28',   label: 'This Month',         icon: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z', iconClass: 'icon-box-blue' },
    { value: '5',    label: 'Awaiting Signature', icon: 'M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z', iconClass: 'icon-box-amber' },
    { value: '12',   label: 'Draft Records',      icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2', iconClass: 'icon-box-teal' },
  ];

  records: MedicalRecord[] = [
    { id: '1', patient: 'Sarah Johnson',  initials: 'SJ', type: 'SOAP Note — General Checkup',      category: 'Clinical Notes', date: new Date(2026, 6, 23), summary: 'Patient presents for routine annual physical. BP 120/80, HR 72 bpm, all vitals within normal limits. Diabetic management reviewed.', provider: 'Dr. Patel',  status: 'Final',   color: '#16a34a' },
    { id: '2', patient: 'Robert Davis',   initials: 'RD', type: 'Cardiology Consultation Report',    category: 'Clinical Notes', date: new Date(2026, 6, 22), summary: 'Patient referred for evaluation of chest pain. ECG shows ST-segment changes. Echocardiogram recommended. Carvedilol 6.25mg BID initiated.', provider: 'Dr. Garcia', status: 'Final',   color: '#dc2626' },
    { id: '3', patient: 'Emma Williams',  initials: 'EW', type: 'CBC and Metabolic Panel',           category: 'Lab Results',    date: new Date(2026, 6, 21), summary: 'Complete blood count shows mild leukocytosis. Comprehensive metabolic panel within normal range. Cortisol elevated, further evaluation recommended.', provider: 'Lab Team',  status: 'Final',   color: '#7c3aed' },
    { id: '4', patient: 'Michael Chen',   initials: 'MC', type: 'Chest X-Ray Report',               category: 'Imaging',        date: new Date(2026, 6, 20), summary: 'PA and lateral chest radiograph. Lungs are hyperinflated, consistent with COPD. No acute cardiopulmonary process identified.', provider: 'Dr. Patel',  status: 'Final',   color: '#2563eb' },
    { id: '5', patient: 'Linda Martinez', initials: 'LM', type: 'Levothyroxine Prescription',        category: 'Prescriptions',  date: new Date(2026, 6, 19), summary: 'Levothyroxine 75mcg daily. 90-day supply with 3 refills. TSH to be rechecked in 6 weeks. Patient counseled on administration.', provider: 'Dr. Patel',  status: 'Final',   color: '#0d9488' },
    { id: '6', patient: 'James Wilson',   initials: 'JW', type: 'Spirometry Assessment',             category: 'Procedures',     date: new Date(2026, 6, 18), summary: 'Pulmonary function testing reveals moderate obstructive pattern (FEV1/FVC = 0.58). Consistent with COPD diagnosis. Inhaler technique reviewed.', provider: 'Dr. Smith', status: 'Draft',   color: '#d97706' },
  ];

  filteredRecords(): MedicalRecord[] {
    if (this.activeCategory === 'All') return this.records;
    return this.records.filter(r => r.category === this.activeCategory);
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      'Final':   'badge-success',
      'Draft':   'badge-warning',
      'Amended': 'badge-info',
    };
    return map[status] || 'badge-neutral';
  }

  getCategoryIcon(category: string): { box: string; path: string } {
    const map: Record<string, { box: string; path: string }> = {
      'Clinical Notes': { box: 'icon-box-primary', path: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01m-.01 4h.01' },
      'Lab Results':    { box: 'icon-box-teal',    path: 'M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z' },
      'Imaging':        { box: 'icon-box-blue',    path: 'M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z' },
      'Prescriptions':  { box: 'icon-box-purple',  path: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z' },
      'Procedures':     { box: 'icon-box-amber',   path: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4' },
    };
    return map[category] || map['Clinical Notes'];
  }

  ngOnInit(): void {}
}
