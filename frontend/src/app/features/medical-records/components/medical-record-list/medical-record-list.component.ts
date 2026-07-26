import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

export interface MedicalRecord {
  id: string;
  patient: string;
  initials: string;
  type: string;
  category: string;
  date: Date;
  summary: string;
  provider: string;
  status: 'Final' | 'Draft' | 'Amended';
  color: string;
}

@Component({
  selector: 'app-medical-record-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './medical-record-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MedicalRecordListComponent {
  @Input() records: MedicalRecord[] = [];

  trackById(_: number, r: MedicalRecord): string { return r.id; }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      'Final':   'badge-success',
      'Draft':   'badge-warning',
      'Amended': 'badge-info',
    };
    return map[status] || 'badge-neutral';
  }

  getCategoryIcon(category: string): { box: string; path: string } {
    const map: Record<string, { box: string; path: string }> = {
      'Clinical Notes': { box: 'icon-box-primary', path: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01m-.01 4h.01' },
      'Lab Results':    { box: 'icon-box-teal',    path: 'M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z' },
      'Imaging':        { box: 'icon-box-blue',    path: 'M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z' },
      'Prescriptions':  { box: 'icon-box-purple',  path: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z' },
      'Procedures':     { box: 'icon-box-amber',   path: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4' },
    };
    return map[category] || map['Clinical Notes'];
  }
}
