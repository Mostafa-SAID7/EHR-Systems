import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

export interface PatientCard {
  id: string;
  name: string;
  initials: string;
  age: number;
  gender: string;
  mrn: string;
  lastVisit: Date;
  status: 'Active' | 'Inactive' | 'Critical';
  conditions: string[];
  color: string;
}

@Component({
  selector: 'app-patient-cards-grid',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './patient-cards-grid.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatientCardsGridComponent {
  @Input() patients: PatientCard[] = [];

  trackById(_: number, p: PatientCard): string { return p.id; }

  getStatusClass(status: string): string {
    return status === 'Active' ? 'badge-success' :
           status === 'Critical' ? 'badge-danger' : 'badge-neutral';
  }
}
