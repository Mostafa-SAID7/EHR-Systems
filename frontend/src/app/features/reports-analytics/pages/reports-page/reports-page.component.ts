import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReportStatsGridComponent, ReportStat } from '../../components/report-stats-grid/report-stats-grid.component';
import { ReportCardGridComponent, ReportItem } from '../../components/report-card-grid/report-card-grid.component';

@Component({
  selector: 'app-reports-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReportStatsGridComponent,
    ReportCardGridComponent,
  ],
  templateUrl: './reports-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReportsPageComponent implements OnInit {
  activeCategory = 'All';
  generating = false;
  categories = ['All', 'Clinical', 'Financial', 'Operational', 'Compliance', 'Population Health'];

  stats: ReportStat[] = [
    { label: 'Reports Generated',  value: '1,284', change: '+14.2% this month', positive: true,  icon: 'M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z', iconClass: 'icon-box-primary' },
    { label: 'Scheduled Reports',   value: '24',    change: '3 due today',       positive: true,  icon: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z',  iconClass: 'icon-box-teal' },
    { label: 'Data Exports',        value: '312',   change: '+8.5% this month',  positive: true,  icon: 'M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4', iconClass: 'icon-box-amber' },
    { label: 'Active Dashboards',   value: '8',     change: '2 new this week',   positive: true,  icon: 'M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z', iconClass: 'icon-box-primary' },
  ];

  reports: ReportItem[] = [
    { title: 'Monthly Patient Volume',        description: 'Total patients seen, new registrations, and visit type breakdown by month.',       category: 'Operational',        lastRun: 'Jul 24, 2026',  iconClass: 'icon-box-primary', icon: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z' },
    { title: 'Revenue Cycle Analysis',        description: 'Claims submitted, collected, denied, and aged accounts receivable summary.',       category: 'Financial',          lastRun: 'Jul 23, 2026',  iconClass: 'icon-box-teal',    icon: 'M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z' },
    { title: 'Chronic Disease Registry',      description: 'Patient roster for diabetes, hypertension, and COPD with quality measures.',       category: 'Clinical',           lastRun: 'Jul 22, 2026',  iconClass: 'icon-box-amber',   icon: 'M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z' },
    { title: 'Prescription Analysis',         description: 'Top prescribed medications, refill rates, generic substitution, and drug costs.', category: 'Clinical',           lastRun: 'Jul 21, 2026',  iconClass: 'icon-box-teal',    icon: 'M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z' },
    { title: 'HIPAA Compliance Summary',      description: 'PHI access logs, breach incidents, training completion, and audit findings.',     category: 'Compliance',         lastRun: 'Jul 20, 2026',  iconClass: 'icon-box-red',     icon: 'M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z' },
    { title: 'Lab Turnaround Times',          description: 'Average time from lab order to result delivery, broken down by test category.',   category: 'Operational',        lastRun: 'Jul 19, 2026',  iconClass: 'icon-box-primary', icon: 'M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z' },
    { title: 'Population Health Outcomes',    description: 'Preventive care gaps, screenings due, and population wellness index by cohort.',  category: 'Population Health',  lastRun: 'Jul 18, 2026',  iconClass: 'icon-box-teal',    icon: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z' },
    { title: 'Physician Productivity Report', description: 'Visits per physician, RVUs, documentation time, and quality metric performance.', category: 'Operational',        lastRun: 'Jul 17, 2026',  iconClass: 'icon-box-amber',   icon: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z' },
    { title: 'Appointment No-Show Analysis',  description: 'No-show rates by provider, specialty, day of week, and patient demographics.',   category: 'Operational',        lastRun: 'Jul 16, 2026',  iconClass: 'icon-box-red',     icon: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z' },
  ];

  recentRuns = [
    { title: 'Monthly Patient Volume — July 2026',    user: 'Dr. Patel',    time: '10 mins ago', format: 'PDF',  iconClass: 'icon-box-primary', icon: 'M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z' },
    { title: 'Revenue Cycle Analysis — Q2 2026',      user: 'Admin',        time: '2 hrs ago',   format: 'XLSX', iconClass: 'icon-box-teal',    icon: 'M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z' },
    { title: 'HIPAA Compliance Summary — Jul 2026',   user: 'Sarah Admin',  time: '4 hrs ago',   format: 'PDF',  iconClass: 'icon-box-red',     icon: 'M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z' },
    { title: 'Prescription Analysis — July 2026',     user: 'Dr. Smith',    time: 'Yesterday',   format: 'CSV',  iconClass: 'icon-box-amber',   icon: 'M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z' },
  ];

  filteredReports() {
    if (this.activeCategory === 'All') return this.reports;
    return this.reports.filter(r => r.category === this.activeCategory);
  }

  generateReport(r: any): void {
    this.generating = true;
    setTimeout(() => { this.generating = false; }, 2500);
  }

  ngOnInit(): void {}
}
