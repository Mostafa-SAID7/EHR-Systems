import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-compliance-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="space-y-6 stagger">

      <!-- ── Header ───────────────────────────────────── -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="heading-xl">Compliance &amp; Audit</h1>
          <p class="body-text mt-1">HIPAA compliance tracking, audit trail, and regulatory reporting</p>
        </div>
        <div class="flex items-center gap-2 shrink-0">
          <button class="btn-secondary btn-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/>
            </svg>
            Export Report
          </button>
          <button class="btn-primary btn-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"/>
            </svg>
            Run Compliance Check
          </button>
        </div>
      </div>

      <!-- ── Compliance Score Banner ──────────────────── -->
      <div class="card bg-gradient-to-br from-primary-600 to-primary-800 dark:from-primary-700 dark:to-primary-900 text-white relative overflow-hidden">
        <div class="absolute right-0 top-0 bottom-0 w-64 bg-white/5 rounded-l-full"></div>
        <div class="relative z-10 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-6">
          <div>
            <p class="text-primary-100 text-sm font-semibold uppercase tracking-wider mb-1">Overall Compliance Score</p>
            <div class="flex items-end gap-3">
              <span class="text-6xl font-extrabold tracking-tight">96.4</span>
              <span class="text-2xl font-bold text-primary-200 mb-1">/ 100</span>
            </div>
            <p class="text-primary-100 text-sm mt-2">Last assessed: July 24, 2026 &bull; Next scheduled: Aug 24, 2026</p>
          </div>
          <div class="flex flex-wrap gap-3">
            <div *ngFor="let c of certifications" class="px-4 py-3 rounded-xl bg-white/15 backdrop-blur-sm text-center border border-white/20">
              <p class="text-sm font-bold text-white">{{ c.name }}</p>
              <p class="text-xs text-primary-200">{{ c.status }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- ── Compliance area stats ─────────────────────── -->
      <div class="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <div *ngFor="let area of complianceAreas" class="card-hover">
          <div class="flex items-center justify-between mb-3">
            <div [ngClass]="area.iconClass" class="icon-box-md shrink-0">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75" [attr.d]="area.icon"/>
              </svg>
            </div>
            <span class="text-xl font-extrabold" [ngClass]="area.score >= 95 ? 'text-primary-600 dark:text-primary-400' : area.score >= 80 ? 'text-amber-600 dark:text-amber-400' : 'text-red-600 dark:text-red-400'">
              {{ area.score }}%
            </span>
          </div>
          <p class="text-sm font-semibold text-gray-900 dark:text-white mb-1">{{ area.label }}</p>
          <div class="progress-bar">
            <div class="progress-fill" [style.width.%]="area.score"
              [ngClass]="area.score >= 95 ? '' : area.score >= 80 ? 'bg-amber-500' : 'bg-red-500'"></div>
          </div>
          <p class="text-2xs text-gray-400 mt-1.5">{{ area.detail }}</p>
        </div>
      </div>

      <!-- ── Open findings ────────────────────────────── -->
      <div class="card p-0 overflow-hidden">
        <div class="card-header">
          <h2 class="heading-sm">Open Compliance Findings</h2>
          <span class="badge-warning">{{ openFindingsCount() }} open</span>
        </div>
        <div class="divide-y divide-surface-100 dark:divide-surface-700/50">
          <div *ngFor="let f of findings" class="flex items-start gap-4 px-5 py-4 hover:bg-primary-50/30 dark:hover:bg-primary-900/10 transition-colors">
            <div [ngClass]="f.severity === 'High' ? 'icon-box-sm icon-box-red' : f.severity === 'Medium' ? 'icon-box-sm icon-box-amber' : 'icon-box-sm icon-box-primary'" class="shrink-0 mt-0.5">
              <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"/>
              </svg>
            </div>
            <div class="flex-1 min-w-0">
              <div class="flex items-start justify-between gap-3 flex-wrap mb-1">
                <p class="text-sm font-semibold text-gray-900 dark:text-white">{{ f.title }}</p>
                <div class="flex items-center gap-2 shrink-0">
                  <span [ngClass]="f.severity === 'High' ? 'badge-danger' : f.severity === 'Medium' ? 'badge-warning' : 'badge-info'" class="badge text-2xs">{{ f.severity }}</span>
                  <span [ngClass]="f.open ? 'badge-warning' : 'badge-success'" class="badge text-2xs">{{ f.open ? 'Open' : 'Resolved' }}</span>
                </div>
              </div>
              <p class="text-xs text-gray-500 dark:text-gray-400">{{ f.description }}</p>
              <div class="flex items-center gap-4 mt-2">
                <span class="text-2xs text-gray-400">Area: {{ f.area }}</span>
                <span class="text-2xs text-gray-400">Due: {{ f.due }}</span>
                <span class="text-2xs text-gray-400">Owner: {{ f.owner }}</span>
              </div>
            </div>
            <button *ngIf="f.open" (click)="resolveFinding(f)" class="btn-secondary btn-sm shrink-0">Resolve</button>
          </div>
        </div>
      </div>

      <!-- ── PHI Access Log ─────────────────────────────── -->
      <div class="card p-0 overflow-hidden">
        <div class="card-header">
          <h2 class="heading-sm">Recent PHI Access Log</h2>
          <a routerLink="/admin/audit" class="link-primary text-xs">View full audit →</a>
        </div>
        <div class="divide-y divide-surface-100 dark:divide-surface-700/50">
          <div *ngFor="let log of phiLogs" class="flex items-center gap-4 px-5 py-3.5 hover:bg-primary-50/30 dark:hover:bg-primary-900/10 transition-colors">
            <div [ngClass]="phiIconClass(log.type)" class="shrink-0">
              <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75" [attr.d]="phiIconPath(log.type)"/>
              </svg>
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium text-gray-900 dark:text-white truncate">{{ log.action }}</p>
              <p class="text-2xs text-gray-400">{{ log.user }} &middot; Patient: {{ log.patient }}</p>
            </div>
            <div class="text-right shrink-0">
              <p class="text-xs font-mono text-gray-500">{{ log.time }}</p>
              <span [ngClass]="phiBadgeClass(log.type)" class="badge text-2xs">{{ log.type }}</span>
            </div>
          </div>
        </div>
      </div>

    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CompliancePageComponent implements OnInit {
  certifications = [
    { name: 'HIPAA',   status: 'Compliant' },
    { name: 'SOC 2',   status: 'Type II ✓' },
    { name: 'FHIR R4', status: 'Active' },
    { name: 'ISO 27001',status: 'Certified' },
  ];

  complianceAreas = [
    { label: 'PHI Access Controls',  score: 98, detail: '2 exceptions this month',   iconClass: 'icon-box-primary', icon: 'M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z' },
    { label: 'Staff Training',       score: 91, detail: '4 staff pending training',  iconClass: 'icon-box-teal',    icon: 'M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253' },
    { label: 'Data Encryption',      score: 100,detail: 'AES-256 & TLS 1.3',        iconClass: 'icon-box-primary', icon: 'M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z' },
    { label: 'Breach Management',    score: 94, detail: '1 incident under review',   iconClass: 'icon-box-amber',   icon: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z' },
  ];

  findings = [
    { title: 'Workforce Training Gap — 4 Staff Overdue',       area: 'Staff Training',        severity: 'Medium', open: true,  due: 'Aug 5, 2026',  owner: 'HR Manager',   description: 'Four clinical staff members have not completed the mandatory annual HIPAA awareness training.' },
    { title: 'PHI Disclosure Log — 1 Unauthorized Access',     area: 'Access Controls',       severity: 'High',   open: true,  due: 'Jul 30, 2026', owner: 'Compliance Officer', description: 'An after-hours PHI record access without documented clinical justification was detected in audit log.' },
    { title: 'Business Associate Agreement — Vendor Expired',  area: 'BAA Management',        severity: 'Medium', open: true,  due: 'Aug 10, 2026', owner: 'Legal Team',   description: 'BAA with third-party lab integration vendor LabConnect expired on June 30. Renewal in progress.' },
    { title: 'Patch Management — 3 Non-Critical Updates',      area: 'System Security',       severity: 'Low',    open: false, due: 'Jul 15, 2026', owner: 'IT Admin',     description: 'Three non-critical OS security patches were pending. Applied during scheduled maintenance window.' },
  ];

  phiLogs = [
    { action: 'Viewed patient record',        user: 'Dr. Patel',   patient: 'Sarah Johnson (MRN 00-1234)',  time: '10:32 AM', type: 'Authorized' },
    { action: 'Downloaded lab results PDF',   user: 'Dr. Smith',   patient: 'Robert Davis (MRN 00-4567)',   time: '10:18 AM', type: 'Authorized' },
    { action: 'Accessed record after-hours',  user: 'User-4891',   patient: 'Emma Williams (MRN 00-3456)',  time: '02:14 AM', type: 'Flagged' },
    { action: 'Updated prescription details', user: 'Dr. Garcia',  patient: 'Linda Martinez (MRN 00-5678)', time: 'Jul 24',   type: 'Authorized' },
    { action: 'Exported patient dataset',     user: 'Admin',       patient: 'Bulk export — 48 records',    time: 'Jul 23',   type: 'Authorized' },
  ];

  openFindingsCount(): number {
    return this.findings.filter(f => f.open).length;
  }

  resolveFinding(f: any): void {
    f.open = false;
  }

  phiIconClass(type: string): string {
    return type === 'Authorized' ? 'icon-box-sm icon-box-primary' : 'icon-box-sm icon-box-red';
  }

  phiIconPath(type: string): string {
    return type === 'Authorized'
      ? 'M15 12a3 3 0 11-6 0 3 3 0 016 0zM2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z'
      : 'M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636';
  }

  phiBadgeClass(type: string): string {
    return type === 'Authorized' ? 'badge-success' : 'badge-danger';
  }

  ngOnInit(): void {}
}
