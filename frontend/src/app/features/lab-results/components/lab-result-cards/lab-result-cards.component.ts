import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface LabResult {
  id: string;
  patient: string;
  initials: string;
  test: string;
  category: string;
  date: Date;
  value: string;
  unit: string;
  range: string;
  status: 'Normal' | 'High' | 'Low' | 'Critical' | 'Pending';
  color: string;
}

@Component({
  selector: 'app-lab-result-cards',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
      <div *ngFor="let r of results; trackBy: trackById" class="card-hover group">

        <!-- Patient header -->
        <div class="flex items-start justify-between gap-3 mb-3">
          <div class="flex items-center gap-2.5">
            <div class="w-9 h-9 rounded-xl shrink-0 flex items-center justify-center
                        text-white text-xs font-bold shadow-sm"
              [style.background]="r.color">
              {{ r.initials }}
            </div>
            <div class="min-w-0">
              <p class="text-sm font-semibold text-gray-900 dark:text-white truncate">{{ r.patient }}</p>
              <p class="text-2xs text-gray-500 dark:text-gray-400">{{ r.date | date:'MMM d, y' }}</p>
            </div>
          </div>
          <span [ngClass]="getResultStatusClass(r.status)" class="badge shrink-0">{{ r.status }}</span>
        </div>

        <!-- Value panel -->
        <div class="card-green p-3 mb-3">
          <p class="text-xs font-semibold text-gray-700 dark:text-gray-300 mb-2">{{ r.test }}</p>
          <div class="flex items-end justify-between">
            <div>
              <span [ngClass]="getValueClass(r.status)" class="text-2xl font-bold tabular-nums">{{ r.value }}</span>
              <span class="text-xs text-gray-500 dark:text-gray-400 ml-1.5">{{ r.unit }}</span>
            </div>
            <span class="text-xs text-gray-400 dark:text-gray-500">Ref: {{ r.range }}</span>
          </div>
        </div>

        <!-- Footer -->
        <div class="flex items-center justify-between">
          <span class="badge-neutral text-2xs">{{ r.category }}</span>
          <button class="text-xs font-semibold text-primary-600 hover:text-primary-700
                         dark:text-primary-400 transition-colors flex items-center gap-0.5">
            Details
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
            </svg>
          </button>
        </div>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabResultCardsComponent {
  @Input() results: LabResult[] = [];

  trackById(_: number, r: LabResult): string { return r.id; }

  getResultStatusClass(status: string): string {
    const map: Record<string, string> = {
      'Normal':   'badge-success',
      'High':     'badge-danger',
      'Low':      'badge-info',
      'Critical': 'badge-danger',
      'Pending':  'badge-warning',
    };
    return map[status] || 'badge-neutral';
  }

  getValueClass(status: string): string {
    if (status === 'Critical' || status === 'High') return 'text-red-600 dark:text-red-400';
    if (status === 'Low') return 'text-blue-600 dark:text-blue-400';
    return 'text-primary-700 dark:text-primary-300';
  }
}
