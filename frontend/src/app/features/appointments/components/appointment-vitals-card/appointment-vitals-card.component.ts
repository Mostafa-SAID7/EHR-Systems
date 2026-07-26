import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface VitalsData {
  label: string;
  value: string;
  unit: string;
}

@Component({
  selector: 'app-appointment-vitals-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './appointment-vitals-card.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentVitalsCardComponent {
  @Input() vitals: VitalsData[] = [];
  @Input() show = true;
}
