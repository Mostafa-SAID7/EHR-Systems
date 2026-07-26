import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface PrescriptionStat {
  label: string;
  value: string;
  icon: string;
  iconClass: string;
}

@Component({
  selector: 'app-prescription-stats-strip',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './prescription-stats-strip.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PrescriptionStatsStripComponent {
  @Input() stats: PrescriptionStat[] = [];
}
