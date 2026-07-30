import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { AppointmentNotesCardComponent } from '../../components/appointment-notes-card/appointment-notes-card.component';
import { AppointmentVitalsCardComponent } from '../../components/appointment-vitals-card/appointment-vitals-card.component';
import * as AppointmentActions from '../../store/appointment.actions';
import {
  selectSelectedAppointment,
  selectLoading,
  selectError,
  selectConfirmInProgress,
  selectCancelInProgress,
  selectCheckInInProgress,
  selectCompleteInProgress
} from '../../store/appointment.selectors';
import { AppointmentDetailedResponseDto, AppointmentStatus, getAvailableActions } from '../../models/appointment.model';

@Component({
  selector: 'app-appointment-detail-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    AppointmentNotesCardComponent,
    AppointmentVitalsCardComponent,
  ],
  templateUrl: './appointment-detail-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentDetailPageComponent implements OnInit {
  appointment$: Observable<AppointmentDetailedResponseDto | null>;
  loading$: Observable<boolean>;
  error$: Observable<string | null>;
  
  confirmInProgress$!: Observable<boolean>;
  cancelInProgress$!: Observable<boolean>;
  checkInInProgress$!: Observable<boolean>;
  completeInProgress$!: Observable<boolean>;

  statusClass(status: AppointmentStatus): string {
    const classes: Record<AppointmentStatus, string> = {
      [AppointmentStatus.Scheduled]:   'badge-info',
      [AppointmentStatus.Confirmed]:   'badge-primary',
      [AppointmentStatus.InProgress]:  'badge-warning',
      [AppointmentStatus.Completed]:   'badge-success',
      [AppointmentStatus.Cancelled]:   'badge-danger',
      [AppointmentStatus.NoShow]:      'badge-neutral',
      [AppointmentStatus.Rescheduled]: 'badge-info',
    };
    return classes[status] || 'badge-neutral';
  }

  constructor(
    private store: Store,
    private route: ActivatedRoute
  ) {
    this.appointment$ = this.store.select(selectSelectedAppointment);
    this.loading$ = this.store.select(selectLoading);
    this.error$ = this.store.select(selectError);
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const appointmentId = params['id'];
      if (appointmentId) {
        this.store.dispatch(AppointmentActions.loadAppointmentDetail({ appointmentId }));
        
        // Setup progress observables
        this.confirmInProgress$ = this.store.select(selectConfirmInProgress(appointmentId));
        this.cancelInProgress$ = this.store.select(selectCancelInProgress(appointmentId));
        this.checkInInProgress$ = this.store.select(selectCheckInInProgress(appointmentId));
        this.completeInProgress$ = this.store.select(selectCompleteInProgress(appointmentId));
      }
    });
  }

  confirm(appointmentId: string): void {
    this.store.dispatch(AppointmentActions.confirmAppointment({ appointmentId }));
  }

  cancel(appointmentId: string, reason: string = 'Patient requested'): void {
    this.store.dispatch(
      AppointmentActions.cancelAppointment({ appointmentId, reason })
    );
  }

  checkIn(appointmentId: string): void {
    this.store.dispatch(AppointmentActions.checkInAppointment({ appointmentId }));
  }

  complete(appointmentId: string): void {
    this.store.dispatch(AppointmentActions.completeAppointment({ appointmentId }));
  }

  getAvailableActions(status: AppointmentStatus): string[] {
    return getAvailableActions(status);
  }
}

