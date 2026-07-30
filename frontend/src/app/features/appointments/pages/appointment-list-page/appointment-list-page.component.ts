import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AppointmentScheduleTableComponent, AppointmentRow } from '../../components/appointment-schedule-table/appointment-schedule-table.component';
import * as AppointmentActions from '../../store/appointment.actions';
import { selectAppointments, selectLoading, selectError, selectAppointmentStats } from '../../store/appointment.selectors';
import { AppointmentResponseDto, AppointmentStatus } from '../../models/appointment.model';

@Component({
  selector: 'app-appointment-list-page',
  standalone: true,
  imports: [CommonModule, RouterModule, AppointmentScheduleTableComponent],
  templateUrl: './appointment-list-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentListPageComponent implements OnInit {
  activeView = 'day';
  todayLabel = new Date().toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric' });

  views = [
    { key: 'day',   label: 'Day' },
    { key: 'week',  label: 'Week' },
    { key: 'month', label: 'Month' },
  ];

  appointments$: Observable<AppointmentResponseDto[]>;
  rows$: Observable<AppointmentRow[]>;
  stats$: Observable<any>;
  loading$: Observable<boolean>;
  error$: Observable<string | null>;

  statusSummary = [
    { label: 'Scheduled',   status: AppointmentStatus.Scheduled,   dotClass: 'bg-blue-500' },
    { label: 'Confirmed',   status: AppointmentStatus.Confirmed,   dotClass: 'bg-primary-500' },
    { label: 'In Progress', status: AppointmentStatus.InProgress,  dotClass: 'bg-primary-500 animate-pulse-soft' },
    { label: 'Completed',   status: AppointmentStatus.Completed,   dotClass: 'bg-green-500' },
    { label: 'Cancelled',   status: AppointmentStatus.Cancelled,   dotClass: 'bg-red-500' },
    { label: 'No Show',     status: AppointmentStatus.NoShow,      dotClass: 'bg-gray-400' },
  ];

  currentPatientId = 'current-patient-id'; // In real app, get from auth service

  constructor(
    private store: Store,
    private route: ActivatedRoute
  ) {
    this.appointments$ = this.store.select(selectAppointments);
    this.rows$ = this.appointments$.pipe(map(apts => apts.map(a => this.appointmentToRow(a))));
    this.stats$ = this.store.select(selectAppointmentStats);
    this.loading$ = this.store.select(selectLoading);
    this.error$ = this.store.select(selectError);
  }

  ngOnInit(): void {
    this.store.dispatch(
      AppointmentActions.loadAppointments({
        patientId: this.currentPatientId,
        filter: { pageNumber: 1, pageSize: 20 }
      })
    );
  }

  getStatusCount(status: AppointmentStatus, stats: any): number {
    const statusMap: Record<AppointmentStatus, string> = {
      [AppointmentStatus.Scheduled]:   'scheduled',
      [AppointmentStatus.Confirmed]:   'confirmed',
      [AppointmentStatus.InProgress]:  'inProgress',
      [AppointmentStatus.Completed]:   'completed',
      [AppointmentStatus.Cancelled]:   'cancelled',
      [AppointmentStatus.NoShow]:      'noShow',
      [AppointmentStatus.Rescheduled]: 'rescheduled',
    };
    return stats?.[statusMap[status]] || 0;
  }

  appointmentToRow(apt: AppointmentResponseDto): AppointmentRow {
    return {
      id: apt.id,
      patient: 'Patient Name',
      initials: 'PN',
      type: apt.appointmentType,
      doctor: 'Dr. Name',
      date: new Date(apt.scheduledStart),
      duration: apt.durationMinutes || 30,
      status: this.mapStatusToDisplayString(apt.status),
      room: 'TBD',
      color: this.getStatusColor(apt.status)
    };
  }

  private mapStatusToDisplayString(status: AppointmentStatus): AppointmentRow['status'] {
    const map: Record<AppointmentStatus, AppointmentRow['status']> = {
      [AppointmentStatus.Scheduled]:   'Scheduled',
      [AppointmentStatus.Confirmed]:   'Confirmed',
      [AppointmentStatus.InProgress]:  'In Progress',
      [AppointmentStatus.Completed]:   'Completed',
      [AppointmentStatus.Cancelled]:   'Cancelled',
      [AppointmentStatus.NoShow]:      'No Show',
      [AppointmentStatus.Rescheduled]: 'Rescheduled',
    };
    return map[status] ?? 'Scheduled';
  }

  private getStatusColor(status: AppointmentStatus): string {
    const colors: Record<AppointmentStatus, string> = {
      [AppointmentStatus.Scheduled]:   '#2563eb',
      [AppointmentStatus.Confirmed]:   '#7c3aed',
      [AppointmentStatus.InProgress]:  '#dc2626',
      [AppointmentStatus.Completed]:   '#16a34a',
      [AppointmentStatus.Cancelled]:   '#d97706',
      [AppointmentStatus.NoShow]:      '#6b7280',
      [AppointmentStatus.Rescheduled]: '#0891b2',
    };
    return colors[status] ?? '#6b7280';
  }

  getTodayCount(stats: any): number {
    return stats?.scheduled || 0;
  }
}
