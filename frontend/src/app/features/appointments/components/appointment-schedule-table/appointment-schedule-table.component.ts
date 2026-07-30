import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

export interface AppointmentRow {
  id: string;
  patient: string;
  initials: string;
  type: string;
  doctor: string;
  date: Date;
  duration: number;
  status: 'Scheduled' | 'Confirmed' | 'In Progress' | 'Completed' | 'Cancelled' | 'No Show' | 'Rescheduled';
  room?: string;
  color: string;
}

@Component({
  selector: 'app-appointment-schedule-table',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './appointment-schedule-table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentScheduleTableComponent {
  @Input() appointments: AppointmentRow[] = [];

  trackById(_: number, a: AppointmentRow): string { return a.id; }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      'Scheduled':   'badge-info',
      'Confirmed':   'badge-primary',
      'In Progress': 'badge-warning',
      'Completed':   'badge-success',
      'Cancelled':   'badge-danger',
      'No Show':     'badge-neutral',
      'Rescheduled': 'badge-info',
    };
    return map[status] || 'badge-neutral';
  }
}
