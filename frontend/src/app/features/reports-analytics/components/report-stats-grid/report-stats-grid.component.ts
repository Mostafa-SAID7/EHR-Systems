import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface ReportStat {
  label: string;
  value: string;
  change: string;
  positive: boolean;
  icon: string;
  iconClass: string;
}

@Component({
  selector: 'app-report-stats-grid',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="grid-stats">
      <div *ngFor="let s of stats; let i = index" class="stat-card animate-count-up" [style.animation-delay]="i * 70 + 'ms'">
        <div class="flex items-start justify-between gap-2">
          <div>
            <p class="stat-label">{{ s.label }}</p>
            <p class="stat-value mt-1.5">{{ s.value }}</p>
          </div>
          <div [ngClass]="s.iconClass" class="icon-box-lg shrink-0">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75" [attr.d]="s.icon"/>
            </svg>
          </div>
        </div>
        <div class="mt-3" [ngClass]="s.positive ? 'stat-change positive' : 'stat-change negative'">
          <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" [attr.d]="s.positive ? 'M5 15l7-7 7 7' : 'M19 9l-7 7-7-7'"/>
          </svg>
          <span>{{ s.change }}</span>
        </div>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReportStatsGridComponent {
  @Input() stats: ReportStat[] = [];
}
