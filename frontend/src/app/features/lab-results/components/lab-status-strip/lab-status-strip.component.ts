import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface LabStatusStat {
  label: string;
  count: number;
  icon: string;
  iconClass: string;
}

@Component({
  selector: 'app-lab-status-strip',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './lab-status-strip.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabStatusStripComponent {
  @Input() stats: LabStatusStat[] = [];
}
