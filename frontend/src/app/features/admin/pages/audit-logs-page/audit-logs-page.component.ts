import { Component, ChangeDetectionStrategy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

interface AuditLog {
  id: string;
  timestamp: Date;
  user: string;
  role: string;
  action: string;
  resource: string;
  resourceId: string;
  status: 'Success' | 'Failed' | 'Warning';
  ip: string;
  details: string;
  severity: 'low' | 'medium' | 'high' | 'critical';
}

@Component({
  selector: 'app-audit-logs-page',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './audit-logs-page.component.html',
})
export class AuditLogsPageComponent {
  dateRanges = ['Today', '7 Days', '30 Days'];
  activeDateRange = signal('Today');
  selectedLog = signal<AuditLog | null>(null);
  searchTerm = signal('');
  severityFilter = signal('');
  statusFilter = signal('');

  stats = [
    { label: 'Total Events', value: '24,891', sub: 'Last 30 days', color: 'text-gray-900 dark:text-white' },
    { label: 'Failed Logins', value: '47', sub: 'Last 24 hours', color: 'text-red-600' },
    { label: 'Critical Events', value: '3', sub: 'Requires review', color: 'text-red-600' },
    { label: 'Active Users', value: '142', sub: 'Currently online', color: 'text-emerald-600' },
  ];

  logs: AuditLog[] = [
    { id: 'EVT-001', timestamp: new Date(Date.now()-2*60000), user: 'Dr. Sarah Johnson', role: 'Physician', action: 'Viewed Patient Record', resource: 'Patient', resourceId: 'P-4821', status: 'Success', ip: '192.168.1.42', details: 'Accessed patient demographics and medical history for routine review.', severity: 'low' },
    { id: 'EVT-002', timestamp: new Date(Date.now()-8*60000), user: 'Admin User', role: 'Administrator', action: 'Modified User Permissions', resource: 'User', resourceId: 'U-0093', status: 'Success', ip: '10.0.0.5', details: 'Elevated user role from Nurse to Head Nurse for Emily Carter.', severity: 'high' },
    { id: 'EVT-003', timestamp: new Date(Date.now()-15*60000), user: 'Unknown', role: '—', action: 'Failed Login Attempt', resource: 'Auth', resourceId: 'N/A', status: 'Failed', ip: '203.0.113.74', details: 'Multiple failed login attempts from external IP. Account temporarily locked.', severity: 'critical' },
    { id: 'EVT-004', timestamp: new Date(Date.now()-22*60000), user: 'Nurse Williams', role: 'Registered Nurse', action: 'Updated Vitals', resource: 'Clinical Record', resourceId: 'CR-2291', status: 'Success', ip: '192.168.1.55', details: 'Blood pressure, heart rate, and SpO2 readings updated for patient visit.', severity: 'low' },
    { id: 'EVT-005', timestamp: new Date(Date.now()-34*60000), user: 'Dr. James Chen', role: 'Cardiologist', action: 'Prescribed Medication', resource: 'Prescription', resourceId: 'RX-8847', status: 'Success', ip: '192.168.1.38', details: 'New prescription issued: Metoprolol 25mg twice daily for cardiac arrhythmia.', severity: 'medium' },
    { id: 'EVT-006', timestamp: new Date(Date.now()-41*60000), user: 'Billing Clerk', role: 'Billing Staff', action: 'Exported Invoice Data', resource: 'Billing', resourceId: 'RPT-112', status: 'Warning', ip: '192.168.2.10', details: 'Large data export of 3,200 invoice records flagged for review.', severity: 'high' },
    { id: 'EVT-007', timestamp: new Date(Date.now()-55*60000), user: 'Dr. Sarah Johnson', role: 'Physician', action: 'Added Clinical Note', resource: 'Clinical Record', resourceId: 'CR-2289', status: 'Success', ip: '192.168.1.42', details: 'SOAP note added for follow-up appointment regarding hypertension management.', severity: 'low' },
    { id: 'EVT-008', timestamp: new Date(Date.now()-72*60000), user: 'System', role: 'Automated', action: 'Backup Completed', resource: 'System', resourceId: 'SYS-BAK', status: 'Success', ip: '127.0.0.1', details: 'Nightly database backup completed successfully. 14.2 GB archived.', severity: 'low' },
  ];

  filteredLogs = computed(() => {
    let result = this.logs;
    const term = this.searchTerm().toLowerCase();
    if (term) result = result.filter(l => l.user.toLowerCase().includes(term) || l.action.toLowerCase().includes(term) || l.resource.toLowerCase().includes(term));
    if (this.severityFilter()) result = result.filter(l => l.severity === this.severityFilter());
    if (this.statusFilter()) result = result.filter(l => l.status === this.statusFilter());
    return result;
  });

  search(e: Event) { this.searchTerm.set((e.target as HTMLInputElement).value); }
  filterSeverity(e: Event) { this.severityFilter.set((e.target as HTMLSelectElement).value); }
  filterStatus(e: Event) { this.statusFilter.set((e.target as HTMLSelectElement).value); }

  severityClass(s: string) {
    return {
      critical: 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400',
      high:     'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400',
      medium:   'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400',
      low:      'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400',
    }[s] ?? '';
  }
}
