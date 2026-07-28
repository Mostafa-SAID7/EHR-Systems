import { createReducer, on } from '@ngrx/store';
import * as AppointmentActions from './appointment.actions';
import { initialAppointmentState, AppointmentState } from './appointment.state';
import { AppointmentStatus } from '../models/appointment.model';

export const appointmentReducer = createReducer(
  initialAppointmentState,

  // ============================================================
  // LOAD APPOINTMENTS
  // ============================================================

  on(AppointmentActions.loadAppointments, (state) => ({
    ...state,
    loading: true,
    error: null
  })),

  on(AppointmentActions.loadAppointmentsSuccess, (state, { appointments, total }) => ({
    ...state,
    appointments,
    paging: { ...state.paging, total },
    loading: false
  })),

  on(AppointmentActions.loadAppointmentsFailure, (state, { error }) => ({
    ...state,
    error,
    loading: false
  })),

  // ============================================================
  // LOAD APPOINTMENT DETAIL
  // ============================================================

  on(AppointmentActions.loadAppointmentDetail, (state) => ({
    ...state,
    loading: true,
    error: null
  })),

  on(AppointmentActions.loadAppointmentDetailSuccess, (state, { appointment }) => ({
    ...state,
    selectedAppointment: appointment,
    loading: false
  })),

  on(AppointmentActions.loadAppointmentDetailFailure, (state, { error }) => ({
    ...state,
    error,
    loading: false
  })),

  // ============================================================
  // SCHEDULE APPOINTMENT
  // ============================================================

  on(AppointmentActions.scheduleAppointment, (state) => ({
    ...state,
    actionInProgress: { ...state.actionInProgress, schedule: true },
    error: null
  })),

  on(AppointmentActions.scheduleAppointmentSuccess, (state, { appointment }) => ({
    ...state,
    appointments: [appointment, ...state.appointments],
    paging: { ...state.paging, total: state.paging.total + 1 },
    actionInProgress: { ...state.actionInProgress, schedule: false }
  })),

  on(AppointmentActions.scheduleAppointmentFailure, (state, { error }) => ({
    ...state,
    error,
    actionInProgress: { ...state.actionInProgress, schedule: false }
  })),

  // ============================================================
  // CANCEL APPOINTMENT
  // ============================================================

  on(AppointmentActions.cancelAppointment, (state, { appointmentId }) => ({
    ...state,
    actionInProgress: { ...state.actionInProgress, [`cancel_${appointmentId}`]: true },
    error: null
  })),

  on(AppointmentActions.cancelAppointmentSuccess, (state, { appointmentId }) => ({
    ...state,
    appointments: state.appointments.map(a =>
      a.id === appointmentId ? { ...a, status: AppointmentStatus.Cancelled } : a
    ),
    actionInProgress: { ...state.actionInProgress, [`cancel_${appointmentId}`]: false }
  })),

  on(AppointmentActions.cancelAppointmentFailure, (state, { error }) => ({
    ...state,
    error,
    actionInProgress: { ...Object.keys(state.actionInProgress).reduce((acc, key) => ({ ...acc, [key]: false }), {}) }
  })),

  // ============================================================
  // CONFIRM APPOINTMENT
  // ============================================================

  on(AppointmentActions.confirmAppointment, (state, { appointmentId }) => ({
    ...state,
    actionInProgress: { ...state.actionInProgress, [`confirm_${appointmentId}`]: true },
    error: null
  })),

  on(AppointmentActions.confirmAppointmentSuccess, (state, { appointmentId }) => ({
    ...state,
    appointments: state.appointments.map(a =>
      a.id === appointmentId ? { ...a, status: AppointmentStatus.Confirmed } : a
    ),
    actionInProgress: { ...state.actionInProgress, [`confirm_${appointmentId}`]: false }
  })),

  on(AppointmentActions.confirmAppointmentFailure, (state, { error }) => ({
    ...state,
    error,
    actionInProgress: { ...Object.keys(state.actionInProgress).reduce((acc, key) => ({ ...acc, [key]: false }), {}) }
  })),

  // ============================================================
  // CHECK-IN APPOINTMENT
  // ============================================================

  on(AppointmentActions.checkInAppointment, (state, { appointmentId }) => ({
    ...state,
    actionInProgress: { ...state.actionInProgress, [`checkin_${appointmentId}`]: true },
    error: null
  })),

  on(AppointmentActions.checkInAppointmentSuccess, (state, { appointmentId }) => ({
    ...state,
    appointments: state.appointments.map(a =>
      a.id === appointmentId ? { ...a, status: AppointmentStatus.InProgress } : a
    ),
    actionInProgress: { ...state.actionInProgress, [`checkin_${appointmentId}`]: false }
  })),

  on(AppointmentActions.checkInAppointmentFailure, (state, { error }) => ({
    ...state,
    error,
    actionInProgress: { ...Object.keys(state.actionInProgress).reduce((acc, key) => ({ ...acc, [key]: false }), {}) }
  })),

  // ============================================================
  // COMPLETE APPOINTMENT
  // ============================================================

  on(AppointmentActions.completeAppointment, (state, { appointmentId }) => ({
    ...state,
    actionInProgress: { ...state.actionInProgress, [`complete_${appointmentId}`]: true },
    error: null
  })),

  on(AppointmentActions.completeAppointmentSuccess, (state, { appointmentId }) => ({
    ...state,
    appointments: state.appointments.map(a =>
      a.id === appointmentId ? { ...a, status: AppointmentStatus.Completed } : a
    ),
    actionInProgress: { ...state.actionInProgress, [`complete_${appointmentId}`]: false }
  })),

  on(AppointmentActions.completeAppointmentFailure, (state, { error }) => ({
    ...state,
    error,
    actionInProgress: { ...Object.keys(state.actionInProgress).reduce((acc, key) => ({ ...acc, [key]: false }), {}) }
  })),

  // ============================================================
  // AVAILABLE SLOTS
  // ============================================================

  on(AppointmentActions.loadAvailableSlots, (state) => ({
    ...state,
    loading: true,
    error: null
  })),

  on(AppointmentActions.loadAvailableSlotsSuccess, (state, { slots }) => ({
    ...state,
    availableSlots: slots,
    loading: false
  })),

  on(AppointmentActions.loadAvailableSlotsFailure, (state, { error }) => ({
    ...state,
    error,
    loading: false
  })),

  // ============================================================
  // SET PROVIDER AVAILABILITY
  // ============================================================

  on(AppointmentActions.setProviderAvailability, (state) => ({
    ...state,
    actionInProgress: { ...state.actionInProgress, setAvailability: true },
    error: null
  })),

  on(AppointmentActions.setProviderAvailabilitySuccess, (state, { availability }) => ({
    ...state,
    availableSlots: [availability, ...state.availableSlots],
    actionInProgress: { ...state.actionInProgress, setAvailability: false }
  })),

  on(AppointmentActions.setProviderAvailabilityFailure, (state, { error }) => ({
    ...state,
    error,
    actionInProgress: { ...state.actionInProgress, setAvailability: false }
  })),

  // ============================================================
  // UPDATE FILTER
  // ============================================================

  on(AppointmentActions.updateFilter, (state, { filter }) => ({
    ...state,
    filter: { ...state.filter, ...filter },
    paging: { ...state.paging, pageNumber: 1 }
  })),

  // ============================================================
  // CLEAR ERROR
  // ============================================================

  on(AppointmentActions.clearError, (state) => ({
    ...state,
    error: null
  }))
);
