# Phase 2 - Detailed Implementation Guides

**Status:** READY FOR IMPLEMENTATION  
**Sprint:** 6-8 weeks  
**Complexity:** Medium-High

---

## Quick Start Commands

### 1. Reminders Backend
```bash
# Create reminder domain
dotnet new classlib -n Features.Reminders
# Add to appointment service
# Implement ReminderBackgroundService
# Register in Program.cs
```

### 2. Rescheduling Backend
```bash
# Add RescheduleHistory entity
# Create RescheduleAppointmentCommand
# Add reschedule validation
# Update AppointmentsController
```

### 3. Notes Backend
```bash
# Add AppointmentNote entity
# Create AddNoteCommand
# Add NoteController endpoints
# Add audit trail
```

### 4. SignalR Frontend/Backend
```bash
# Install: dotnet add package Microsoft.AspNetCore.SignalR
# Install: npm install @microsoft/signalr
# Create AppointmentHub
# Add SignalR service to frontend
```

---

## Estimated Effort

| Feature | Backend | Frontend | Testing | Total |
|---|---|---|---|---|
| Reminders | 8h | 6h | 4h | 18h |
| Rescheduling | 12h | 10h | 6h | 28h |
| Notes | 6h | 8h | 4h | 18h |
| Real-Time | 10h | 12h | 8h | 30h |
| **Total** | **36h** | **36h** | **22h** | **94h** |

**Timeline:** 6-8 weeks with 1 developer

---

## Success Criteria

✅ All 4 features implemented  
✅ 95%+ test coverage  
✅ Zero data loss on reschedule  
✅ Reminders send within 5 min  
✅ Real-time updates < 500ms  
✅ Backward compatible  

---

## Risks & Mitigations

| Risk | Probability | Impact | Mitigation |
|---|---|---|---|
| Reminder delays | Medium | Medium | Queue monitoring |
| Data loss on reschedule | Low | High | Audit trail + backup |
| Real-time sync issues | Medium | Medium | Fallback polling |
| Performance degradation | Low | Medium | Caching + indexing |

