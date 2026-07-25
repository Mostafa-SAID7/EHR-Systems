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
  template: `
    <div class="space-y-3">
      <div *ngFor="let r of records; trackBy: trackById" class="card-hover group flex gap-4">

        <!-- Category icon -->
        <div [ngClass]="getCategoryIcon(r.category).box" class="icon-box-lg shrink-0 self-start">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75"
              [attr.d]="getCategoryIcon(r.category).path"/>
          </svg>
        </div>

        <!-- Content -->
        <div class="flex-1 min-w-0">
          <div class="flex items-start justify-between gap-3 mb-2 flex-wrap">
            <div>
              <p class="text-sm font-semibold text-gray-900 dark:text-white">{{ r.type }}</p>
              <p class="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                {{ r.patient }} &middot; {{ r.provider }} &middot; {{ r.date | date:'MMM d, y' }}
              </p>
            </div>
            <div class="flex items-center gap-2 shrink-0">
              <span [ngClass]="getStatusClass(r.status)" class="badge">{{ r.status }}</span>
              <span class="badge-neutral text-2xs">{{ r.category }}</span>
            </div>
          </div>
          <p class="text-sm text-gray-600 dark:text-gray-400 leading-relaxed">{{ r.summary }}</p>
          <div class="flex items-center gap-3 mt-3">
            <button class="link-primary">
              <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/>
              </svg>
              View
            </button>
            <button class="text-xs font-medium text-gray-400 hover:text-gray-600 dark:hover:text-gray-200
                           transition-colors inline-flex items-center gap-1">
              <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12"/>
              </svg>
              Download
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
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
