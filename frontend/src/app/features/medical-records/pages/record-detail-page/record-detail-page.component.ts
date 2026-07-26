import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-record-detail-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './record-detail-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecordDetailPageComponent {
  noteTabs  = ['All', 'Subjective', 'Objective', 'Assessment', 'Plan'];
  activeTab = 'All';

  record = {
    id: 'MR-2026-1182',
    title: 'Office Visit — Diabetes Follow-up',
    category: 'Office Visit',
    date: new Date(2026, 6, 20),
    status: 'Final',
    patient: 'Sarah Johnson',
    patientInitials: 'SJ',
    mrn: '00-1234',
    provider: 'Dr. Ramesh Patel',
    providerInitials: 'RP',
    providerSpecialty: 'Internal Medicine',
    facility: 'Sunrise Medical Center',
    modified: 'Jul 20, 2026 — 4:30 PM',
    version: 'v1.0 (Final)',
    subjective: `Patient presents for routine diabetes management follow-up. Reports improved adherence to Metformin regimen over the past 3 months. Denies hypoglycemic episodes. Occasional fatigue in the afternoon noted. Blood glucose home monitoring shows fasting levels ranging 110–140 mg/dL. Patient has been following a low-carbohydrate diet with moderate success. No new complaints. Denies chest pain, dyspnea, or peripheral edema.`,
    vitals: [
      { label: 'Blood Pressure', value: '132/84 mmHg' },
      { label: 'Heart Rate',     value: '76 bpm' },
      { label: 'Weight',         value: '184 lbs' },
      { label: 'BMI',            value: '27.4' },
      { label: 'Temperature',    value: '98.4°F' },
      { label: 'SpO₂',          value: '98%' },
    ],
    diagnoses: [
      { name: 'Type 2 Diabetes Mellitus — Suboptimal Control', code: 'E11.65' },
      { name: 'Essential Hypertension',                         code: 'I10' },
      { name: 'Obesity — BMI 27.0–27.9',                        code: 'Z68.27' },
    ],
    plan: [
      'Continue Metformin 1000mg twice daily — no dose change at this time',
      'Increase Lisinopril from 10mg to 20mg daily for improved BP control',
      'Repeat HbA1c in 3 months — target <7.0%',
      'Fasting lipid panel and CMP ordered for next visit',
      'Referral to certified diabetes educator for medical nutrition therapy',
      'Continue blood glucose home monitoring; log readings for next visit',
      'Follow-up appointment in 3 months or sooner if symptoms worsen',
    ],
    attachments: [
      { name: 'HbA1c Lab Report — Jul 2026.pdf', type: 'PDF', size: '142 KB' },
      { name: 'Medication Reconciliation List.pdf', type: 'PDF', size: '98 KB' },
      { name: 'Patient Education — Diabetes Diet.pdf', type: 'PDF', size: '2.1 MB' },
    ],
    related: [
      { title: 'Office Visit — Diabetes Follow-up (Apr 2026)', date: 'Apr 15, 2026' },
      { title: 'HbA1c Lab Result — Apr 2026',                  date: 'Apr 15, 2026' },
      { title: 'Prescription — Metformin 1000mg',              date: 'Jan 8, 2026'  },
      { title: 'Office Visit — Annual Physical (Jan 2026)',     date: 'Jan 8, 2026'  },
    ],
  };
}
