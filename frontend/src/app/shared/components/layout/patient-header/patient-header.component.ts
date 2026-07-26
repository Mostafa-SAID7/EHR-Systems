import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Patient } from '../../../core/models';

/**
 * Patient Header — sticky cinematic header, no left-border patterns
 */
@Component({
  selector: 'app-patient-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './patient-header.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatientHeaderComponent {
  @Input() patient!: Patient;

  getInitials(): string {
    return [this.patient?.firstName, this.patient?.lastName]
      .filter(Boolean)
      .map(n => n[0].toUpperCase())
      .join('');
  }

  getAge(): string {
    if (!this.patient?.dateOfBirth) return '—';
    const dob = new Date(this.patient.dateOfBirth);
    const today = new Date();
    let age = today.getFullYear() - dob.getFullYear();
    if (today.getMonth() < dob.getMonth() ||
       (today.getMonth() === dob.getMonth() && today.getDate() < dob.getDate())) {
      age--;
    }
    return `${age}y`;
  }

  hasAlerts(): boolean {
    return !!(this.patient?.allergies?.length || this.patient?.chronicConditions?.length);
  }
}
