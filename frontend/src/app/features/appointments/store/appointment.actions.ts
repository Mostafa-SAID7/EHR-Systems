import { createAction, props } from '@ngrx/store';
import {
  AppointmentResponseDto,
  AppointmentDetailedResponseDto,
  ScheduleAppointmentRequest,
  CancelAppointmentRequest,
  AppointmentFilter,
  ProviderAvailabilityDto,
  SetProviderAvailabilityRequest
} from '../models/appointment.model';

// ============================================================
// LOAD APPOINTMENTS
// ============================================================

export const loadAppointments = createAction(
  '[Appointments] Load Appointments',
  props<{ patientId: string; filter?: AppointmentFilter }>()
);

export const loadAppointmentsSuccess = createAction(
  '[Appointments] Load Appointments Success',
  props<{ appointments: AppointmentResponseDto[]; total: number }>()
);

export const loadAppointmentsFailure = createAction(
  '[Appointments] Load Appointments Failure',
  props<{ error: string }>()
);

// ============================================================
// LOAD APPOINTMENT DETAIL
// ============================================================

export const loadAppointmentDetail = createAction(
  '[Appointments] Load Appointment Detail',
  props<{ appointmentId: string }>()
);

export const loadAppointmentDetailSuccess = createAction(
  '[Appointments] Load Appointment Detail Success',
  props<{ appointment: AppointmentDetailedResponseDto }>()
);

export const loadAppointmentDetailFailure = createAction(
  '[Appointments] Load Appointment Detail Failure',
  props<{ error: string }>()
);

// ============================================================
// SCHEDULE APPOINTMENT
// ============================================================

export const scheduleAppointment = createAction(
  '[Appointments] Schedule Appointment',
  props<{ request: ScheduleAppointmentRequest }>()
);

export const scheduleAppointmentSuccess = createAction(
  '[Appointments] Schedule Appointment Success',
  props<{ appointment: AppointmentResponseDto }>()
);

export const scheduleAppointmentFailure = createAction(
  '[Appointments] Schedule Appointment Failure',
  props<{ error: string }>()
);

// ============================================================
// CANCEL APPOINTMENT
// ============================================================

export const cancelAppointment = createAction(
  '[Appointments] Cancel Appointment',
  props<{ appointmentId: string; reason: string }>()
);

export const cancelAppointmentSuccess = createAction(
  '[Appointments] Cancel Appointment Success',
  props<{ appointmentId: string }>()
);

export const cancelAppointmentFailure = createAction(
  '[Appointments] Cancel Appointment Failure',
  props<{ error: string }>()
);

// ============================================================
// CONFIRM APPOINTMENT
// ============================================================

export const confirmAppointment = createAction(
  '[Appointments] Confirm Appointment',
  props<{ appointmentId: string }>()
);

export const confirmAppointmentSuccess = createAction(
  '[Appointments] Confirm Appointment Success',
  props<{ appointmentId: string }>()
);

export const confirmAppointmentFailure = createAction(
  '[Appointments] Confirm Appointment Failure',
  props<{ error: string }>()
);

// ============================================================
// CHECK-IN APPOINTMENT
// ============================================================

export const checkInAppointment = createAction(
  '[Appointments] CheckIn Appointment',
  props<{ appointmentId: string }>()
);

export const checkInAppointmentSuccess = createAction(
  '[Appointments] CheckIn Appointment Success',
  props<{ appointmentId: string }>()
);

export const checkInAppointmentFailure = createAction(
  '[Appointments] CheckIn Appointment Failure',
  props<{ error: string }>()
);

// ============================================================
// COMPLETE APPOINTMENT
// ============================================================

export const completeAppointment = createAction(
  '[Appointments] Complete Appointment',
  props<{ appointmentId: string }>()
);

export const completeAppointmentSuccess = createAction(
  '[Appointments] Complete Appointment Success',
  props<{ appointmentId: string }>()
);

export const completeAppointmentFailure = createAction(
  '[Appointments] Complete Appointment Failure',
  props<{ error: string }>()
);

// ============================================================
// LOAD AVAILABLE SLOTS
// ============================================================

export const loadAvailableSlots = createAction(
  '[Appointments] Load Available Slots',
  props<{ providerId: string; fromDate: Date; toDate: Date; appointmentType?: string }>()
);

export const loadAvailableSlotsSuccess = createAction(
  '[Appointments] Load Available Slots Success',
  props<{ slots: ProviderAvailabilityDto[] }>()
);

export const loadAvailableSlotsFailure = createAction(
  '[Appointments] Load Available Slots Failure',
  props<{ error: string }>()
);

// ============================================================
// SET PROVIDER AVAILABILITY
// ============================================================

export const setProviderAvailability = createAction(
  '[Appointments] Set Provider Availability',
  props<{ request: SetProviderAvailabilityRequest }>()
);

export const setProviderAvailabilitySuccess = createAction(
  '[Appointments] Set Provider Availability Success',
  props<{ availability: ProviderAvailabilityDto }>()
);

export const setProviderAvailabilityFailure = createAction(
  '[Appointments] Set Provider Availability Failure',
  props<{ error: string }>()
);

// ============================================================
// UPDATE FILTER
// ============================================================

export const updateFilter = createAction(
  '[Appointments] Update Filter',
  props<{ filter: AppointmentFilter }>()
);

// ============================================================
// CLEAR ERROR
// ============================================================

export const clearError = createAction('[Appointments] Clear Error');

// ============================================================
// SCHEDULE REMINDER
// ============================================================

export const scheduleReminder = createAction(
  '[Appointments] Schedule Reminder',
  props<{ appointmentId: string; reminderTime: Date; reminderType: string }>()
);

export const scheduleReminderSuccess = createAction(
  '[Appointments] Schedule Reminder Success',
  props<{ appointmentId: string }>()
);

export const scheduleReminderFailure = createAction(
  '[Appointments] Schedule Reminder Failure',
  props<{ error: string }>()
);

// ============================================================
// LOAD PENDING REMINDERS
// ============================================================

export const loadPendingReminders = createAction(
  '[Appointments] Load Pending Reminders'
);

export const loadPendingRemindersSuccess = createAction(
  '[Appointments] Load Pending Reminders Success',
  props<{ reminders: any[] }>()
);

export const loadPendingRemindersFailure = createAction(
  '[Appointments] Load Pending Reminders Failure',
  props<{ error: string }>()
);

// ============================================================
// SEND REMINDER
// ============================================================

export const sendReminder = createAction(
  '[Appointments] Send Reminder',
  props<{ reminderId: string }>()
);

export const sendReminderSuccess = createAction(
  '[Appointments] Send Reminder Success',
  props<{ reminderId: string }>()
);

export const sendReminderFailure = createAction(
  '[Appointments] Send Reminder Failure',
  props<{ error: string }>()
);

// ============================================================
// SEND ALL PENDING REMINDERS
// ============================================================

export const sendAllPendingReminders = createAction(
  '[Appointments] Send All Pending Reminders'
);

export const sendAllPendingRemindersSuccess = createAction(
  '[Appointments] Send All Pending Reminders Success',
  props<{ sentCount: number }>()
);

export const sendAllPendingRemindersFailure = createAction(
  '[Appointments] Send All Pending Reminders Failure',
  props<{ error: string }>()
);

// ============================================================
// GET NOTIFICATION STATUS
// ============================================================

export const getNotificationStatus = createAction(
  '[Appointments] Get Notification Status'
);

export const getNotificationStatusSuccess = createAction(
  '[Appointments] Get Notification Status Success',
  props<{ status: any }>()
);

export const getNotificationStatusFailure = createAction(
  '[Appointments] Get Notification Status Failure',
  props<{ error: string }>()
);

// ============================================================
// ADD NOTE
// ============================================================

export const addNote = createAction(
  '[Appointments] Add Note',
  props<{ appointmentId: string; content: string; createdById: string; privacyLevel?: string }>()
);

export const addNoteSuccess = createAction(
  '[Appointments] Add Note Success',
  props<{ appointmentId: string }>()
);

export const addNoteFailure = createAction(
  '[Appointments] Add Note Failure',
  props<{ error: string }>()
);

// ============================================================
// RESCHEDULE APPOINTMENT
// ============================================================

export const rescheduleAppointment = createAction(
  '[Appointments] Reschedule Appointment',
  props<{ appointmentId: string; newScheduledStart: Date; durationMinutes: number; reason?: string }>()
);

export const rescheduleAppointmentSuccess = createAction(
  '[Appointments] Reschedule Appointment Success',
  props<{ appointmentId: string }>()
);

export const rescheduleAppointmentFailure = createAction(
  '[Appointments] Reschedule Appointment Failure',
  props<{ error: string }>()
);
