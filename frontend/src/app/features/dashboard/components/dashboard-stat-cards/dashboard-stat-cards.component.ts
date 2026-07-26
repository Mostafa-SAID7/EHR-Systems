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
  templateUrl: './dashboard-stat-cards.component.html',
  host: {
    class: 'w-full block'
  },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardStatCardsComponent {
  @Input() stats: DashboardStat[] = [];
  trackByLabel(_: number, s: DashboardStat): string { return s.label; }
}
