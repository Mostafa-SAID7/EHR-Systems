import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-lab-result-detail-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './lab-result-detail-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabResultDetailPageComponent {
  resultPct = 62; // position on the range bar

  result = {
    id: 'LAB-2026-0312',
    test: 'Hemoglobin A1c (HbA1c)',
    category: 'Endocrinology',
    date: new Date(2026, 6, 23),
    value: '7.2',
    unit: '%',
    range: '4.0 – 5.6',
    status: 'High',
    method: 'Ion-exchange HPLC',
    specimen: 'Whole Blood',
    interpretation: 'HbA1c of 7.2% indicates suboptimal glycemic control. Values above 5.6% suggest pre-diabetes; above 6.5% confirms diagnosis of diabetes mellitus. Current level indicates average blood glucose of approximately 160 mg/dL over the past 2–3 months.',
    actions: [
      'Review and adjust diabetes medications (Metformin dosage)',
      'Patient education on diet and lifestyle modifications',
      'Schedule follow-up in 3 months for repeat HbA1c',
      'Refer to diabetes educator if further support needed',
    ],
    patient: 'Sarah Johnson',
    patientInitials: 'SJ',
    mrn: '00-1234',
    dob: 'Mar 12, 1985',
    physician: 'Dr. Ramesh Patel',
    physicianInitials: 'RP',
    physicianSpecialty: 'Internal Medicine',
    lab: 'Sunrise Clinical Labs',
    collected: 'Jul 23, 2026 — 8:14 AM',
    resulted: 'Jul 23, 2026 — 2:47 PM',
    accession: 'SCL-2026-7412',
    history: [
      { date: 'Jan 2026', value: '7.8', unit: '%', status: 'High',   pct: 80 },
      { date: 'Oct 2025', value: '7.5', unit: '%', status: 'High',   pct: 74 },
      { date: 'Jul 2025', value: '7.2', unit: '%', status: 'High',   pct: 62 },
      { date: 'Apr 2025', value: '6.9', unit: '%', status: 'High',   pct: 55 },
      { date: 'Jan 2025', value: '6.4', unit: '%', status: 'High',   pct: 45 },
      { date: 'Oct 2024', value: '5.9', unit: '%', status: 'Normal', pct: 32 },
    ],
  };
}
