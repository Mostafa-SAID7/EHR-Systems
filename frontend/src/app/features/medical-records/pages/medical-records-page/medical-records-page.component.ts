import { Component, OnInit, ChangeDetectionStrategy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MedicalRecordStatsComponent, RecordStat } from '../../components/medical-record-stats/medical-record-stats.component';
import { MedicalRecordListComponent, MedicalRecord } from '../../components/medical-record-list/medical-record-list.component';

@Component({
  selector: 'app-medical-records-page',
  standalone: true,
  imports: [CommonModule, RouterModule, MedicalRecordStatsComponent, MedicalRecordListComponent],
  templateUrl: './medical-records-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MedicalRecordsPageComponent implements OnInit {
  /** Signal-backed active filter — drives computed list without method calls on every CD tick */
  readonly activeCategory = signal('All');

  categories = ['All', 'Clinical Notes', 'Lab Results', 'Imaging', 'Prescriptions', 'Procedures'];

  stats: RecordStat[] = [
    { value: '342', label: 'Total Records',     icon: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z', iconClass: 'icon-box-primary' },
    { value: '28',  label: 'This Month',        icon: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z', iconClass: 'icon-box-blue' },
    { value: '5',   label: 'Awaiting Signature',icon: 'M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z', iconClass: 'icon-box-amber' },
    { value: '12',  label: 'Draft Records',     icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2', iconClass: 'icon-box-teal' },
  ];

  private readonly _records: MedicalRecord[] = [
    { id: '1', patient: 'Sarah Johnson',  initials: 'SJ', type: 'SOAP Note — General Checkup',    category: 'Clinical Notes', date: new Date(2026, 6, 23), summary: 'Patient presents for routine annual physical. BP 120/80, HR 72 bpm, all vitals within normal limits. Diabetic management reviewed.', provider: 'Dr. Patel',  status: 'Final', color: '#16a34a' },
    { id: '2', patient: 'Robert Davis',   initials: 'RD', type: 'Cardiology Consultation Report',  category: 'Clinical Notes', date: new Date(2026, 6, 22), summary: 'Patient referred for evaluation of chest pain. ECG shows ST-segment changes. Echocardiogram recommended. Carvedilol 6.25mg BID initiated.', provider: 'Dr. Garcia', status: 'Final', color: '#dc2626' },
    { id: '3', patient: 'Emma Williams',  initials: 'EW', type: 'CBC and Metabolic Panel',         category: 'Lab Results',    date: new Date(2026, 6, 21), summary: 'Complete blood count shows mild leukocytosis. Comprehensive metabolic panel within normal range. Cortisol elevated, further evaluation recommended.', provider: 'Lab Team',  status: 'Final', color: '#7c3aed' },
    { id: '4', patient: 'Michael Chen',   initials: 'MC', type: 'Chest X-Ray Report',             category: 'Imaging',        date: new Date(2026, 6, 20), summary: 'PA and lateral chest radiograph. Lungs are hyperinflated, consistent with COPD. No acute cardiopulmonary process identified.', provider: 'Dr. Patel',  status: 'Final', color: '#2563eb' },
    { id: '5', patient: 'Linda Martinez', initials: 'LM', type: 'Levothyroxine Prescription',     category: 'Prescriptions',  date: new Date(2026, 6, 19), summary: 'Levothyroxine 75mcg daily. 90-day supply with 3 refills. TSH to be rechecked in 6 weeks. Patient counseled on administration.', provider: 'Dr. Patel',  status: 'Final', color: '#0d9488' },
    { id: '6', patient: 'James Wilson',   initials: 'JW', type: 'Spirometry Assessment',          category: 'Procedures',     date: new Date(2026, 6, 18), summary: 'Pulmonary function testing reveals moderate obstructive pattern (FEV1/FVC = 0.58). Consistent with COPD diagnosis. Inhaler technique reviewed.', provider: 'Dr. Smith', status: 'Draft', color: '#d97706' },
  ];

  /** Computed — recalculates ONLY when activeCategory signal changes, not on every CD cycle */
  readonly filteredRecords = computed(() => {
    const cat = this.activeCategory();
    return cat === 'All' ? this._records : this._records.filter(r => r.category === cat);
  });

  setCategory(cat: string): void { this.activeCategory.set(cat); }

  trackByValue(_: number, val: string): string { return val; }

  ngOnInit(): void {}
}
