import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { PatientProfileCardComponent } from '../../components/patient-profile-card/patient-profile-card.component';
import { PatientVitalsGridComponent } from '../../components/patient-vitals-grid/patient-vitals-grid.component';

@Component({
  selector: 'app-patient-detail-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    PatientProfileCardComponent,
    PatientVitalsGridComponent
  ],
  templateUrl: './patient-detail-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatientDetailPageComponent implements OnInit {
  activeTab = 'visits';
  tabs = [
    { key: 'visits', label: 'Visit History' },
    { key: 'rx',     label: 'Prescriptions' },
    { key: 'labs',   label: 'Lab Results' },
  ];

  patient = {
    id: '1',
    name: 'Sarah Johnson',
    initials: 'SJ',
    dob: 'March 12, 1985',
    age: 39,
    gender: 'Female',
    mrn: '00-1234',
    phone: '(555) 010-1234',
    status: 'Active',
    conditions: ['Type 2 Diabetes', 'Hypertension', 'Hyperlipidemia'],
    allergies: ['Penicillin', 'Sulfa drugs'],
  };

  demographics = [
    { label: 'Blood Type',    value: 'A+' },
    { label: 'Insurance',     value: 'BlueCross PPO' },
    { label: 'Primary Care',  value: 'Dr. Patel' },
    { label: 'Emergency Contact', value: 'John Johnson (Spouse)' },
    { label: 'Last Visit',    value: 'July 23, 2026' },
    { label: 'Next Appt.',    value: 'Aug 15, 2026' },
  ];

  vitals = [
    { label: 'Blood Pressure', value: '138/88', unit: 'mmHg', icon: 'M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z', iconClass: 'icon-box-red', alert: true },
    { label: 'Heart Rate',     value: '72',     unit: 'bpm',  icon: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z', iconClass: 'icon-box-primary', alert: false },
    { label: 'Temperature',   value: '98.6',   unit: '°F',   icon: 'M9 19V6l12-3v13M9 19c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zm12-3c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zM9 10l12-3', iconClass: 'icon-box-amber', alert: false },
    { label: 'BMI',           value: '27.4',   unit: 'kg/m²',icon: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z', iconClass: 'icon-box-teal', alert: false },
    { label: 'O₂ Saturation', value: '98',     unit: '%',    icon: 'M3 15a4 4 0 004 4h9a5 5 0 10-.1-9.999 5.002 5.002 0 10-9.78 2.096A4.001 4.001 0 003 15z', iconClass: 'icon-box-primary', alert: false },
    { label: 'HbA1c',         value: '7.2',    unit: '%',    icon: 'M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z', iconClass: 'icon-box-amber', alert: true },
  ];

  recentVisits = [
    { type: 'Annual Physical Exam', provider: 'Dr. Patel', date: 'July 23, 2026', status: 'Final', notes: 'BP slightly elevated at 138/88. Diabetes well-managed with HbA1c at 7.2%. Continued current medications. Follow-up in 3 months.' },
    { type: 'Diabetes Management Review', provider: 'Dr. Patel', date: 'April 15, 2026', status: 'Final', notes: 'Fasting glucose 128 mg/dL. Metformin dose increased to 1000mg BID. Dietary counseling provided. Labs ordered.' },
    { type: 'Hypertension Follow-up', provider: 'Dr. Patel', date: 'Jan 10, 2026', status: 'Final', notes: 'BP 142/90 on last visit, now 138/88 with lifestyle changes. Lisinopril 10mg continued. DASH diet reinforced.' },
  ];

  prescriptions = [
    { drug: 'Metformin 1000mg',        dosage: '1000mg',  frequency: 'Twice daily', active: true,  refills: 3 },
    { drug: 'Lisinopril 10mg',         dosage: '10mg',    frequency: 'Once daily',  active: true,  refills: 5 },
    { drug: 'Atorvastatin 40mg',       dosage: '40mg',    frequency: 'At bedtime',  active: true,  refills: 2 },
    { drug: 'Aspirin 81mg',            dosage: '81mg',    frequency: 'Once daily',  active: true,  refills: 6 },
    { drug: 'Glipizide 5mg',           dosage: '5mg',     frequency: 'Before meals',active: false, refills: 0 },
  ];

  labResults = [
    { test: 'HbA1c',            value: '7.2', unit: '%',    range: '4.0–5.6', status: 'High',   date: 'Jul 23, 2026' },
    { test: 'Fasting Glucose',  value: '128', unit: 'mg/dL',range: '70–100',  status: 'High',   date: 'Jul 23, 2026' },
    { test: 'LDL Cholesterol',  value: '94',  unit: 'mg/dL',range: '<100',    status: 'Normal', date: 'Jul 23, 2026' },
    { test: 'eGFR',             value: '78',  unit: 'mL/min',range: '>60',    status: 'Normal', date: 'Jul 23, 2026' },
  ];

  trackByKey(_: number, item: { key: string }): string { return item.key; }
  trackByVisit(_: number, v: any): string { return v.type + v.date; }
  trackByRx(_: number, rx: any): string { return rx.drug; }
  trackByLab(_: number, l: any): string { return l.test; }

  ngOnInit(): void {}
  constructor(private route: ActivatedRoute) {}
}
