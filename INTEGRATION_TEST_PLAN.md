# Appointment Service - Integration Test Plan

**Status:** READY FOR EXECUTION  
**Date:** July 28, 2026  
**Scope:** Full end-to-end appointment workflows

---

## Test Suite Overview

### Test Categories
1. **API Integration Tests** (11 tests)
2. **State Management Tests** (8 tests)
3. **End-to-End Workflows** (6 tests)
4. **Data Validation Tests** (5 tests)
5. **Error Scenario Tests** (7 tests)
6. **Performance Tests** (4 tests)

**Total Tests:** 41  
**Coverage Target:** 95%+

---

## 1. API Integration Tests

### Test 1.1: Schedule Appointment
```typescript
describe('Schedule Appointment', () => {
  it('should schedule new appointment with valid data', async () => {
    // Arrange
    const request: ScheduleAppointmentRequest = {
      patientId: 'patient-1',
      providerId: 'provider-1',
      scheduledStart: new Date('2026-08-01T14:00:00Z'),
      durationMinutes: 30,
      appointmentType: AppointmentType.Office,
      reasonForVisit: 'Annual checkup',
      notes: 'Patient has allergies'
    };

    // Act
    const response = await service.scheduleAppointment(request).toPromise();

    // Assert
    expect(response).toBeDefined();
    expect(response?.id).toBeTruthy();
    expect(response?.status).toBe(AppointmentStatus.Scheduled);
    expect(response?.patientId).toBe('patient-1');
    expect(response?.scheduledStart).toEqual(request.scheduledStart);
  });

  it('should return 400 for invalid duration', async () => {
    // Arrange
    const request: ScheduleAppointmentRequest = {
      patientId: 'patient-1',
      providerId: 'provider-1',
      scheduledStart: new Date('2026-08-01T14:00:00Z'),
      durationMinutes: 0, // Invalid
      appointmentType: AppointmentType.Office,
      reasonForVisit: 'Test'
    };

    // Act & Assert
    try {
      await service.scheduleAppointment(request).toPromise();
      fail('Should have thrown error');
    } catch (error: any) {
      expect(error.status).toBe(400);
    }
  });
});
```

### Test 1.2: Get Appointment by ID
```typescript
it('should retrieve appointment by ID', async () => {
  // Arrange
  const appointmentId = 'apt-123';

  // Act
  const response = await service.getAppointmentById(appointmentId).toPromise();

  // Assert
  expect(response).toBeDefined();
  expect(response?.id).toBe(appointmentId);
  expect(response?.patientName).toBeTruthy();
  expect(response?.providerName).toBeTruthy();
});

it('should return 404 for non-existent appointment', async () => {
  // Arrange
  const appointmentId = 'non-existent';

  // Act & Assert
  try {
    await service.getAppointmentById(appointmentId).toPromise();
    fail('Should have thrown 404');
  } catch (error: any) {
    expect(error.status).toBe(404);
  }
});
```

### Test 1.3: Get Patient Appointments with Pagination
```typescript
it('should retrieve paginated appointments for patient', async () => {
  // Arrange
  const patientId = 'patient-1';
  const filter: AppointmentFilter = {
    pageNumber: 1,
    pageSize: 20,
    startDate: new Date('2026-07-01'),
    endDate: new Date('2026-08-31')
  };

  // Act
  const response = await service.getPatientAppointments(patientId, filter).toPromise();

  // Assert
  expect(response).toBeDefined();
  expect(response?.items.length).toBeGreaterThan(0);
  expect(response?.totalCount).toBeGreaterThan(0);
  expect(response?.pageNumber).toBe(1);
  expect(response?.hasNextPage).toBeDefined();
});

it('should handle empty results gracefully', async () => {
  // Arrange
  const patientId = 'patient-no-apt';
  const filter: AppointmentFilter = { pageNumber: 1, pageSize: 20 };

  // Act
  const response = await service.getPatientAppointments(patientId, filter).toPromise();

  // Assert
  expect(response?.items.length).toBe(0);
  expect(response?.totalCount).toBe(0);
});
```

### Test 1.4: Confirm Appointment
```typescript
it('should confirm scheduled appointment', async () => {
  // Arrange
  const appointmentId = 'apt-scheduled';

  // Act
  await service.confirmAppointment(appointmentId).toPromise();
  const result = await service.getAppointmentById(appointmentId).toPromise();

  // Assert
  expect(result?.status).toBe(AppointmentStatus.Confirmed);
  expect(result?.confirmedAt).toBeTruthy();
});

it('should fail to confirm already confirmed appointment', async () => {
  // Arrange
  const appointmentId = 'apt-confirmed';

  // Act & Assert
  try {
    await service.confirmAppointment(appointmentId).toPromise();
    fail('Should have thrown error');
  } catch (error: any) {
    expect(error.status).toBe(400);
  }
});
```

### Test 1.5: Cancel Appointment
```typescript
it('should cancel appointment with reason', async () => {
  // Arrange
  const appointmentId = 'apt-scheduled';
  const reason = 'PatientRequested';

  // Act
  await service.cancelAppointment(appointmentId, reason).toPromise();
  const result = await service.getAppointmentById(appointmentId).toPromise();

  // Assert
  expect(result?.status).toBe(AppointmentStatus.Cancelled);
  expect(result?.cancelledAt).toBeTruthy();
  expect(result?.cancelReason).toBe('PatientRequested');
});

it('should fail to cancel completed appointment', async () => {
  // Arrange
  const appointmentId = 'apt-completed';

  // Act & Assert
  try {
    await service.cancelAppointment(appointmentId, 'Other').toPromise();
    fail('Should have thrown error');
  } catch (error: any) {
    expect(error.status).toBe(400);
  }
});
```

### Test 1.6: Check-In Appointment
```typescript
it('should check in confirmed appointment', async () => {
  // Arrange
  const appointmentId = 'apt-confirmed';

  // Act
  await service.checkInAppointment(appointmentId).toPromise();
  const result = await service.getAppointmentById(appointmentId).toPromise();

  // Assert
  expect(result?.status).toBe(AppointmentStatus.InProgress);
});

it('should fail to check in non-confirmed appointment', async () => {
  // Arrange
  const appointmentId = 'apt-scheduled';

  // Act & Assert
  try {
    await service.checkInAppointment(appointmentId).toPromise();
    fail('Should have thrown error');
  } catch (error: any) {
    expect(error.status).toBe(400);
  }
});
```

### Test 1.7: Complete Appointment
```typescript
it('should complete in-progress appointment', async () => {
  // Arrange
  const appointmentId = 'apt-inprogress';

  // Act
  await service.completeAppointment(appointmentId).toPromise();
  const result = await service.getAppointmentById(appointmentId).toPromise();

  // Assert
  expect(result?.status).toBe(AppointmentStatus.Completed);
});
```

### Test 1.8: Get Appointments by Type
```typescript
it('should retrieve appointments filtered by type', async () => {
  // Arrange
  const appointmentType = 'Office';

  // Act
  const response = await service.getAppointmentsByType(appointmentType, 1, 20).toPromise();

  // Assert
  expect(response?.items.every(a => a.appointmentType === AppointmentType.Office)).toBe(true);
});
```

### Test 1.9: Get Available Slots
```typescript
it('should retrieve available provider slots', async () => {
  // Arrange
  const providerId = 'provider-1';
  const fromDate = new Date('2026-08-01');
  const toDate = new Date('2026-08-31');

  // Act
  const slots = await service.getAvailableSlots(providerId, fromDate, toDate).toPromise();

  // Assert
  expect(slots).toBeDefined();
  expect(Array.isArray(slots)).toBe(true);
  expect(slots?.every(s => new Date(s.slotStart) >= fromDate)).toBe(true);
});
```

### Test 1.10: Set Provider Availability
```typescript
it('should set provider availability', async () => {
  // Arrange
  const request: SetProviderAvailabilityRequest = {
    providerId: 'provider-1',
    slotStart: new Date('2026-08-01T08:00:00Z'),
    slotEnd: new Date('2026-08-01T17:00:00Z'),
    isRecurring: true,
    recurrencePattern: 'WEEKDAY',
    maxAppointmentsPerSlot: 4
  };

  // Act
  const result = await service.setProviderAvailability(request).toPromise();

  // Assert
  expect(result).toBeDefined();
  expect(result?.providerId).toBe('provider-1');
  expect(result?.isActive).toBe(true);
});
```

### Test 1.11: Health Check
```typescript
it('should return healthy status', async () => {
  // Act
  const result = await service.healthCheck().toPromise();

  // Assert
  expect(result?.status).toBe('healthy');
});
```

---

## 2. State Management Tests

### Test 2.1: NgRx Actions Dispatch
```typescript
it('should dispatch loadAppointments action', () => {
  // Arrange
  const action = loadAppointments({ patientId: 'patient-1' });

  // Act
  store.dispatch(action);

  // Assert
  expect(action.patientId).toBe('patient-1');
});
```

### Test 2.2: Reducer State Updates
```typescript
it('should update state on loadAppointmentsSuccess', () => {
  // Arrange
  const appointments: AppointmentResponseDto[] = [
    { id: '1', patientId: 'p1', status: AppointmentStatus.Scheduled } as any
  ];
  const action = loadAppointmentsSuccess({ appointments, total: 1 });

  // Act
  const state = appointmentReducer(initialAppointmentState, action);

  // Assert
  expect(state.appointments.length).toBe(1);
  expect(state.paging.total).toBe(1);
  expect(state.loading).toBe(false);
});
```

### Test 2.3: Effects Integration
```typescript
it('should load appointments through effects', (done) => {
  // Arrange
  const action = loadAppointments({ patientId: 'patient-1' });
  const appointments: AppointmentResponseDto[] = [];
  spyOn(service, 'getPatientAppointments').and.returnValue(of({ items: appointments, totalCount: 0 }));

  // Act & Assert
  effects.loadAppointments$.subscribe(result => {
    expect(result.type).toBe('[Appointments] Load Appointments Success');
    done();
  });

  store.dispatch(action);
});
```

### Test 2.4: Selector Tests
```typescript
it('should select scheduled appointments', () => {
  // Arrange
  const state: AppointmentState = {
    appointments: [
      { status: AppointmentStatus.Scheduled } as any,
      { status: AppointmentStatus.Confirmed } as any
    ]
  } as any;

  // Act
  const result = selectScheduledAppointments(state);

  // Assert
  expect(result.length).toBe(1);
});
```

### Test 2.5: Stats Selector
```typescript
it('should calculate appointment statistics', () => {
  // Arrange
  const state: AppointmentState = {
    appointments: [
      { status: AppointmentStatus.Scheduled },
      { status: AppointmentStatus.Confirmed },
      { status: AppointmentStatus.Completed },
      { status: AppointmentStatus.Cancelled },
      { status: AppointmentStatus.NoShow },
      { status: AppointmentStatus.Rescheduled }
    ]
  } as any;

  // Act
  const stats = selectAppointmentStats(state);

  // Assert
  expect(stats.total).toBe(6);
  expect(stats.scheduled).toBe(1);
  expect(stats.noShow).toBe(1);
  expect(stats.rescheduled).toBe(1);
});
```

### Test 2.6-2.8: Additional State Tests
(Similar patterns for error handling, filter updates, action progress tracking)

---

## 3. End-to-End Workflow Tests

### Workflow 1: Complete Appointment Lifecycle
```typescript
describe('Complete Appointment Lifecycle', () => {
  it('Schedule → Confirm → CheckIn → Complete', async () => {
    // 1. Schedule
    const scheduleReq: ScheduleAppointmentRequest = {
      patientId: 'p1', providerId: 'pr1', scheduledStart: new Date(),
      durationMinutes: 30, appointmentType: AppointmentType.Office,
      reasonForVisit: 'Test'
    };
    const scheduled = await service.scheduleAppointment(scheduleReq).toPromise();
    expect(scheduled?.status).toBe(AppointmentStatus.Scheduled);

    // 2. Confirm
    await service.confirmAppointment(scheduled!.id).toPromise();
    let apt = await service.getAppointmentById(scheduled!.id).toPromise();
    expect(apt?.status).toBe(AppointmentStatus.Confirmed);

    // 3. CheckIn
    await service.checkInAppointment(scheduled!.id).toPromise();
    apt = await service.getAppointmentById(scheduled!.id).toPromise();
    expect(apt?.status).toBe(AppointmentStatus.InProgress);

    // 4. Complete
    await service.completeAppointment(scheduled!.id).toPromise();
    apt = await service.getAppointmentById(scheduled!.id).toPromise();
    expect(apt?.status).toBe(AppointmentStatus.Completed);
  });
});
```

### Workflow 2: Appointment Cancellation
```typescript
it('Schedule → Cancel', async () => {
  const scheduled = await service.scheduleAppointment(req).toPromise();
  await service.cancelAppointment(scheduled!.id, 'PatientRequested').toPromise();
  const result = await service.getAppointmentById(scheduled!.id).toPromise();
  expect(result?.status).toBe(AppointmentStatus.Cancelled);
  expect(result?.cancelReason).toBe('PatientRequested');
});
```

### Workflow 3: Provider Availability Management
```typescript
it('Set Availability → Get Slots', async () => {
  const availReq: SetProviderAvailabilityRequest = { /* ... */ };
  const availability = await service.setProviderAvailability(availReq).toPromise();
  expect(availability?.isActive).toBe(true);

  const slots = await service.getAvailableSlots(
    'provider-1', new Date(), new Date()
  ).toPromise();
  expect(slots?.length).toBeGreaterThan(0);
});
```

### Workflows 4-6: Additional Scenarios
(Rescheduling, bulk operations, concurrent access)

---

## 4. Data Validation Tests

### Test 4.1: Date/Time Conversion
```typescript
it('should convert ISO 8601 to Date correctly', () => {
  const isoDate = '2026-08-01T14:30:00Z';
  const apt = { scheduledStart: new Date(isoDate) };
  
  expect(apt.scheduledStart.getFullYear()).toBe(2026);
  expect(apt.scheduledStart.getMonth()).toBe(7); // 0-indexed
});
```

### Test 4.2: Enum Validation
```typescript
it('should only accept valid AppointmentStatus values', () => {
  const validStatuses = Object.values(AppointmentStatus);
  expect(validStatuses).toContain(AppointmentStatus.Scheduled);
  expect(validStatuses).toContain(AppointmentStatus.NoShow);
  expect(validStatuses).toContain(AppointmentStatus.Rescheduled);
});
```

### Test 4.3: CancellationReason Validation
```typescript
it('should accept all CancellationReason enum values', () => {
  const reasons = Object.values(CancellationReason);
  expect(reasons).toContain(CancellationReason.PatientRequested);
  expect(reasons).toContain(CancellationReason.Emergency);
  expect(reasons.length).toBe(8);
});
```

### Tests 4.4-4.5: Pagination and Filter Validation

---

## 5. Error Scenario Tests

### Test 5.1-5.7: Various Error Conditions
- Invalid appointment ID
- Concurrent modifications
- Database connection failure
- Invalid date ranges
- Missing required fields
- Duplicate appointment
- Provider not found

---

## 6. Performance Tests

### Test 6.1: Large Result Set Handling
```typescript
it('should handle 1000+ appointments efficiently', async () => {
  // Create 1000 appointments
  const start = performance.now();
  
  const response = await service.getPatientAppointments('patient-1', {
    pageSize: 100,
    pageNumber: 1
  }).toPromise();
  
  const duration = performance.now() - start;
  expect(duration).toBeLessThan(2000); // Should complete in < 2 seconds
});
```

### Test 6.2: Selector Memoization
```typescript
it('should use memoized selectors efficiently', () => {
  const state = { appointments: [/* large array */] };
  
  const result1 = selectAppointmentStats(state);
  const result2 = selectAppointmentStats(state);
  
  // Should return same reference due to memoization
  expect(result1).toBe(result2);
});
```

### Tests 6.3-6.4: Pagination Performance and Concurrent Operations

---

## Test Execution Strategy

### Phase 1: Unit Tests (Week 1)
- Run all 41 tests locally
- Verify 95%+ code coverage
- Fix any failures

### Phase 2: Integration Tests (Week 2)
- Deploy to staging
- Run full test suite against staging API
- Load testing with 1000+ concurrent users

### Phase 3: E2E Testing (Week 3)
- User workflow testing
- Manual QA verification
- Performance benchmarking

### Phase 4: Production Ready (Week 4)
- Final sign-off
- Deployment checklist
- Production monitoring setup

---

## Success Criteria

✅ All 41 tests passing  
✅ Code coverage > 95%  
✅ Response time < 500ms (p95)  
✅ 99.9% uptime SLA  
✅ Zero critical bugs  
✅ Performance acceptable under load  

