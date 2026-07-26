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
  templateUrl: './lab-results-summary.component.html',
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
