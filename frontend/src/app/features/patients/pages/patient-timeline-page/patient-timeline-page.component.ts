import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

interface TimelineEvent {
  id: string;
  date: Date;
  type: 'visit' | 'lab' | 'prescription' | 'imaging' | 'procedure' | 'alert';
  title: string;
  description: string;
  provider?: string;
  tags?: string[];
}

@Component({
  selector: 'app-patient-timeline-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="space-y-6 stagger">

      <!-- ── Header ───────────────────────────────────── -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div class="flex items-center gap-3">
          <a routerLink="/patients" class="btn-icon-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
            </svg>
          </a>
          <div>
            <h1 class="heading-xl">Clinical Timeline</h1>
            <p class="body-text mt-0.5">Sarah Johnson · MRN 00-1234</p>
          </div>
        </div>
        <div class="flex items-center gap-2 shrink-0">
          <button class="btn-secondary btn-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z"/>
            </svg>
            Filter Events
          </button>
          <a routerLink="/patients/1" class="btn-ghost btn-sm">Patient Profile</a>
        </div>
      </div>

      <!-- ── Event type filter pills ───────────────────── -->
      <div class="flex flex-wrap gap-2">
        <button *ngFor="let f of filters"
          (click)="activeFilter = f.key"
          [class]="activeFilter === f.key ? 'filter-pill-active' : 'filter-pill'"
          class="flex items-center gap-1.5">
          <span class="w-2 h-2 rounded-full" [style.background]="f.color"></span>
          {{ f.label }}
        </button>
      </div>

      <!-- ── Timeline ─────────────────────────────────── -->
      <div class="relative">
        <!-- Vertical line -->
        <div class="absolute left-[22px] top-0 bottom-0 w-0.5 bg-primary-100 dark:bg-primary-900/40"></div>

        <div class="space-y-4">
          <ng-container *ngFor="let event of filteredEvents(); let i = index">

            <!-- Year separator -->
            <div *ngIf="showYearSeparator(event, i)"
              class="relative flex items-center gap-3 py-2">
              <div class="w-11 h-11 rounded-full bg-primary-600 flex items-center justify-center text-white text-xs font-bold z-10 shrink-0 shadow-md shadow-primary-600/20">
                {{ event.date | date:'yyyy' }}
              </div>
              <div class="h-px flex-1 bg-primary-100 dark:bg-primary-900/30"></div>
            </div>

            <!-- Event card -->
            <div class="flex gap-4 items-start group">
              <!-- Icon node -->
              <div class="relative z-10 w-11 h-11 rounded-full flex items-center justify-center shrink-0 shadow-sm"
                [ngClass]="getEventStyle(event.type).bg">
                <svg class="w-5 h-5" [ngClass]="getEventStyle(event.type).icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75" [attr.d]="getEventStyle(event.type).path"/>
                </svg>
              </div>

              <!-- Content -->
              <div class="flex-1 min-w-0 card-hover pb-2">
                <div class="flex items-start justify-between gap-3 flex-wrap mb-2">
                  <div>
                    <p class="text-sm font-semibold text-gray-900 dark:text-white">{{ event.title }}</p>
                    <p class="text-xs text-gray-400 mt-0.5">
                      {{ event.date | date:'MMMM d, y' }}
                      <span *ngIf="event.provider"> &middot; {{ event.provider }}</span>
                    </p>
                  </div>
                  <span class="badge text-2xs shrink-0" [ngClass]="getEventStyle(event.type).badge">
                    {{ event.type | titlecase }}
                  </span>
                </div>
                <p class="text-sm text-gray-600 dark:text-gray-400 leading-relaxed">{{ event.description }}</p>
                <div class="flex flex-wrap gap-1.5 mt-3" *ngIf="event.tags && event.tags.length">
                  <span *ngFor="let t of event.tags" class="badge-neutral text-2xs">{{ t }}</span>
                </div>
              </div>
            </div>

          </ng-container>
        </div>
      </div>

    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatientTimelinePageComponent implements OnInit {
  activeFilter = 'all';

  filters = [
    { key: 'all',         label: 'All Events',    color: '#16a34a' },
    { key: 'visit',       label: 'Visits',         color: '#16a34a' },
    { key: 'lab',         label: 'Lab Results',    color: '#0d9488' },
    { key: 'prescription',label: 'Prescriptions',  color: '#7c3aed' },
    { key: 'imaging',     label: 'Imaging',        color: '#2563eb' },
    { key: 'alert',       label: 'Alerts',         color: '#dc2626' },
  ];

  events: TimelineEvent[] = [
    { id: '1',  date: new Date(2026, 6, 23), type: 'visit',        title: 'Annual Physical Exam',            description: 'Comprehensive annual exam. BP 138/88, slightly elevated. HbA1c 7.2%. Diabetes well-managed. Continued current medication regimen.', provider: 'Dr. Patel', tags: ['Hypertension','Diabetes'] },
    { id: '2',  date: new Date(2026, 6, 23), type: 'lab',          title: 'HbA1c & Metabolic Panel',         description: 'HbA1c: 7.2% (High). Fasting glucose: 128 mg/dL. LDL cholesterol: 94 mg/dL (Normal). eGFR: 78 mL/min (Normal).', provider: 'Lab Team', tags: ['HbA1c','Glucose','LDL'] },
    { id: '3',  date: new Date(2026, 5, 15), type: 'prescription', title: 'Metformin Dose Increased',        description: 'Metformin increased from 500mg BID to 1000mg BID due to sub-optimal glycemic control. Patient counseled on GI side effects.', provider: 'Dr. Patel', tags: ['Metformin','Diabetes Management'] },
    { id: '4',  date: new Date(2026, 3, 10), type: 'visit',        title: 'Diabetes Management Review',      description: 'Fasting glucose 128 mg/dL. Medication adjustment made. Dietary counseling provided with referral to nutritionist.', provider: 'Dr. Patel', tags: ['Diabetes','Follow-up'] },
    { id: '5',  date: new Date(2026, 1, 5),  type: 'alert',        title: 'Drug Interaction Warning',        description: 'Potential interaction flagged between Metformin and recent OTC ibuprofen purchase. Patient notified to avoid NSAIDs.', tags: ['Drug Interaction','Alert'] },
    { id: '6',  date: new Date(2026, 0, 10), type: 'visit',        title: 'Hypertension Follow-up',          description: 'BP improved from 142/90 to 138/88 with lifestyle modifications. Lisinopril 10mg continued. DASH diet reinforced.', provider: 'Dr. Patel', tags: ['Hypertension'] },
    { id: '7',  date: new Date(2025, 9, 20), type: 'imaging',      title: 'Abdominal Ultrasound',            description: 'Liver appears normal in echogenicity. No gallstones detected. Kidneys bilaterally normal in size. No hydronephrosis.', provider: 'Radiology', tags: ['Ultrasound','Abdominal'] },
    { id: '8',  date: new Date(2025, 6, 12), type: 'lab',          title: 'Annual Blood Work',               description: 'CBC normal. Comprehensive metabolic panel within limits. HbA1c 7.8% — slightly elevated, medication review scheduled.', provider: 'Lab Team', tags: ['HbA1c','CBC'] },
    { id: '9',  date: new Date(2025, 3, 5),  type: 'prescription', title: 'Lisinopril Initiated',            description: 'Lisinopril 10mg daily started for hypertension management. BP at time of prescription: 148/94. Follow-up scheduled in 4 weeks.', provider: 'Dr. Patel', tags: ['Lisinopril','Hypertension'] },
    { id: '10', date: new Date(2024, 11, 15),type: 'visit',        title: 'Type 2 Diabetes Diagnosis',       description: 'Fasting glucose 182 mg/dL, HbA1c 8.4% on two separate occasions. Type 2 Diabetes Mellitus diagnosed. Metformin 500mg BID initiated.', provider: 'Dr. Patel', tags: ['Diagnosis','Diabetes'] },
  ];

  filteredEvents(): TimelineEvent[] {
    if (this.activeFilter === 'all') return this.events;
    return this.events.filter(e => e.type === this.activeFilter);
  }

  showYearSeparator(event: TimelineEvent, i: number): boolean {
    if (i === 0) return true;
    const prev = this.filteredEvents()[i - 1];
    return event.date.getFullYear() !== prev.date.getFullYear();
  }

  getEventStyle(type: string): { bg: string; icon: string; path: string; badge: string } {
    const map: Record<string, { bg: string; icon: string; path: string; badge: string }> = {
      visit:        { bg: 'bg-primary-100 dark:bg-primary-900/50', icon: 'text-primary-700 dark:text-primary-300', badge: 'badge-success', path: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2' },
      lab:          { bg: 'bg-teal-100 dark:bg-teal-900/40',      icon: 'text-teal-700 dark:text-teal-300',   badge: 'badge-info',    path: 'M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z' },
      prescription: { bg: 'bg-violet-100 dark:bg-violet-900/40',  icon: 'text-violet-700 dark:text-violet-300',badge: 'badge-primary', path: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z' },
      imaging:      { bg: 'bg-primary-100 dark:bg-primary-900/40',icon: 'text-primary-600 dark:text-primary-400',badge: 'badge-success',path: 'M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z' },
      procedure:    { bg: 'bg-amber-100 dark:bg-amber-900/40',    icon: 'text-amber-700 dark:text-amber-300', badge: 'badge-warning', path: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4' },
      alert:        { bg: 'bg-red-100 dark:bg-red-900/40',        icon: 'text-red-700 dark:text-red-300',    badge: 'badge-danger',  path: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z' },
    };
    return map[type] || map['visit'];
  }

  ngOnInit(): void {}
}
