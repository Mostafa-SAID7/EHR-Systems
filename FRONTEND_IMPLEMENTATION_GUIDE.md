# Frontend Appointment Integration - Implementation Guide

## 🎯 STEP-BY-STEP IMPLEMENTATION

### STEP 1: Create Models File (appointment.model.ts)

**File Location:** `frontend/src/app/features/appointments/models/appointment.model.ts`

```typescript
/**
 * Appointment Models - Matches Backend DTOs
 * Organized by feature and responsibility
 */

// ============================================================
// ENUMS
// ============================================================

export enum AppointmentStatus {
  Scheduled = 'Scheduled',
  Confirmed = 'Confirmed',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

export enum AppointmentType {
  Office = 'Office',
  Telehealth = 'Telehealth',
  Phone = 'Phone'
}

export enum ReminderType {
  Email = 'Email',
  SMS = 'SMS',
  Push = 'Push'
}

// ============================================================
// CORE MODELS
// ============================================================

/**
 * Appointment Reminder
 */
export interface AppointmentReminder {
  id: string;
  appointmentId: string;
  reminderType: ReminderType;
  reminderTime: Date;
  isSent: boolean;
  sentAt?: Date;
}

/**
 * Appointment Response DTO
 * Main appointment model matching backend
 */
export interface AppointmentResponseDto {
  id: string;
  patientId: string;
  providerId: string;
  scheduledStart: Date;
  scheduledEnd: Date;
  appointmentType: AppointmentType;
  status: AppointmentStatus;
  reasonForVisit?: string;
  notes?: string;
  durationMinutes?: number;
  reminders?: AppointmentReminder[];
  createdAt: Date;
  updatedAt: Date;
}

/**
 * Appointment Detailed Response DTO
 * Extended appointment with related entities
 */
export interface AppointmentDetailedResponseDto extends AppointmentResponseDto {
  patientName: string;
  providerName: string;
  confirmedAt?: Date;
  cancelledAt?: Date;
  cancelReason?: string;
  reminderSent: boolean;
}

/**
 * Provider Availability Slot
 */
export interface ProviderAvailabilityDto {
  id: string;
  providerId: string;
  slotStart: Date;
  slotEnd: Date;
  isRecurring: boolean;
  recurrencePattern?: string;
  maxAppointmentsPerSlot: number;
  currentBookings: number;
  isActive: boolean;
}

// ============================================================
// REQUEST MODELS
// ============================================================

/**
 * Schedule Appointment Request
 */
export interface ScheduleAppointmentRequest {
  patientId: string;
  providerId: string;
  scheduledStart: Date;
  durationMinutes: number;
  appointmentType: AppointmentType;
  reasonForVisit?: string;
  notes?: string;
}

/**
 * Cancel Appointment Request
 */
export interface CancelAppointmentRequest {
  appointmentId: string;
  reason: string;
}

/**
 * Confirm/CheckIn/Complete Request (minimal)
 */
export interface AppointmentActionRequest {
  appointmentId: string;
}

/**
 * Set Provider Availability Request
 */
export interface SetProviderAvailabilityRequest {
  providerId: string;
  slotStart: Date;
  slotEnd: Date;
  isRecurring: boolean;
  recurrencePattern?: string;
  maxAppointmentsPerSlot?: number;
}

// ============================================================
// FILTER & QUERY MODELS
// ============================================================

/**
 * Appointment Filter for List/Search
 */
export interface AppointmentFilter {
  patientId?: string;
  providerId?: string;
  startDate?: Date;
  endDate?: Date;
  status?: AppointmentStatus;
  appointmentType?: AppointmentType;
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
}

/**
 * Paging Response
 */
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// ============================================================
// HELPER TYPES & UTILITY
// ============================================================

/**
 * Appointment with UI metadata
 */
export interface AppointmentWithUI extends AppointmentResponseDto {
  isEditable: boolean;
  isPending: boolean;
  canCancel: boolean;
  canConfirm: boolean;
  canCheckIn: boolean;
  canComplete: boolean;
  displayTime: string;
  statusColor: string;
}

/**
 * Get status color for UI
 */
export function getStatusColor(status: AppointmentStatus): string {
  const colors: Record<AppointmentStatus, string> = {
    [AppointmentStatus.Scheduled]: 'info',
    [AppointmentStatus.Confirmed]: 'success',
    [AppointmentStatus.InProgress]: 'warning',
    [AppointmentStatus.Completed]: 'success',
    [AppointmentStatus.Cancelled]: 'danger'
  };
  return colors[status] || 'default';
}

/**
 * Get available actions based on status
 */
export function getAvailableActions(status: AppointmentStatus): string[] {
  const actions: Record<AppointmentStatus, string[]> = {
    [AppointmentStatus.Scheduled]: ['Confirm', 'Cancel'],
    [AppointmentStatus.Confirmed]: ['CheckIn', 'Cancel'],
    [AppointmentStatus.InProgress]: ['Complete'],
    [AppointmentStatus.Completed]: [],
    [AppointmentStatus.Cancelled]: []
  };
  return actions[status] || [];
}
```

---

### STEP 2: Update Appointment Service

**File:** `frontend/src/app/features/appointments/services/appointment.service.ts`

```typescript
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
  AppointmentActionRequest,
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
  private availabilityUrl = `${environment.apiUrl}/provider-availability`;

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
      .set('providerId', providerId)
      .set('fromDate', fromDate.toISOString())
      .set('toDate', toDate.toISOString());
    
    if (appointmentType) {
      params = params.set('appointmentType', appointmentType);
    }

    return this.http.get<ProviderAvailabilityDto[]>(
      `${this.availabilityUrl}/slots`,
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
      `${this.availabilityUrl}/set`,
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
```

---

### STEP 3: Create NgRx Store Structure

**Files to create in `frontend/src/app/features/appointments/store/`:**

#### 3a. State (appointment.state.ts)

```typescript
import { AppointmentResponseDto, AppointmentDetailedResponseDto, AppointmentFilter, ProviderAvailabilityDto, PagedResult } from '../models/appointment.model';

export interface AppointmentState {
  // List data
  appointments: AppointmentResponseDto[];
  selectedAppointment: AppointmentDetailedResponseDto | null;
  availableSlots: ProviderAvailabilityDto[];
  
  // Paging
  paging: {
    pageNumber: number;
    pageSize: number;
    total: number;
  };
  
  // Filters
  filter: AppointmentFilter;
  
  // UI state
  loading: boolean;
  error: string | null;
  actionInProgress: { [key: string]: boolean };
}

export const initialAppointmentState: AppointmentState = {
  appointments: [],
  selectedAppointment: null,
  availableSlots: [],
  paging: { pageNumber: 1, pageSize: 20, total: 0 },
  filter: {},
  loading: false,
  error: null,
  actionInProgress: {}
};
```

#### 3b. Actions (appointment.actions.ts)

```typescript
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

// Load Appointments
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

// Load Appointment Detail
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

// Schedule Appointment
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

// Cancel Appointment
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

// Confirm Appointment
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

// CheckIn Appointment
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

// Complete Appointment
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

// Load Available Slots
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

// Set Provider Availability
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

// Update Filter
export const updateFilter = createAction(
  '[Appointments] Update Filter',
  props<{ filter: AppointmentFilter }>()
);

// Clear Error
export const clearError = createAction('[Appointments] Clear Error');
```

#### 3c. Reducer (appointment.reducer.ts)

```typescript
import { createReducer, on } from '@ngrx/store';
import * as AppointmentActions from './appointment.actions';
import { initialAppointmentState, AppointmentState } from './appointment.state';

export const appointmentReducer = createReducer(
  initialAppointmentState,

  // Load Appointments
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

  // Load Detail
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

  // Schedule
  on(AppointmentActions.scheduleAppointment, (state) => ({
    ...state,
    actionInProgress: { ...state.actionInProgress, schedule: true },
    error: null
  })),

  on(AppointmentActions.scheduleAppointmentSuccess, (state, { appointment }) => ({
    ...state,
    appointments: [appointment, ...state.appointments],
    actionInProgress: { ...state.actionInProgress, schedule: false }
  })),

  on(AppointmentActions.scheduleAppointmentFailure, (state, { error }) => ({
    ...state,
    error,
    actionInProgress: { ...state.actionInProgress, schedule: false }
  })),

  // Cancel
  on(AppointmentActions.cancelAppointmentSuccess, (state, { appointmentId }) => ({
    ...state,
    appointments: state.appointments.map(a =>
      a.id === appointmentId ? { ...a, status: 'Cancelled' as any } : a
    ),
    actionInProgress: { ...state.actionInProgress, [`cancel_${appointmentId}`]: false }
  })),

  // Confirm
  on(AppointmentActions.confirmAppointmentSuccess, (state, { appointmentId }) => ({
    ...state,
    appointments: state.appointments.map(a =>
      a.id === appointmentId ? { ...a, status: 'Confirmed' as any } : a
    ),
    actionInProgress: { ...state.actionInProgress, [`confirm_${appointmentId}`]: false }
  })),

  // CheckIn
  on(AppointmentActions.checkInAppointmentSuccess, (state, { appointmentId }) => ({
    ...state,
    appointments: state.appointments.map(a =>
      a.id === appointmentId ? { ...a, status: 'InProgress' as any } : a
    ),
    actionInProgress: { ...state.actionInProgress, [`checkin_${appointmentId}`]: false }
  })),

  // Complete
  on(AppointmentActions.completeAppointmentSuccess, (state, { appointmentId }) => ({
    ...state,
    appointments: state.appointments.map(a =>
      a.id === appointmentId ? { ...a, status: 'Completed' as any } : a
    ),
    actionInProgress: { ...state.actionInProgress, [`complete_${appointmentId}`]: false }
  })),

  // Available Slots
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

  // Filter
  on(AppointmentActions.updateFilter, (state, { filter }) => ({
    ...state,
    filter: { ...state.filter, ...filter },
    paging: { ...state.paging, pageNumber: 1 }
  })),

  // Clear Error
  on(AppointmentActions.clearError, (state) => ({
    ...state,
    error: null
  }))
);
```

---

## 📋 NEXT STEPS

This guide provides:
1. ✅ Complete models matching backend DTOs
2. ✅ Updated service with real HTTP calls
3. ✅ NgRx store structure (state, actions, reducer)

**Still needed:**
4. Effects (appointment.effects.ts)
5. Selectors (appointment.selectors.ts)
6. Component implementations

---

**Status: Ready for Effects & Selectors Implementation**

