import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { catchError, map, switchMap, withLatestFrom, mergeMap } from 'rxjs/operators';
import { Store } from '@ngrx/store';
import * as AppointmentActions from './appointment.actions';
import { AppointmentService } from '../services/appointment.service';

@Injectable()
export class AppointmentEffects {
  // ============================================================
  // LOAD APPOINTMENTS EFFECT
  // ============================================================
  loadAppointments$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AppointmentActions.loadAppointments),
      switchMap(({ patientId, filter }) =>
        this.appointmentService.getPatientAppointments(patientId, filter).pipe(
          map(result =>
            AppointmentActions.loadAppointmentsSuccess({
              appointments: result.items,
              total: result.totalCount
            })
          ),
          catchError(error =>
            of(
              AppointmentActions.loadAppointmentsFailure({
                error: error?.message || 'Failed to load appointments'
              })
            )
          )
        )
      )
    )
  );

  // ============================================================
  // LOAD APPOINTMENT DETAIL EFFECT
  // ============================================================
  loadAppointmentDetail$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AppointmentActions.loadAppointmentDetail),
      switchMap(({ appointmentId }) =>
        this.appointmentService.getAppointmentById(appointmentId).pipe(
          map(appointment =>
            AppointmentActions.loadAppointmentDetailSuccess({ appointment })
          ),
          catchError(error =>
            of(
              AppointmentActions.loadAppointmentDetailFailure({
                error: error?.message || 'Failed to load appointment details'
              })
            )
          )
        )
      )
    )
  );

  // ============================================================
  // SCHEDULE APPOINTMENT EFFECT
  // ============================================================
  scheduleAppointment$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AppointmentActions.scheduleAppointment),
      switchMap(({ request }) =>
        this.appointmentService.scheduleAppointment(request).pipe(
          map(appointment =>
            AppointmentActions.scheduleAppointmentSuccess({ appointment })
          ),
          catchError(error =>
            of(
              AppointmentActions.scheduleAppointmentFailure({
                error: error?.message || 'Failed to schedule appointment'
              })
            )
          )
        )
      )
    )
  );

  // ============================================================
  // CANCEL APPOINTMENT EFFECT
  // ============================================================
  cancelAppointment$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AppointmentActions.cancelAppointment),
      switchMap(({ appointmentId, reason }) =>
        this.appointmentService.cancelAppointment(appointmentId, reason).pipe(
          map(() =>
            AppointmentActions.cancelAppointmentSuccess({ appointmentId })
          ),
          catchError(error =>
            of(
              AppointmentActions.cancelAppointmentFailure({
                error: error?.message || 'Failed to cancel appointment'
              })
            )
          )
        )
      )
    )
  );

  // ============================================================
  // CONFIRM APPOINTMENT EFFECT
  // ============================================================
  confirmAppointment$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AppointmentActions.confirmAppointment),
      switchMap(({ appointmentId }) =>
        this.appointmentService.confirmAppointment(appointmentId).pipe(
          map(() =>
            AppointmentActions.confirmAppointmentSuccess({ appointmentId })
          ),
          catchError(error =>
            of(
              AppointmentActions.confirmAppointmentFailure({
                error: error?.message || 'Failed to confirm appointment'
              })
            )
          )
        )
      )
    )
  );

  // ============================================================
  // CHECK-IN APPOINTMENT EFFECT
  // ============================================================
  checkInAppointment$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AppointmentActions.checkInAppointment),
      switchMap(({ appointmentId }) =>
        this.appointmentService.checkInAppointment(appointmentId).pipe(
          map(() =>
            AppointmentActions.checkInAppointmentSuccess({ appointmentId })
          ),
          catchError(error =>
            of(
              AppointmentActions.checkInAppointmentFailure({
                error: error?.message || 'Failed to check-in appointment'
              })
            )
          )
        )
      )
    )
  );

  // ============================================================
  // COMPLETE APPOINTMENT EFFECT
  // ============================================================
  completeAppointment$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AppointmentActions.completeAppointment),
      switchMap(({ appointmentId }) =>
        this.appointmentService.completeAppointment(appointmentId).pipe(
          map(() =>
            AppointmentActions.completeAppointmentSuccess({ appointmentId })
          ),
          catchError(error =>
            of(
              AppointmentActions.completeAppointmentFailure({
                error: error?.message || 'Failed to complete appointment'
              })
            )
          )
        )
      )
    )
  );

  // ============================================================
  // LOAD AVAILABLE SLOTS EFFECT
  // ============================================================
  loadAvailableSlots$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AppointmentActions.loadAvailableSlots),
      switchMap(({ providerId, fromDate, toDate, appointmentType }) =>
        this.appointmentService
          .getAvailableSlots(providerId, fromDate, toDate, appointmentType)
          .pipe(
            map(slots =>
              AppointmentActions.loadAvailableSlotsSuccess({ slots })
            ),
            catchError(error =>
              of(
                AppointmentActions.loadAvailableSlotsFailure({
                  error: error?.message || 'Failed to load available slots'
                })
              )
            )
          )
      )
    )
  );

  // ============================================================
  // SET PROVIDER AVAILABILITY EFFECT
  // ============================================================
  setProviderAvailability$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AppointmentActions.setProviderAvailability),
      switchMap(({ request }) =>
        this.appointmentService.setProviderAvailability(request).pipe(
          map(availability =>
            AppointmentActions.setProviderAvailabilitySuccess({ availability })
          ),
          catchError(error =>
            of(
              AppointmentActions.setProviderAvailabilityFailure({
                error: error?.message || 'Failed to set provider availability'
              })
            )
          )
        )
      )
    )
  );

  // ============================================================
  // SCHEDULE REMINDER EFFECT
  // ============================================================
  scheduleReminder$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AppointmentActions.scheduleReminder),
      switchMap(({ appointmentId, reminderTime, reminderType }) =>
        this.appointmentService.scheduleReminder(appointmentId, reminderTime, reminderType).pipe(
          map(() =>
            AppointmentActions.scheduleReminderSuccess({ appointmentId })
          ),
          catchError(error =>
            of(
              AppointmentActions.scheduleReminderFailure({
                error: error?.message || 'Failed to schedule reminder'
              })
            )
          )
        )
      )
    )
  );

  // ============================================================
  // LOAD PENDING REMINDERS EFFECT
  // ============================================================
  loadPendingReminders$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AppointmentActions.loadPendingReminders),
      switchMap(() =>
        this.appointmentService.getPendingReminders().pipe(
          map(reminders =>
            AppointmentActions.loadPendingRemindersSuccess({ reminders })
          ),
          catchError(error =>
            of(
              AppointmentActions.loadPendingRemindersFailure({
                error: error?.message || 'Failed to load pending reminders'
              })
            )
          )
        )
      )
    )
  );

  // ============================================================
  // SEND REMINDER EFFECT
  // ============================================================
  sendReminder$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AppointmentActions.sendReminder),
      switchMap(({ reminderId }) =>
        this.appointmentService.sendReminder(reminderId).pipe(
          map(() =>
            AppointmentActions.sendReminderSuccess({ reminderId })
          ),
          catchError(error =>
            of(
              AppointmentActions.sendReminderFailure({
                error: error?.message || 'Failed to send reminder'
              })
            )
          )
        )
      )
    )
  );

  // ============================================================
  // SEND ALL PENDING REMINDERS EFFECT
  // ============================================================
  sendAllPendingReminders$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AppointmentActions.sendAllPendingReminders),
      switchMap(() =>
        this.appointmentService.sendAllPendingReminders().pipe(
          map(({ sentCount }) =>
            AppointmentActions.sendAllPendingRemindersSuccess({ sentCount })
          ),
          catchError(error =>
            of(
              AppointmentActions.sendAllPendingRemindersFailure({
                error: error?.message || 'Failed to send reminders'
              })
            )
          )
        )
      )
    )
  );

  // ============================================================
  // GET NOTIFICATION STATUS EFFECT
  // ============================================================
  getNotificationStatus$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AppointmentActions.getNotificationStatus),
      switchMap(() =>
        this.appointmentService.getNotificationStatus().pipe(
          map(status =>
            AppointmentActions.getNotificationStatusSuccess({ status })
          ),
          catchError(error =>
            of(
              AppointmentActions.getNotificationStatusFailure({
                error: error?.message || 'Failed to get notification status'
              })
            )
          )
        )
      )
    )
  );

  // ============================================================
  // ADD NOTE EFFECT
  // ============================================================
  addNote$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AppointmentActions.addNote),
      switchMap(({ appointmentId, content, createdById, privacyLevel }) =>
        this.appointmentService.addNote(appointmentId, content, createdById, privacyLevel || 'InternalOnly').pipe(
          map(() =>
            AppointmentActions.addNoteSuccess({ appointmentId })
          ),
          catchError(error =>
            of(
              AppointmentActions.addNoteFailure({
                error: error?.message || 'Failed to add note'
              })
            )
          )
        )
      )
    )
  );

  // ============================================================
  // RESCHEDULE APPOINTMENT EFFECT
  // ============================================================
  rescheduleAppointment$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AppointmentActions.rescheduleAppointment),
      switchMap(({ appointmentId, newScheduledStart, durationMinutes, reason }) =>
        this.appointmentService.rescheduleAppointment(appointmentId, newScheduledStart, durationMinutes, reason).pipe(
          map(() =>
            AppointmentActions.rescheduleAppointmentSuccess({ appointmentId })
          ),
          catchError(error =>
            of(
              AppointmentActions.rescheduleAppointmentFailure({
                error: error?.message || 'Failed to reschedule appointment'
              })
            )
          )
        )
      )
    )
  );

  constructor(
    private actions$: Actions,
    private appointmentService: AppointmentService,
    private store: Store
  ) {}
}
