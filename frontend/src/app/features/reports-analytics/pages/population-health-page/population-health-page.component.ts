import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-population-health-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './population-health-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PopulationHealthPageComponent implements OnInit {
  activeView = 'overview';

  views = [
    { key: 'overview', label: 'Overview' },
    { key: 'cohorts',  label: 'Cohorts' },
    { key: 'trends',   label: 'Trends' },
  ];

  keyMetrics = [
    { label: 'Total Population',     value: '1,248', change: '+18 new this month', positive: true,  icon: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z', iconClass: 'icon-box-primary' },
    { label: 'High-Risk Patients',   value: '84',    change: '-6 vs last quarter', positive: true,  icon: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z', iconClass: 'icon-box-red' },
    { label: 'Wellness Index',        value: '72.4',  change: '+1.8 pts this quarter', positive: true, icon: 'M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z', iconClass: 'icon-box-teal' },
    { label: 'Preventive Care Rate', value: '64.8%', change: '+3.2% vs last year', positive: true,  icon: 'M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z', iconClass: 'icon-box-amber' },
  ];

  diseases = [
    { name: 'Type 2 Diabetes',    count: 312, pct: 25.0, controlled: 68, screeningDue: 48, color: '#16a34a' },
    { name: 'Hypertension',       count: 448, pct: 35.9, controlled: 72, screeningDue: 61, color: '#0d9488' },
    { name: 'COPD / Asthma',      count: 186, pct: 14.9, controlled: 54, screeningDue: 34, color: '#d97706' },
    { name: 'Heart Disease',      count: 124, pct: 9.9,  controlled: 81, screeningDue: 22, color: '#dc2626' },
    { name: 'Hypothyroidism',     count: 98,  pct: 7.9,  controlled: 89, screeningDue: 11, color: '#7c3aed' },
    { name: 'Mental Health',      count: 178, pct: 14.3, controlled: 61, screeningDue: 52, color: '#2563eb' },
  ];

  riskTiers = [
    { tier: 'High Risk',    count: 84,  pct: 6.7,  description: 'Multiple chronic conditions, recent hospitalization, or poor medication adherence. Require intensive case management.', iconClass: 'icon-box-red',     icon: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z', valueClass: 'text-red-600 dark:text-red-400',     borderClass: 'border-red-100 dark:border-red-900/40 bg-red-50/40 dark:bg-red-950/20',     barClass: 'bg-red-500' },
    { tier: 'Moderate Risk',count: 248, pct: 19.9, description: 'Single chronic condition with suboptimal control, or two or more risk factors. Proactive monitoring recommended.', iconClass: 'icon-box-amber',   icon: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z', valueClass: 'text-amber-600 dark:text-amber-400', borderClass: 'border-amber-100 dark:border-amber-900/40 bg-amber-50/40 dark:bg-amber-950/20', barClass: 'bg-amber-500' },
    { tier: 'Low Risk',     count: 916, pct: 73.4, description: 'Well-controlled conditions or no significant chronic disease. Routine preventive care and annual wellness visits.', iconClass: 'icon-box-primary', icon: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z',                                                                                                                                                                                  valueClass: 'text-primary-600 dark:text-primary-400', borderClass: 'border-primary-100 dark:border-primary-900/40 bg-primary-50/40 dark:bg-primary-950/20', barClass: 'progress-fill' },
  ];

  careGaps = [
    { label: 'Diabetes Foot Exam',        description: 'Annual foot exam overdue for patients with T2DM > 2 years.',    count: 48, urgent: true,  icon: 'M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z' },
    { label: 'Colorectal Cancer Screening',description: 'Colonoscopy or FIT test due for patients 50–75 years.',       count: 62, urgent: true,  icon: 'M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z' },
    { label: 'Mammography',               description: 'Annual mammogram overdue for female patients aged 40+.',        count: 34, urgent: false, icon: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z' },
    { label: 'Influenza Vaccination',      description: 'Annual flu vaccine due for patients 65+ and high-risk groups.', count: 91, urgent: false, icon: 'M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z' },
    { label: 'Blood Pressure Check',       description: 'No BP reading in 12+ months for hypertension patients.',       count: 28, urgent: false, icon: 'M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z' },
    { label: 'Retinal Eye Exam',           description: 'Annual dilated eye exam due for diabetic patients.',            count: 56, urgent: true,  icon: 'M15 12a3 3 0 11-6 0 3 3 0 016 0z M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z' },
  ];

  ageGroups = [
    { range: '0 – 17',  count: 68,  pct: 5 },
    { range: '18 – 34', count: 174, pct: 14 },
    { range: '35 – 49', count: 286, pct: 23 },
    { range: '50 – 64', count: 374, pct: 30 },
    { range: '65 – 74', count: 224, pct: 18 },
    { range: '75 +',    count: 122, pct: 10 },
  ];

  genderBreakdown = [
    { label: 'Female', pct: 54, count: 674 },
    { label: 'Male',   pct: 44, count: 549 },
    { label: 'Other',  pct: 2,  count: 25 },
  ];

  insurance = [
    { label: 'Private / Commercial', pct: 48 },
    { label: 'Medicare',             pct: 26 },
    { label: 'Medicaid',             pct: 16 },
    { label: 'Self-Pay / Uninsured', pct: 7 },
    { label: 'Other Government',     pct: 3 },
  ];

  ngOnInit(): void {}
}
