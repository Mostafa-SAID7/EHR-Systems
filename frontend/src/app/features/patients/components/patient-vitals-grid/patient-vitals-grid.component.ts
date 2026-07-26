import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface PatientVitals {
  label: string;
  value: string;
  unit: string;
  icon: string;
  iconClass: string;
  alert: boolean;
}

@Component({
  selector: 'app-patient-vitals-grid',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './patient-vitals-grid.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatientVitalsGridComponent {
  @Input() vitals: PatientVitals[] = [];
}
