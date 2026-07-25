import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface RecordStat {
  value: string;
  label: string;
  icon: string;
  iconClass: string;
}

@Component({
  selector: 'app-medical-record-stats',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
      <div *ngFor="let s of stats" class="card flex items-center gap-3 p-3.5">
        <div [ngClass]="s.iconClass" class="icon-box-md shrink-0">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75" [attr.d]="s.icon"/>
          </svg>
        </div>
        <div>
          <p class="text-base font-bold text-gray-900 dark:text-white tabular-nums">{{ s.value }}</p>
          <p class="text-2xs text-gray-500 dark:text-gray-400 font-medium">{{ s.label }}</p>
        </div>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MedicalRecordStatsComponent {
  @Input() stats: RecordStat[] = [];
}
