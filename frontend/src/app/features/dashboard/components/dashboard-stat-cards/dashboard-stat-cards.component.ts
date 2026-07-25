import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

export interface DashboardStat {
  label: string;
  value: string;
  iconPath: string;
  iconBoxClass: string;
  change: string;
  changePositive: boolean;
}

@Component({
  selector: 'app-dashboard-stat-cards',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="grid-stats">
      <div *ngFor="let stat of stats; let i = index; trackBy: trackByLabel"
        class="stat-card animate-count-up"
        [style.animation-delay]="i * 70 + 'ms'">
        <div class="flex items-start justify-between gap-2">
          <div class="min-w-0">
            <p class="stat-label">{{ stat.label }}</p>
            <p class="stat-value mt-1.5">{{ stat.value }}</p>
          </div>
          <div [ngClass]="stat.iconBoxClass" class="icon-box-lg shrink-0">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75" [attr.d]="stat.iconPath"/>
            </svg>
          </div>
        </div>
        <div class="mt-3" [ngClass]="stat.changePositive ? 'stat-change positive' : 'stat-change negative'">
          <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5"
              [attr.d]="stat.changePositive ? 'M5 15l7-7 7 7' : 'M19 9l-7 7-7-7'"/>
          </svg>
          <span>{{ stat.change }} vs last week</span>
        </div>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardStatCardsComponent {
  @Input() stats: DashboardStat[] = [];
  trackByLabel(_: number, s: DashboardStat): string { return s.label; }
}
