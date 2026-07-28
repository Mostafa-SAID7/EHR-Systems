import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '@env/environment';
import {
  AppointmentResponseDto,
  AppointmentDetailedResponseDto,
  ScheduleAppointmentRequest,
  CancelAppointmentRequest,
  AppointmentFilter,
  PagedResult,
  ProviderAvailabilityDto,
  SetProviderAvailabilityRequest
} from '../models/appointment.model';

@Injectable({
  providedIn: 'root'
})
export class AppointmentService {
  private apiUrl = `${environment.apiUrl}/appointments`;
  private providerUrl = `${environment.apiUrl}/providers`;

  constructor(private http: HttpClient) {}

  // ============================================================
  // APPOINTMENT QUERIES
  // ============================================================

  /**
   * Get all appointments for a patient
   */
  getPatientAppointments(
    patientId: string,
    filter?: AppointmentFilter
  ): Observable<PagedResult<AppointmentResponseDto>> {
    let params = new HttpParams();
    
    if (filter?.startDate) params = params.set('fromDate', filter.startDate.toISOString());
    if (filter?.endDate) params = params.set('toDate', filter.endDate.toISOString());
    if (filter?.pageNumber) params = params.set('pageNumber', filter.pageNumber.toString());
    if (filter?.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<AppointmentResponseDto>>(
      `${this.apiUrl}/patient/${patientId}`,
      { params }
    ).pipe(
      map(result => this.mapPagedResult(result)),
      catchError(error => this.handleError('getPatientAppointments', error))
    );
  }

  /**
   * Get appointment by ID
   */
  getAppointmentById(appointmentId: string): Observable<AppointmentDetailedResponseDto> {
    return this.http.get<AppointmentDetailedResponseDto>(
      `${this.apiUrl}/${appointmentId}`
    ).pipe(
      map(apt => this.mapAppointmentDates(apt)),
      catchError(error => this.handleError('getAppointmentById', error))
    );
  }

  /**
   * Get appointments by type
   */
  getAppointmentsByType(
    appointmentType: string,
    pageNumber: number = 1,
    pageSize: number = 20
  ): Observable<PagedResult<AppointmentResponseDto>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<AppointmentResponseDto>>(
      `${this.apiUrl}/by-type/${appointmentType}`,
      { params }
    ).pipe(
      map(result => this.mapPagedResult(result)),
      catchError(error => this.handleError('getAppointmentsByType', error))
    );
  }

  // ============================================================
  // APPOINTMENT COMMANDS
  // ============================================================

  /**
   * Schedule new appointment
   */
  scheduleAppointment(request: ScheduleAppointmentRequest): Observable<AppointmentResponseDto> {
    const payload = {
      ...request,
      scheduledStart: request.scheduledStart.toISOString(),
      durationMinutes: request.durationMinutes
    };

    return this.http.post<AppointmentResponseDto>(
      this.apiUrl,
      payload
    ).pipe(
      map(apt => this.mapAppointmentDates(apt)),
      catchError(error => this.handleError('scheduleAppointment', error))
    );
  }

  /**
   * Cancel appointment
   */
  cancelAppointment(appointmentId: string, reason: string): Observable<void> {
    const params = new HttpParams().set('reason', reason);
    
    return this.http.post<void>(
      `${this.apiUrl}/${appointmentId}/cancel`,
      {},
      { params }
    ).pipe(
      catchError(error => this.handleError('cancelAppointment', error))
    );
  }

  /**
   * Confirm appointment
   */
  confirmAppointment(appointmentId: string): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/${appointmentId}/confirm`,
      {}
    ).pipe(
      catchError(error => this.handleError('confirmAppointment', error))
    );
  }

  /**
   * Check in to appointment
   */
  checkInAppointment(appointmentId: string): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/${appointmentId}/check-in`,
      {}
    ).pipe(
      catchError(error => this.handleError('checkInAppointment', error))
    );
  }

  /**
   * Complete appointment
   */
  completeAppointment(appointmentId: string): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/${appointmentId}/complete`,
      {}
    ).pipe(
      catchError(error => this.handleError('completeAppointment', error))
    );
  }

  // ============================================================
  // REMINDERS
  // ============================================================

  /**
   * Schedule a reminder for an appointment
   */
  scheduleReminder(appointmentId: string, reminderTime: Date, reminderType: string): Observable<void> {
    const payload = {
      reminderTime: reminderTime.toISOString(),
      reminderType
    };

    return this.http.post<void>(
      `${this.apiUrl}/${appointmentId}/reminders`,
      payload
    ).pipe(
      catchError(error => this.handleError('scheduleReminder', error))
    );
  }

  /**
   * Get all pending reminders
   */
  getPendingReminders(): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.apiUrl}/reminders/pending`
    ).pipe(
      catchError(error => this.handleError('getPendingReminders', error))
    );
  }

  /**
   * Send a specific reminder
   */
  sendReminder(reminderId: string): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/reminders/${reminderId}/send`,
      {}
    ).pipe(
      catchError(error => this.handleError('sendReminder', error))
    );
  }

  /**
   * Send all pending reminders
   */
  sendAllPendingReminders(): Observable<{ sentCount: number }> {
    return this.http.post<{ sentCount: number }>(
      `${this.apiUrl}/reminders/send-all`,
      {}
    ).pipe(
      catchError(error => this.handleError('sendAllPendingReminders', error))
    );
  }

  // ============================================================
  // PROVIDER AVAILABILITY
  // ============================================================

  /**
   * Get available slots for provider
   */
  getAvailableSlots(
    providerId: string,
    fromDate: Date,
    toDate: Date,
    appointmentType?: string
  ): Observable<ProviderAvailabilityDto[]> {
    let params = new HttpParams()
      .set('fromDate', fromDate.toISOString())
      .set('toDate', toDate.toISOString());
    
    if (appointmentType) {
      params = params.set('appointmentType', appointmentType);
    }

    return this.http.get<ProviderAvailabilityDto[]>(
      `${this.providerUrl}/${providerId}/availability`,
      { params }
    ).pipe(
      map(slots => slots.map(s => this.mapAvailabilityDates(s))),
      catchError(error => this.handleError('getAvailableSlots', error))
    );
  }

  /**
   * Set provider availability
   */
  setProviderAvailability(request: SetProviderAvailabilityRequest): Observable<ProviderAvailabilityDto> {
    const payload = {
      ...request,
      slotStart: request.slotStart.toISOString(),
      slotEnd: request.slotEnd.toISOString()
    };

    return this.http.post<ProviderAvailabilityDto>(
      `${this.providerUrl}/${request.providerId}/availability`,
      payload
    ).pipe(
      map(av => this.mapAvailabilityDates(av)),
      catchError(error => this.handleError('setProviderAvailability', error))
    );
  }

  // ============================================================
  // HEALTH CHECK
  // ============================================================

  /**
   * Check appointment service health
   */
  healthCheck(): Observable<{ status: string }> {
    return this.http.get<{ status: string }>(`${this.apiUrl}/health`).pipe(
      catchError(error => this.handleError('healthCheck', error))
    );
  }

  /**
   * Get notification provider status
   */
  getNotificationStatus(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/health/notifications`).pipe(
      catchError(error => this.handleError('getNotificationStatus', error))
    );
  }

  // ============================================================
  // NOTES
  // ============================================================

  /**
   * Add a note to an appointment
   */
  addNote(appointmentId: string, content: string, createdById: string, privacyLevel: string = 'InternalOnly'): Observable<void> {
    const payload = {
      content,
      createdById,
      privacyLevel
    };

    return this.http.post<void>(
      `${this.apiUrl}/${appointmentId}/notes`,
      payload
    ).pipe(
      catchError(error => this.handleError('addNote', error))
    );
  }

  // ============================================================
  // RESCHEDULING
  // ============================================================

  /**
   * Reschedule an appointment
   */
  rescheduleAppointment(
    appointmentId: string,
    newScheduledStart: Date,
    durationMinutes: number,
    reason?: string
  ): Observable<void> {
    const payload = {
      newScheduledStart: newScheduledStart.toISOString(),
      durationMinutes,
      reason,
      initiatedById: 'current-user-id', // Would come from auth service
      initiatedBy: 'Patient'
    };

    return this.http.post<void>(
      `${this.apiUrl}/${appointmentId}/reschedule`,
      payload
    ).pipe(
      catchError(error => this.handleError('rescheduleAppointment', error))
    );
  }

  // ============================================================
  // PRIVATE HELPERS
  // ============================================================

  private mapAppointmentDates(apt: any): AppointmentDetailedResponseDto {
    return {
      ...apt,
      scheduledStart: new Date(apt.scheduledStart),
      scheduledEnd: new Date(apt.scheduledEnd),
      createdAt: new Date(apt.createdAt),
      updatedAt: new Date(apt.updatedAt),
      confirmedAt: apt.confirmedAt ? new Date(apt.confirmedAt) : undefined,
      cancelledAt: apt.cancelledAt ? new Date(apt.cancelledAt) : undefined
    };
  }

  private mapAvailabilityDates(av: any): ProviderAvailabilityDto {
    return {
      ...av,
      slotStart: new Date(av.slotStart),
      slotEnd: new Date(av.slotEnd)
    };
  }

  private mapPagedResult<T>(result: any): PagedResult<T> {
    return {
      items: result.items || result.data || [],
      totalCount: result.totalCount || 0,
      pageNumber: result.pageNumber || 1,
      pageSize: result.pageSize || 20,
      totalPages: result.totalPages || 0,
      hasNextPage: result.hasNextPage || false,
      hasPreviousPage: result.hasPreviousPage || false
    };
  }

  private handleError(operation: string, error: any): Observable<never> {
    console.error(`Appointment service error in ${operation}:`, error);
    
    const message = error?.error?.message || 
                   error?.statusText || 
                   'An unexpected error occurred';
    
    return throwError(() => ({
      message,
      operation,
      status: error?.status,
      error
    }));
  }
}
