import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface PatientDemographics {
  label: string;
  value: string;
}

export interface PatientProfile {
  name: string;
  initials: string;
  dob: string;
  phone: string;
  conditions: string[];
  allergies: string[];
}

@Component({
  selector: 'app-patient-profile-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './patient-profile-card.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatientProfileCardComponent {
  @Input() patient!: PatientProfile;
  @Input() demographics: PatientDemographics[] = [];
}
