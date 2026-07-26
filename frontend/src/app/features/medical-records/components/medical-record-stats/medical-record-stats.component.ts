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
  templateUrl: './medical-record-stats.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MedicalRecordStatsComponent {
  @Input() stats: RecordStat[] = [];
}
