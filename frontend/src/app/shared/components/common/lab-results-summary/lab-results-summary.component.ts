import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface LabResult {
  id: string;
  name: string;
  value: number;
  unit: string;
  normal: { min: number; max: number };
  status: 'normal' | 'abnormal' | 'critical';
  testDate: Date;
  previousValue?: number;
}

/**
 * Lab Results Summary — clean table, green/amber/red status, no left-border patterns
 */
@Component({
  selector: 'app-lab-results-summary',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="table-container">
      <table class="table-base">
        <thead>
          <tr>
            <th>Test Name</th>
            <th>Result</th>
            <th class="hidden sm:table-cell">Normal Range</th>
            <th>Status</th>
            <th class="hidden md:table-cell">Trend</th>
            <th class="hidden lg:table-cell">Date</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let r of results" class="animate-fade-in">
            <!-- Name -->
            <td>
              <span class="font-medium text-gray-900 dark:text-white">{{ r.name }}</span>
            </td>

            <!-- Value -->
            <td>
              <span [ngClass]="getValueClasses(r.status)"
                class="text-base font-bold tabular-nums">
                {{ r.value }}
                <span class="text-xs font-normal text-gray-400 ml-0.5">{{ r.unit }}</span>
              </span>
            </td>

            <!-- Range -->
            <td class="hidden sm:table-cell text-gray-500 dark:text-gray-400 tabular-nums">
              {{ r.normal.min }}–{{ r.normal.max }} {{ r.unit }}
            </td>

            <!-- Status badge -->
            <td>
              <span [ngClass]="getBadgeClass(r.status)" class="badge capitalize">
                {{ r.status }}
              </span>
            </td>

            <!-- Trend -->
            <td class="hidden md:table-cell">
              <div *ngIf="r.previousValue" class="flex items-center gap-1.5">
                <span [ngClass]="getTrendClasses(r)" class="text-sm font-bold">
                  {{ getTrendIcon(r) }}
                </span>
                <span [ngClass]="getTrendClasses(r)" class="text-xs font-medium tabular-nums">
                  {{ getTrendPercent(r) > 0 ? '+' : '' }}{{ getTrendPercent(r) }}%
                </span>
              </div>
              <span *ngIf="!r.previousValue" class="text-gray-300 dark:text-gray-600 text-xs">—</span>
            </td>

            <!-- Date -->
            <td class="hidden lg:table-cell text-gray-500 dark:text-gray-400 text-xs">
              {{ r.testDate | date:'MMM d, yyyy' }}
            </td>
          </tr>

          <tr *ngIf="results.length === 0">
            <td colspan="6" class="py-16">
              <div class="empty-state">
                <div class="empty-icon">🔬</div>
                <p class="empty-title">No lab results</p>
                <p class="empty-body">Lab results will appear here once recorded.</p>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabResultsSummaryComponent {
  @Input() results: LabResult[] = [];

  getValueClasses(status: string) {
    return {
      'text-primary-700 dark:text-primary-400': status === 'normal',
      'text-yellow-600  dark:text-yellow-400':  status === 'abnormal',
      'text-red-600     dark:text-red-400':     status === 'critical',
    };
  }

  getBadgeClass(status: string): string {
    return status === 'normal'   ? 'badge-success'
         : status === 'abnormal' ? 'badge-warning'
         :                         'badge-danger';
  }

  getTrendIcon(r: LabResult): string {
    if (!r.previousValue) return '';
    return r.value > r.previousValue ? '↑' : r.value < r.previousValue ? '↓' : '→';
  }

  getTrendPercent(r: LabResult): number {
    if (!r.previousValue || r.previousValue === 0) return 0;
    return Math.round(((r.value - r.previousValue) / r.previousValue) * 100);
  }

  getTrendClasses(r: LabResult) {
    const delta = r.value - (r.previousValue ?? r.value);
    return {
      'text-red-500     dark:text-red-400':     delta > 0,
      'text-primary-500 dark:text-primary-400': delta < 0,
      'text-gray-400':                          delta === 0,
    };
  }
}
