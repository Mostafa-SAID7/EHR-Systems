# Appointment Service - Frontend Integration Strategy

## 🎯 EXECUTIVE SUMMARY

The frontend has basic appointment components and pages, but they are **MOCK-ONLY** with NO real backend integration. We need to connect the frontend to the newly reorganized Appointment microservice (100% validated, production-ready backend).

### Current State
- ✅ Basic UI components exist
- ✅ Pages and routes defined
- ❌ Service uses mock data only (NO API calls)
- ❌ No state management (NgRx store empty)
- ❌ No real backend integration
- ❌ No appointment workflow (confirm, cancel, check-in, complete)
- ❌ No availability/slots management

---

## 📊 FRONTEND CURRENT INVENTORY

### Components (3 files)
```
✓ appointment-notes-card.component
✓ appointment-schedule-table.component
✓ appointment-vitals-card.component
```

### Pages (3 files)
```
✓ appointment-list-page (day view calendar)
✓ appointment-schedule-page (booking form)
✓ appointment-detail-page (view/edit details)
```

### Services (1 file - MOCK)
```
✓ appointment.service.ts (mock data, NO HTTP calls)
```

### Routes (3 routes)
```
✓ '' → appointment-list-page
✓ 'schedule' → appointment-schedule-page
✓ ':id' → appointment-detail-page
```

### Store (EMPTY - needs creation)
```
❌ NO NgRx store
❌ NO state management
❌ NO selectors
❌ NO effects
```

### Models (EMPTY - needs alignment)
```
❌ Frontend interface mismatch with backend DTOs
```

---

## 🔗 BACKEND API ENDPOINTS AVAILABLE

### Appointments Management
```
POST   /api/v1/appointments                      → Schedule new appointment
GET    /api/v1/appointments/{id}                → Get appointment details
GET    /api/v1/appointments/patient/{patientId} → Get patient's appointments
GET    /api/v1/appointments/by-type/{type}      → Get appointments by type
POST   /api/v1/appointments/{id}/confirm        → Confirm appointment
POST   /api/v1/appointments/{id}/cancel         → Cancel appointment
POST   /api/v1/appointments/{id}/check-in       → Check in to appointment
POST   /api/v1/appointments/{id}/complete       → Complete appointment
```

### Provider Availability
```
GET    /api/v1/provider-availability/slots      → Get availability slots
POST   /api/v1/provider-availability/set        → Set provider availability
```

### Health
```
GET    /api/v1/appointments/health              → Service health check
```

---

## 📋 BACKEND DTOs/MODELS

### Request DTOs
```typescript
// Schedule Appointment
ScheduleAppointmentCommand {
  patientId: UUID
  providerId: UUID
  scheduledStart: DateTime
  durationMinutes: number
  appointmentType: 'Office' | 'Telehealth' | 'Phone'
  reasonForVisit?: string
  notes?: string
}

// Cancel Appointment
CancelAppointmentCommand {
  appointmentId: UUID
  reason: string
}

// Confirm/CheckIn/Complete Appointment
{
  appointmentId: UUID
}

// Set Provider Availability
SetProviderAvailabilityCommand {
  providerId: UUID
  slotStart: DateTime
  slotEnd: DateTime
  isRecurring: boolean
  recurrencePattern?: string
  maxAppointmentsPerSlot?: number
}
```

### Response DTOs
```typescript
// Appointment Response
AppointmentResponseDto {
  id: UUID
  patientId: UUID
  providerId: UUID
  scheduledStart: DateTime
  scheduledEnd: DateTime
  appointmentType: 'Office' | 'Telehealth' | 'Phone'
  status: 'Scheduled' | 'Confirmed' | 'InProgress' | 'Completed' | 'Cancelled'
  reasonForVisit?: string
  notes?: string
  reminders: AppointmentReminderDto[]
  createdAt: DateTime
  updatedAt: DateTime
}

// Appointment Detailed Response
AppointmentDetailedResponseDto {
  ...AppointmentResponseDto
  patientDetails: PatientDto
  providerDetails: ProviderDto
  duration: number
  reminderSent: boolean
  confirmedAt?: DateTime
  cancelledAt?: DateTime
}

// Provider Availability
ProviderAvailabilityDto {
  id: UUID
  providerId: UUID
  slotStart: DateTime
  slotEnd: DateTime
  isRecurring: boolean
  recurrencePattern?: string
  maxAppointmentsPerSlot: number
  currentBookings: number
  isActive: boolean
}
```

---

## 🔨 REQUIRED FRONTEND CHANGES

### Priority 1: Service Layer Integration (CRITICAL)

**File:** `frontend/src/app/features/appointments/services/appointment.service.ts`

**Changes Needed:**
```typescript
// Replace mock data with HTTP calls
getAppointments(patientId: UUID): Observable<AppointmentResponseDto[]>
  → GET /api/v1/appointments/patient/{patientId}

getAppointmentById(id: UUID): Observable<AppointmentDetailedResponseDto>
  → GET /api/v1/appointments/{id}

scheduleAppointment(cmd: ScheduleAppointmentCommand): Observable<AppointmentResponseDto>
  → POST /api/v1/appointments

cancelAppointment(id: UUID, reason: string): Observable<void>
  → POST /api/v1/appointments/{id}/cancel?reason={reason}

confirmAppointment(id: UUID): Observable<void>
  → POST /api/v1/appointments/{id}/confirm

checkInAppointment(id: UUID): Observable<void>
  → POST /api/v1/appointments/{id}/check-in

completeAppointment(id: UUID): Observable<void>
  → POST /api/v1/appointments/{id}/complete

getAvailableSlots(providerId: UUID, date: Date): Observable<ProviderAvailabilityDto[]>
  → GET /api/v1/provider-availability/slots?providerId={providerId}&date={date}

getAppointmentsByType(type: string, page: number = 1): Observable<PagedResult<AppointmentResponseDto>>
  → GET /api/v1/appointments/by-type/{type}?pageNumber={page}
```

**Current Issues:**
- All methods return mock `of()` Observable
- No error handling
- No loading states
- No validation
- No auth headers

---

### Priority 2: Models/Interfaces Alignment

**File:** `frontend/src/app/features/appointments/models/appointment.model.ts` (CREATE NEW)

**Needed Models:**
```typescript
// Must match backend DTOs exactly
export interface Appointment extends AppointmentResponseDto {}
export interface AppointmentDetailed extends AppointmentDetailedResponseDto {}
export interface ProviderAvailability extends ProviderAvailabilityDto {}

// Request models
export interface ScheduleAppointmentRequest extends ScheduleAppointmentCommand {}
export interface CancelAppointmentRequest { appointmentId: UUID; reason: string; }
export interface ConfirmAppointmentRequest { appointmentId: UUID; }

// Query filters
export interface AppointmentFilter {
  patientId?: UUID;
  providerId?: UUID;
  startDate?: DateTime;
  endDate?: DateTime;
  status?: AppointmentStatus;
  type?: AppointmentType;
  pageNumber?: number;
  pageSize?: number;
}

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
```

---

### Priority 3: State Management (NgRx Store)

**Files to Create:**
```
✓ store/appointment.state.ts
✓ store/appointment.actions.ts
✓ store/appointment.reducer.ts
✓ store/appointment.effects.ts
✓ store/appointment.selectors.ts
```

**Store Structure:**
```typescript
interface AppointmentState {
  appointments: Appointment[];
  selectedAppointment: AppointmentDetailed | null;
  availableSlots: ProviderAvailability[];
  
  loading: boolean;
  error: string | null;
  
  filter: AppointmentFilter;
  paging: {
    pageNumber: number;
    pageSize: number;
    total: number;
  };
}

// Actions needed
@Effect() loadAppointments$
@Effect() loadAppointmentById$
@Effect() scheduleAppointment$
@Effect() cancelAppointment$
@Effect() confirmAppointment$
@Effect() checkInAppointment$
@Effect() completeAppointment$
@Effect() loadAvailableSlots$

// Selectors needed
selectAppointments$
selectSelectedAppointment$
selectAvailableSlots$
selectLoading$
selectError$
selectAppointmentsByStatus$
```

---

### Priority 4: Component Updates

**Schedule Page Component:**
```typescript
// appointment-schedule-page.component.ts

// ADD:
- Call service.getAvailableSlots() when provider/date changes
- Validate appointment can be scheduled
- Submit to service.scheduleAppointment()
- Handle success/error responses
- Show loading state
- Redirect to list on success

// Current: Mock form with hardcoded data
// Needed: Real appointment scheduling workflow
```

**List Page Component:**
```typescript
// appointment-list-page.component.ts

// ADD:
- Load appointments from backend (store)
- Display real appointment data
- Show appointment status (Scheduled/Confirmed/Completed/Cancelled)
- Add action buttons (Confirm, Cancel, CheckIn, Complete)
- Handle status transitions
- Show loading/error states

// Current: Mock calendar view
// Needed: Real data with status management
```

**Detail Page Component:**
```typescript
// appointment-detail-page.component.ts

// ADD:
- Load full appointment details from backend
- Display appointment timeline (Scheduled→Confirmed→InProgress→Completed)
- Show reminders list
- Add action buttons based on current status
  - If Scheduled: Cancel, Confirm buttons
  - If Confirmed: Cancel, CheckIn buttons
  - If InProgress: Complete button
  - If Completed/Cancelled: View only

// Current: Empty placeholder
// Needed: Full workflow implementation
```

---

### Priority 5: New Components Needed

**Provider Availability Calendar**
```
✓ provider-availability-calendar.component
  - Show provider's available slots
  - Allow user to select time slot
  - Display availability status
```

**Appointment Status Timeline**
```
✓ appointment-status-timeline.component
  - Show workflow: Scheduled → Confirmed → InProgress → Completed
  - Show dates/times for each status change
  - Show who confirmed/cancelled/completed
```

**Appointment Actions Bar**
```
✓ appointment-actions.component
  - Show context-aware action buttons
  - Confirm, Cancel, CheckIn, Complete based on status
  - Show loading state during action
```

**Reminders Manager**
```
✓ appointment-reminders.component
  - List appointment reminders
  - Show reminder type (Email, SMS)
  - Show reminder status (Sent, Pending)
```

---

## 📈 INTEGRATION WORKFLOW

### Phase 1: Setup (Week 1)
- [ ] Create appointment models matching backend DTOs
- [ ] Create store (state, actions, reducer, selectors)
- [ ] Update appointment.service.ts with HTTP calls
- [ ] Setup interceptor for authorization headers

### Phase 2: List View (Week 1-2)
- [ ] Implement appointment-list-page
- [ ] Add real data loading
- [ ] Add status filtering
- [ ] Add pagination

### Phase 3: Scheduling (Week 2)
- [ ] Create provider availability calendar component
- [ ] Implement appointment-schedule-page
- [ ] Add availability checking
- [ ] Add form validation

### Phase 4: Detail & Actions (Week 2-3)
- [ ] Implement appointment-detail-page
- [ ] Create appointment-status-timeline component
- [ ] Create appointment-actions component
- [ ] Implement all status transitions (Confirm, Cancel, CheckIn, Complete)

### Phase 5: Polish (Week 3)
- [ ] Add error handling
- [ ] Add loading states
- [ ] Add toast notifications
- [ ] Add confirmation dialogs
- [ ] Add responsive design
- [ ] Unit tests

---

## 🚀 QUICK START CHECKLIST

### Step 1: Models (1-2 hours)
```bash
# Create new file
frontend/src/app/features/appointments/models/appointment.model.ts

# Add all interfaces matching backend DTOs
```

### Step 2: Service Integration (2-3 hours)
```bash
# Update existing file
frontend/src/app/features/appointments/services/appointment.service.ts

# Replace all mock implementations with HTTP calls
# Add error handling
# Add loading states
```

### Step 3: Store Setup (2-3 hours)
```bash
# Create store directory
frontend/src/app/features/appointments/store/

# Create 5 files:
# - state.ts
# - actions.ts
# - reducer.ts
# - effects.ts
# - selectors.ts
```

### Step 4: Update Schedule Page (2-3 hours)
```bash
# Update component
frontend/src/app/features/appointments/pages/appointment-schedule-page/

# Connect to service/store
# Add availability loading
# Add form submission
```

### Step 5: Update List Page (2-3 hours)
```bash
# Update component
frontend/src/app/features/appointments/pages/appointment-list-page/

# Connect to store
# Load real appointments
# Add action buttons
```

### Step 6: Update Detail Page (2-3 hours)
```bash
# Update component
frontend/src/app/features/appointments/pages/appointment-detail-page/

# Connect to store
# Load appointment details
# Implement status actions
```

---

## ✅ SUCCESS CRITERIA

- [ ] All HTTP calls use real backend endpoints
- [ ] Appointments list shows real data from backend
- [ ] Can schedule new appointment (POST /api/v1/appointments)
- [ ] Can confirm appointment (POST /api/v1/appointments/{id}/confirm)
- [ ] Can cancel appointment (POST /api/v1/appointments/{id}/cancel)
- [ ] Can check-in to appointment (POST /api/v1/appointments/{id}/check-in)
- [ ] Can complete appointment (POST /api/v1/appointments/{id}/complete)
- [ ] Can view provider availability slots
- [ ] State management works correctly (NgRx store)
- [ ] Loading states show during API calls
- [ ] Error states handled with user messages
- [ ] No mock data - all real backend data
- [ ] Responsive design works on mobile
- [ ] Unit tests pass (>80% coverage)

---

## 📞 BACKEND API DOCUMENTATION

**Base URL:** `http://localhost:5000/api/v1`

**Endpoints:**

### Appointments
- `POST /appointments` - Schedule new appointment
- `GET /appointments/{id}` - Get appointment details
- `GET /appointments/patient/{patientId}` - Get patient's appointments
- `GET /appointments/by-type/{type}` - Filter by type
- `POST /appointments/{id}/confirm` - Confirm appointment
- `POST /appointments/{id}/cancel` - Cancel appointment
- `POST /appointments/{id}/check-in` - Check in
- `POST /appointments/{id}/complete` - Complete

### Provider Availability
- `GET /provider-availability/slots` - Get available slots
- `POST /provider-availability/set` - Set provider availability

---

**Status: READY FOR IMPLEMENTATION**

This is a comprehensive integration plan connecting the production-ready Appointment microservice to the frontend.

