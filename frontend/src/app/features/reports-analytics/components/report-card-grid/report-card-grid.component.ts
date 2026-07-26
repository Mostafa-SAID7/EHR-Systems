import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface ReportItem {
  title: string;
  description: string;
  category: string;
  lastRun: string;
  iconClass: string;
  icon: string;
}

@Component({
  selector: 'app-report-card-grid',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './report-card-grid.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReportCardGridComponent {
  @Input() reports: ReportItem[] = [];
  @Output() generate = new EventEmitter<ReportItem>();
}
