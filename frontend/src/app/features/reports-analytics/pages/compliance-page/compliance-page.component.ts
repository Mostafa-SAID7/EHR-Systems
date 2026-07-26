import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-compliance-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './compliance-page.component.html',
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
