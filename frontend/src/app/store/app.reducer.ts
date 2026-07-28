/**
 * Root Reducers
 * Combines all feature reducers
 */
import { appointmentReducer } from '../features/appointments/store/appointment.reducer';

export const appReducers = {
  appointments: appointmentReducer
};
