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
  template: `
    <div class="space-y-6 stagger">

      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="heading-xl">Audit Logs</h1>
          <p class="body-text mt-1">Comprehensive record of all system activity and access events</p>
        </div>
        <button class="btn-primary flex items-center gap-2">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/></svg>
          Export Logs
        </button>
      </div>

      <!-- Stats Row -->
      <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
        @for (stat of stats; track stat.label) {
          <div class="card p-4">
            <p class="text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wide">{{ stat.label }}</p>
            <p class="text-2xl font-bold mt-1" [class]="stat.color">{{ stat.value }}</p>
            <p class="text-xs text-gray-500 mt-1">{{ stat.sub }}</p>
          </div>
        }
      </div>

      <!-- Filters -->
      <div class="card p-4">
        <div class="flex flex-wrap gap-3 items-center">
          <div class="relative flex-1 min-w-[200px]">
            <svg class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0"/></svg>
            <input type="text" placeholder="Search logs…" (input)="search($event)"
              class="w-full pl-9 pr-4 py-2 text-sm bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-400">
          </div>
          <select (change)="filterSeverity($event)" class="text-sm bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-primary-400">
            <option value="">All Severities</option>
            <option value="critical">Critical</option>
            <option value="high">High</option>
            <option value="medium">Medium</option>
            <option value="low">Low</option>
          </select>
          <select (change)="filterStatus($event)" class="text-sm bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-primary-400">
            <option value="">All Statuses</option>
            <option value="Success">Success</option>
            <option value="Failed">Failed</option>
            <option value="Warning">Warning</option>
          </select>
          <div class="flex gap-2">
            @for (range of dateRanges; track range) {
              <button (click)="activeDateRange.set(range)"
                class="text-xs px-3 py-2 rounded-lg border transition-colors"
                [class]="activeDateRange() === range ? 'bg-primary-500 text-white border-primary-500' : 'border-gray-200 dark:border-gray-700 text-gray-600 dark:text-gray-400 hover:border-primary-300'">
                {{ range }}
              </button>
            }
          </div>
        </div>
      </div>

      <!-- Logs Table -->
      <div class="card overflow-hidden">
        <div class="px-6 py-4 border-b border-gray-100 dark:border-gray-800 flex items-center justify-between">
          <h2 class="font-semibold text-gray-900 dark:text-white">Activity Log</h2>
          <span class="text-sm text-gray-500">{{ filteredLogs().length }} entries</span>
        </div>
        <div class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr class="bg-gray-50 dark:bg-gray-800/50 text-left">
                <th class="px-6 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Timestamp</th>
                <th class="px-6 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">User</th>
                <th class="px-6 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Action</th>
                <th class="px-6 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Resource</th>
                <th class="px-6 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Severity</th>
                <th class="px-6 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Status</th>
                <th class="px-6 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">IP</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100 dark:divide-gray-800">
              @for (log of filteredLogs(); track log.id) {
                <tr class="hover:bg-gray-50 dark:hover:bg-gray-800/30 transition-colors cursor-pointer"
                    (click)="selectedLog.set(log)">
                  <td class="px-6 py-3 text-gray-500 whitespace-nowrap font-mono text-xs">
                    {{ log.timestamp | date:'MM/dd HH:mm:ss' }}
                  </td>
                  <td class="px-6 py-3">
                    <div class="font-medium text-gray-900 dark:text-white">{{ log.user }}</div>
                    <div class="text-xs text-gray-500">{{ log.role }}</div>
                  </td>
                  <td class="px-6 py-3 text-gray-700 dark:text-gray-300">{{ log.action }}</td>
                  <td class="px-6 py-3">
                    <span class="font-medium text-gray-700 dark:text-gray-300">{{ log.resource }}</span>
                    <span class="text-xs text-gray-400 ml-1">#{{ log.resourceId }}</span>
                  </td>
                  <td class="px-6 py-3">
                    <span class="px-2 py-0.5 rounded-full text-xs font-semibold"
                          [class]="severityClass(log.severity)">
                      {{ log.severity }}
                    </span>
                  </td>
                  <td class="px-6 py-3">
                    <span class="flex items-center gap-1.5">
                      <span class="w-1.5 h-1.5 rounded-full"
                            [class]="log.status === 'Success' ? 'bg-emerald-500' : log.status === 'Failed' ? 'bg-red-500' : 'bg-amber-500'"></span>
                      <span [class]="log.status === 'Success' ? 'text-emerald-600' : log.status === 'Failed' ? 'text-red-600' : 'text-amber-600'">
                        {{ log.status }}
                      </span>
                    </span>
                  </td>
                  <td class="px-6 py-3 font-mono text-xs text-gray-500">{{ log.ip }}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>

      <!-- Detail Panel -->
      @if (selectedLog()) {
        <div class="card p-6">
          <div class="flex items-start justify-between mb-4">
            <h3 class="font-semibold text-gray-900 dark:text-white">Event Details</h3>
            <button (click)="selectedLog.set(null)" class="btn-icon-sm">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
            </button>
          </div>
          <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 text-sm">
            <div><span class="text-gray-500">Event ID:</span> <span class="font-mono ml-2">{{ selectedLog()!.id }}</span></div>
            <div><span class="text-gray-500">User:</span> <span class="font-medium ml-2">{{ selectedLog()!.user }}</span></div>
            <div><span class="text-gray-500">Role:</span> <span class="ml-2">{{ selectedLog()!.role }}</span></div>
            <div><span class="text-gray-500">Action:</span> <span class="ml-2">{{ selectedLog()!.action }}</span></div>
            <div><span class="text-gray-500">Resource:</span> <span class="ml-2">{{ selectedLog()!.resource }} #{{ selectedLog()!.resourceId }}</span></div>
            <div><span class="text-gray-500">IP Address:</span> <span class="font-mono ml-2">{{ selectedLog()!.ip }}</span></div>
            <div class="sm:col-span-2 lg:col-span-3"><span class="text-gray-500">Details:</span> <span class="ml-2">{{ selectedLog()!.details }}</span></div>
          </div>
        </div>
      }
    </div>
  `,
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
