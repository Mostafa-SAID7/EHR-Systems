# Phase 2 Enhancements - Appointment Service

**Status:** PLANNED - Ready for Implementation  
**Timeline:** Month 2-3  
**Priority:** Medium (Enhancing core functionality)

---

## 🎯 Enhancement 1: Appointment Reminders

### Backend Implementation

**Domain Model Extension:**
```csharp
public class AppointmentReminder : AuditableEntity
{
    public Guid AppointmentId { get; set; }
    public ReminderType ReminderType { get; set; } // Email, SMS, Push
    public DateTime ReminderTime { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public string? FailureReason { get; set; }
    public int RetryCount { get; set; }
}
```

**Commands:**
- `ScheduleReminderCommand` - Schedule reminder for appointment
- `SendReminderCommand` - Manually send reminder
- `CancelReminderCommand` - Cancel pending reminder

**Queries:**
- `GetAppointmentRemindersQuery` - Get all reminders for appointment
- `GetPendingRemindersQuery` - Get reminders ready to send

**Scheduled Job:**
```csharp
// ReminderBackgroundService.cs
public class ReminderBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Every 5 minutes, check for pending reminders
            var pendingReminders = await _mediator.Send(
                new GetPendingRemindersQuery(), 
                stoppingToken
            );
            
            foreach (var reminder in pendingReminders)
            {
                await _mediator.Send(
                    new SendReminderCommand { ReminderId = reminder.Id },
                    stoppingToken
                );
            }
            
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

### Frontend Implementation

**Models:**
```typescript
export interface AppointmentReminder {
  id: string;
  appointmentId: string;
  reminderType: ReminderType;
  reminderTime: Date;
  isSent: boolean;
  sentAt?: Date;
}

export interface ScheduleReminderRequest {
  appointmentId: string;
  reminderType: ReminderType;
  minutesBefore: number; // 30, 60, 1440 (1 day)
}
```

**Store Actions:**
```typescript
export const scheduleReminder = createAction(
  '[Appointments] Schedule Reminder',
  props<{ request: ScheduleReminderRequest }>()
);

export const sendReminder = createAction(
  '[Appointments] Send Reminder',
  props<{ reminderId: string }>()
);

export const getAppointmentReminders = createAction(
  '[Appointments] Get Appointment Reminders',
  props<{ appointmentId: string }>()
);
```

**Service Methods:**
```typescript
scheduleReminder(request: ScheduleReminderRequest): Observable<AppointmentReminder>
sendReminder(reminderId: string): Observable<void>
getAppointmentReminders(appointmentId: string): Observable<AppointmentReminder[]>
```

**Component UI:**
```typescript
<!-- Reminder scheduling card -->
<div class="card">
  <h3>Set Reminders</h3>
  <div class="reminder-options">
    <button (click)="scheduleReminder(30, ReminderType.Email)">
      Email 30 min before
    </button>
    <button (click)="scheduleReminder(24*60, ReminderType.SMS)">
      SMS 1 day before
    </button>
  </div>
</div>
```

---

## 🎯 Enhancement 2: Appointment Rescheduling

### Backend Implementation

**Domain Model:**
```csharp
public class AppointmentRescheduleHistory : AuditableEntity
{
    public Guid AppointmentId { get; set; }
    public DateTime OriginalScheduledStart { get; set; }
    public DateTime NewScheduledStart { get; set; }
    public string? Reason { get; set; }
    public Guid RescheduleInitiatedBy { get; set; }
}
```

**Commands:**
- `RescheduleAppointmentCommand` - Reschedule to new time
- `GetAvailableRescheduleSlots` - Check availability
- `AcceptRescheduleCommand` - Accept reschedule proposal

**Implementation:**
```csharp
public class RescheduleAppointmentCommand : IRequest<AppointmentResponseDto>
{
    public Guid AppointmentId { get; set; }
    public DateTime NewScheduledStart { get; set; }
    public int DurationMinutes { get; set; }
    public string? Reason { get; set; }
}

public class RescheduleAppointmentCommandHandler : IRequestHandler<RescheduleAppointmentCommand>
{
    public async Task<AppointmentResponseDto> Handle(
        RescheduleAppointmentCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate new slot availability
        var isAvailable = await _availabilityService.CheckSlot(
            appointment.ProviderId,
            request.NewScheduledStart,
            request.DurationMinutes
        );
        
        // 2. Save reschedule history
        var history = new AppointmentRescheduleHistory
        {
            AppointmentId = request.AppointmentId,
            OriginalScheduledStart = appointment.ScheduledStart,
            NewScheduledStart = request.NewScheduledStart,
            Reason = request.Reason
        };
        
        // 3. Update appointment
        appointment.ScheduledStart = request.NewScheduledStart;
        appointment.ScheduledEnd = request.NewScheduledStart.AddMinutes(request.DurationMinutes);
        appointment.Status = AppointmentStatus.Rescheduled;
        
        // 4. Publish event
        appointment.RaiseDomainEvent(
            new AppointmentRescheduleEvent(appointment.Id, history)
        );
        
        return _mapper.Map<AppointmentResponseDto>(appointment);
    }
}
```

### Frontend Implementation

**Store:**
```typescript
export const rescheduleAppointment = createAction(
  '[Appointments] Reschedule Appointment',
  props<{ appointmentId: string; newStartTime: Date; reason?: string }>()
);

export const getAvailableRescheduleSlots = createAction(
  '[Appointments] Get Available Reschedule Slots',
  props<{ appointmentId: string; fromDate: Date; toDate: Date }>()
);
```

**Component:**
```typescript
// Reschedule dialog
openRescheduleDialog(appointmentId: string) {
  const dialogRef = this.dialog.open(RescheduleDialogComponent, {
    data: { appointmentId }
  });

  dialogRef.afterClosed().subscribe(result => {
    if (result) {
      this.store.dispatch(rescheduleAppointment({
        appointmentId,
        newStartTime: result.newTime,
        reason: result.reason
      }));
    }
  });
}
```

---

## 🎯 Enhancement 3: Appointment Notes/Comments

### Backend Implementation

**Domain Model:**
```csharp
public class AppointmentNote : AuditableEntity
{
    public Guid AppointmentId { get; set; }
    public string Content { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsPrivate { get; set; } // Provider-only or visible to patient
}
```

**Commands:**
- `AddAppointmentNoteCommand` - Add new note
- `UpdateAppointmentNoteCommand` - Update note
- `DeleteAppointmentNoteCommand` - Delete note

**Implementation:**
```csharp
public class AddAppointmentNoteCommand : IRequest<AppointmentNoteDto>
{
    public Guid AppointmentId { get; set; }
    public string Content { get; set; }
    public bool IsPrivate { get; set; }
}

public async Task<IActionResult> AddNote(
    Guid appointmentId,
    [FromBody] AddAppointmentNoteCommand command)
{
    command.AppointmentId = appointmentId;
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

### Frontend Implementation

**Models:**
```typescript
export interface AppointmentNote {
  id: string;
  appointmentId: string;
  content: string;
  authorName: string;
  createdAt: Date;
  updatedAt?: Date;
  isPrivate: boolean;
}
```

**Store:**
```typescript
export const addAppointmentNote = createAction(
  '[Appointments] Add Note',
  props<{ appointmentId: string; content: string; isPrivate: boolean }>()
);

export const getAppointmentNotes = createAction(
  '[Appointments] Get Notes',
  props<{ appointmentId: string }>()
);
```

**Component UI:**
```typescript
<div class="notes-section">
  <div class="note-list">
    <div *ngFor="let note of (notes$ | async)" class="note-card">
      <p class="note-content">{{ note.content }}</p>
      <small class="note-meta">
        {{ note.authorName }} • {{ note.createdAt | date }}
        <span *ngIf="note.isPrivate" class="badge-private">Private</span>
      </small>
    </div>
  </div>
  
  <form [formGroup]="noteForm" (ngSubmit)="addNote()">
    <textarea formControlName="content" placeholder="Add a note..."></textarea>
    <label>
      <input type="checkbox" formControlName="isPrivate"/>
      Private note
    </label>
    <button type="submit">Add Note</button>
  </form>
</div>
```

---

## 🎯 Enhancement 4: Real-Time Updates (SignalR)

### Backend Setup

**SignalR Hub:**
```csharp
public class AppointmentHub : Hub
{
    [Authorize]
    public async Task JoinAppointmentGroup(string appointmentId)
    {
        await Groups.AddToGroupAsync(Connection.ConnectionId, $"appointment-{appointmentId}");
    }
    
    public async Task LeaveAppointmentGroup(string appointmentId)
    {
        await Groups.RemoveFromGroupAsync(Connection.ConnectionId, $"appointment-{appointmentId}");
    }
}
```

**Broadcasting Updates:**
```csharp
// After appointment status change
await _hubContext.Clients.Group($"appointment-{appointmentId}")
    .SendAsync("AppointmentUpdated", appointmentDto);

// When note is added
await _hubContext.Clients.Group($"appointment-{appointmentId}")
    .SendAsync("NoteAdded", noteDto);

// When reminder is sent
await _hubContext.Clients.Group($"appointment-{appointmentId}")
    .SendAsync("ReminderSent", reminderDto);
```

**Program.cs:**
```csharp
services.AddSignalR();
app.MapHub<AppointmentHub>("/hubs/appointments");
```

### Frontend Implementation

**Service:**
```typescript
@Injectable()
export class AppointmentRealtimeService {
  private hubConnection?: HubConnection;
  
  connect(): Observable<void> {
    return new Observable(observer => {
      this.hubConnection = new HubConnectionBuilder()
        .withUrl(`${environment.wsUrl}/hubs/appointments`)
        .withAutomaticReconnect()
        .build();
      
      this.hubConnection.start()
        .then(() => observer.next())
        .catch(err => observer.error(err));
    });
  }
  
  onAppointmentUpdated(): Observable<AppointmentResponseDto> {
    return new Observable(observer => {
      this.hubConnection?.on('AppointmentUpdated', 
        (apt: AppointmentResponseDto) => observer.next(apt)
      );
    });
  }
  
  onNoteAdded(): Observable<AppointmentNote> {
    return new Observable(observer => {
      this.hubConnection?.on('NoteAdded', 
        (note: AppointmentNote) => observer.next(note)
      );
    });
  }
  
  joinAppointment(appointmentId: string): Promise<void> {
    return this.hubConnection?.invoke('JoinAppointmentGroup', appointmentId)
      ?? Promise.reject('Not connected');
  }
}
```

**Store Effects:**
```typescript
@Injectable()
export class AppointmentRealtimeEffects {
  appointmentUpdatedFromServer$ = createEffect(() =>
    this.realtime.onAppointmentUpdated().pipe(
      map(apt => updateAppointmentFromServer({ appointment: apt }))
    )
  );
  
  noteAddedFromServer$ = createEffect(() =>
    this.realtime.onNoteAdded().pipe(
      map(note => addNoteFromServer({ note }))
    )
  );
}
```

**Component Integration:**
```typescript
ngOnInit() {
  // Join realtime updates
  this.store.dispatch(joinAppointmentRealtimeGroup({ 
    appointmentId: this.appointmentId 
  }));
  
  // Subscribe to updates
  this.realtime.onAppointmentUpdated().subscribe(apt => {
    // Auto-update UI
  });
}
```

---

## 📊 Implementation Timeline

| Feature | Sprint | Duration | Complexity |
|---|---|---|---|
| Appointment Reminders | Sprint 1 | 2 weeks | Medium |
| Rescheduling | Sprint 1-2 | 2-3 weeks | High |
| Notes/Comments | Sprint 2 | 1-2 weeks | Low |
| Real-Time Updates | Sprint 2-3 | 2-3 weeks | High |

**Total Timeline:** 6-8 weeks

---

## 🎯 Testing Strategy

### Unit Tests
- Reminder scheduling logic
- Reschedule availability checking
- Note creation/updates
- SignalR event handlers

### Integration Tests
- End-to-end reminder flow
- Reschedule workflow
- Note persistence
- Real-time sync

### E2E Tests
- User schedules reminder
- Reminder sends at scheduled time
- User initiates reschedule
- Notes sync across connected clients

---

## Dependencies

- ✅ Notification Service (for reminders)
- ✅ Background Jobs (for reminder scheduler)
- ✅ SignalR (for real-time)
- ✅ Audit Service (for note history)

