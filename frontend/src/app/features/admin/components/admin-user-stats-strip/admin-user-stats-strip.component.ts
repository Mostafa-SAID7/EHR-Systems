import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface AdminStat {
  label: string;
  value: string;
  icon: string;
  iconClass: string;
}

@Component({
  selector: 'app-admin-user-stats-strip',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-user-stats-strip.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminUserStatsStripComponent {
  @Input() stats: AdminStat[] = [];
}
