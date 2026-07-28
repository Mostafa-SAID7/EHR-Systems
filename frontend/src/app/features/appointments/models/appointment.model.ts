/**
 * Appointment Models - Matches Backend DTOs
 */

// ============================================================
// ENUMS
// ============================================================

export enum AppointmentStatus {
  Scheduled = 'Scheduled',
  Confirmed = 'Confirmed',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
  NoShow = 'NoShow',
  Rescheduled = 'Rescheduled'
}

export enum AppointmentType {
  Office = 'Office',
  Telehealth = 'Telehealth',
  Phone = 'Phone'
}

export enum ReminderType {
  Email = 'Email',
  SMS = 'SMS',
  Push = 'Push',
  InApp = 'InApp'
}

export enum CancellationReason {
  PatientRequested = 'PatientRequested',
  ProviderRequested = 'ProviderRequested',
  Emergency = 'Emergency',
  DoubleBooking = 'DoubleBooking',
  SchedulingConflict = 'SchedulingConflict',
  Weather = 'Weather',
  SystemError = 'SystemError',
  Other = 'Other'
}

// ============================================================
// CORE MODELS
// ============================================================

export interface AppointmentReminder {
  id: string;
  appointmentId: string;
  reminderType: ReminderType;
  reminderTime: Date;
  isSent: boolean;
  sentAt?: Date;
}

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

export interface AppointmentDetailedResponseDto extends AppointmentResponseDto {
  patientName: string;
  providerName: string;
  confirmedAt?: Date;
  cancelledAt?: Date;
  cancelReason?: CancellationReason | string;
  reminderSent: boolean;
}

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

export interface ScheduleAppointmentRequest {
  patientId: string;
  providerId: string;
  scheduledStart: Date;
  durationMinutes: number;
  appointmentType: AppointmentType;
  reasonForVisit?: string;
  notes?: string;
}

export interface CancelAppointmentRequest {
  appointmentId: string;
  reason: CancellationReason | string;
}

export interface AppointmentActionRequest {
  appointmentId: string;
}

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
// HELPER FUNCTIONS
// ============================================================

export interface NotificationProviderStatus {
  email: boolean;
  sms: boolean;
  push: boolean;
  inApp: boolean;
  availableProviders: number;
}

export function getStatusColor(status: AppointmentStatus): string {
  const colors: Record<AppointmentStatus, string> = {
    [AppointmentStatus.Scheduled]: 'info',
    [AppointmentStatus.Confirmed]: 'success',
    [AppointmentStatus.InProgress]: 'warning',
    [AppointmentStatus.Completed]: 'success',
    [AppointmentStatus.Cancelled]: 'danger',
    [AppointmentStatus.NoShow]: 'error',
    [AppointmentStatus.Rescheduled]: 'info'
  };
  return colors[status] || 'default';
}

export function getAvailableActions(status: AppointmentStatus): string[] {
  const actions: Record<AppointmentStatus, string[]> = {
    [AppointmentStatus.Scheduled]: ['Confirm', 'Cancel'],
    [AppointmentStatus.Confirmed]: ['CheckIn', 'Cancel'],
    [AppointmentStatus.InProgress]: ['Complete'],
    [AppointmentStatus.Completed]: [],
    [AppointmentStatus.Cancelled]: [],
    [AppointmentStatus.NoShow]: [],
    [AppointmentStatus.Rescheduled]: ['Confirm', 'Cancel']
  };
  return actions[status] || [];
}
