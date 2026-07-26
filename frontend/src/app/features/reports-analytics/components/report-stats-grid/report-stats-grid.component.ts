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
  templateUrl: './report-stats-grid.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReportStatsGridComponent {
  @Input() stats: ReportStat[] = [];
}
