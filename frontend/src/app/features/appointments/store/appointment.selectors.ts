import { createFeatureSelector, createSelector } from '@ngrx/store';
import { AppointmentState } from './appointment.state';
import { AppointmentStatus } from '../models/appointment.model';

// ============================================================
// FEATURE SELECTOR
// ============================================================

export const selectAppointmentFeature = createFeatureSelector<AppointmentState>(
  'appointments'
);

// ============================================================
// MAIN SELECTORS
// ============================================================

export const selectAppointments = createSelector(
  selectAppointmentFeature,
  (state: AppointmentState) => state.appointments
);

export const selectSelectedAppointment = createSelector(
  selectAppointmentFeature,
  (state: AppointmentState) => state.selectedAppointment
);

export const selectAvailableSlots = createSelector(
  selectAppointmentFeature,
  (state: AppointmentState) => state.availableSlots
);

export const selectLoading = createSelector(
  selectAppointmentFeature,
  (state: AppointmentState) => state.loading
);

export const selectError = createSelector(
  selectAppointmentFeature,
  (state: AppointmentState) => state.error
);

export const selectFilter = createSelector(
  selectAppointmentFeature,
  (state: AppointmentState) => state.filter
);

export const selectPaging = createSelector(
  selectAppointmentFeature,
  (state: AppointmentState) => state.paging
);

export const selectActionInProgress = createSelector(
  selectAppointmentFeature,
  (state: AppointmentState) => state.actionInProgress
);

// ============================================================
// FILTERED SELECTORS
// ============================================================

export const selectScheduledAppointments = createSelector(
  selectAppointments,
  (appointments) =>
    appointments.filter(a => a.status === AppointmentStatus.Scheduled)
);

export const selectConfirmedAppointments = createSelector(
  selectAppointments,
  (appointments) =>
    appointments.filter(a => a.status === AppointmentStatus.Confirmed)
);

export const selectCompletedAppointments = createSelector(
  selectAppointments,
  (appointments) =>
    appointments.filter(a => a.status === AppointmentStatus.Completed)
);

export const selectCancelledAppointments = createSelector(
  selectAppointments,
  (appointments) =>
    appointments.filter(a => a.status === AppointmentStatus.Cancelled)
);

export const selectInProgressAppointments = createSelector(
  selectAppointments,
  (appointments) =>
    appointments.filter(a => a.status === AppointmentStatus.InProgress)
);

// ============================================================
// ACTION IN PROGRESS SELECTORS
// ============================================================

export const selectScheduleInProgress = createSelector(
  selectActionInProgress,
  (actions) => actions['schedule'] || false
);

export const selectConfirmInProgress = (appointmentId: string) =>
  createSelector(
    selectActionInProgress,
    (actions) => actions[`confirm_${appointmentId}`] || false
  );

export const selectCancelInProgress = (appointmentId: string) =>
  createSelector(
    selectActionInProgress,
    (actions) => actions[`cancel_${appointmentId}`] || false
  );

export const selectCheckInInProgress = (appointmentId: string) =>
  createSelector(
    selectActionInProgress,
    (actions) => actions[`checkin_${appointmentId}`] || false
  );

export const selectCompleteInProgress = (appointmentId: string) =>
  createSelector(
    selectActionInProgress,
    (actions) => actions[`complete_${appointmentId}`] || false
  );

// ============================================================
// COMBINED SELECTORS
// ============================================================

export const selectAppointmentStats = createSelector(
  selectAppointments,
  (appointments) => ({
    total: appointments.length,
    scheduled: appointments.filter(a => a.status === AppointmentStatus.Scheduled).length,
    confirmed: appointments.filter(a => a.status === AppointmentStatus.Confirmed).length,
    inProgress: appointments.filter(a => a.status === AppointmentStatus.InProgress).length,
    completed: appointments.filter(a => a.status === AppointmentStatus.Completed).length,
    cancelled: appointments.filter(a => a.status === AppointmentStatus.Cancelled).length
  })
);

export const selectUpcomingAppointments = createSelector(
  selectAppointments,
  (appointments) => {
    const now = new Date();
    return appointments
      .filter(
        a =>
          new Date(a.scheduledStart) > now &&
          (a.status === AppointmentStatus.Scheduled ||
            a.status === AppointmentStatus.Confirmed)
      )
      .sort(
        (a, b) =>
          new Date(a.scheduledStart).getTime() -
          new Date(b.scheduledStart).getTime()
      );
  }
);

export const selectPastAppointments = createSelector(
  selectAppointments,
  (appointments) => {
    const now = new Date();
    return appointments
      .filter(a => new Date(a.scheduledStart) <= now)
      .sort(
        (a, b) =>
          new Date(b.scheduledStart).getTime() -
          new Date(a.scheduledStart).getTime()
      );
  }
);
