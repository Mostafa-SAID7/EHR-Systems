import {
  AppointmentResponseDto,
  AppointmentDetailedResponseDto,
  AppointmentFilter,
  ProviderAvailabilityDto,
  AppointmentReminder
} from '../models/appointment.model';

export interface AppointmentState {
  // List data
  appointments: AppointmentResponseDto[];
  selectedAppointment: AppointmentDetailedResponseDto | null;
  availableSlots: ProviderAvailabilityDto[];
  pendingReminders: AppointmentReminder[];
  notificationStatus: any;
  
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
  
  // Real-time
  realtimeConnected: boolean;
}

export const initialAppointmentState: AppointmentState = {
  appointments: [],
  selectedAppointment: null,
  availableSlots: [],
  pendingReminders: [],
  notificationStatus: null,
  paging: { pageNumber: 1, pageSize: 20, total: 0 },
  filter: {},
  loading: false,
  error: null,
  actionInProgress: {},
  realtimeConnected: false
};
