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
  templateUrl: './patient-timeline-page.component.html',
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
