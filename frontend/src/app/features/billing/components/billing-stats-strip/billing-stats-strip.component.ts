import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface BillingStat {
  label: string;
  value: string;
  change: string;
  positive: boolean;
  icon: string;
  iconClass: string;
}

@Component({
  selector: 'app-billing-stats-strip',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './billing-stats-strip.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BillingStatsStripComponent {
  @Input() stats: BillingStat[] = [];
}
