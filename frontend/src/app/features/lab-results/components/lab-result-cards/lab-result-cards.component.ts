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
  templateUrl: './lab-result-cards.component.html',
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
