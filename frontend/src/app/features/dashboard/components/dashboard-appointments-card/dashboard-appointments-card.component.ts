import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

export interface TodayAppointment {
  patient: string;
  type: string;
  time: string;
  urgent: boolean;
}

@Component({
  selector: 'app-dashboard-appointments-card',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard-appointments-card.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardAppointmentsCardComponent {
  @Input() appointments: TodayAppointment[] = [];
  trackByPatient(_: number, a: TodayAppointment): string { return a.patient + a.time; }
}
