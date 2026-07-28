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
  pendingReminders: [],
  paging: { pageNumber: 1, pageSize: 20, total: 0 },
  filter: {},
  loading: false,
  error: null,
  actionInProgress: {}
};
