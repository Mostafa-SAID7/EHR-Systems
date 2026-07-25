# Technical Interview Preparation

Deep technical materials for system design and architecture interviews at TachyHealth.

## Contents

- **System-Design.md** - Medical coding system design walkthrough
- **Patterns.md** - CQRS, Event Sourcing, Outbox, Saga patterns
- **Architecture.md** - Microservices design principles
- **Scenarios.md** - Real technical scenarios and solutions

## What to Expect

TachyHealth will likely ask:

**System Design Questions:**
- "Design a medical coding automation system"
- "How would you build revenue cycle management?"
- "Design for 10x growth"

**Architecture Questions:**
- "How would you structure our backend services?"
- "How do you handle data consistency across services?"
- "What patterns do you use?"

**Technical Depth:**
- "Tell me about your microservices experience"
- "How do you handle failures?"
- "What's your approach to database optimization?"

## Key Technical Areas

### 1. System Design
- Medical coding system (core product)
- Revenue cycle management (adjacent)
- Handling scale and failure

### 2. Backend Patterns
- CQRS (separate read/write concerns)
- Event sourcing (event-driven architecture)
- Outbox pattern (transactional guarantees)
- Saga pattern (distributed transactions)

### 3. Microservices
- Service boundaries
- Communication patterns
- Operational complexity
- Deployment strategy

### 4. Data & Databases
- Query optimization
- Caching strategies
- Consistency models
- Scaling approaches

## Interview Flow - Technical

**Typical Interview Structure:**

1. **Clarifying Questions (5 min)**
   - Ask about scale, requirements, constraints
   - Shows you think like architect

2. **High-Level Design (10 min)**
   - Propose architecture at high level
   - Services, communication, data

3. **Deep Dive (15 min)**
   - Pick one area to deep dive
   - Design decisions with reasoning
   - Tradeoffs explained

4. **Scaling Discussion (10 min)**
   - How to handle 10x growth
   - Database, caching, services

5. **Failure Handling (5 min)**
   - What happens when things break
   - Monitoring, alerting, recovery

6. **Questions (5 min)**
   - Your questions for them
   - Shows genuine interest

## Key Concepts to Know

### CQRS Pattern
**What:** Separate command (write) and query (read) models

**Why:**
- Reads and writes have different requirements
- Reads optimized for speed (denormalized)
- Writes optimized for consistency (normalized)

**Example:**
- Command: Create appointment (normalized database)
- Query: List appointments by date (denormalized read model)

### Event Sourcing
**What:** Store events instead of current state

**Why:**
- Complete audit trail
- Can replay history
- Enables event-driven communication

**Example:**
```
Instead of: appointment.status = "scheduled"
Store: AppointmentScheduledEvent { date, provider, patient, ... }
```

### Outbox Pattern
**What:** Store events in same transaction as data

**Why:**
- Ensures events published if data changes
- No lost events (transactional guarantee)
- Reliable event publishing

### Saga Pattern
**What:** Distributed transaction coordination via events

**Why:**
- Services don't have direct transactions
- Handles failure at service boundaries
- Compensating transactions for rollback

## Interview Tips

**During Interview:**

✅ **DO:**
- Ask clarifying questions (what's scale? latency?)
- Start at high level (big picture first)
- Think out loud (show your thinking)
- Explain tradeoffs (why this not that?)
- Mention monitoring/operations
- Acknowledge constraints
- Ask follow-up questions

❌ **DON'T:**
- Assume scale (ask about requirements)
- Jump to implementation (design first)
- Over-engineer early (ask before adding complexity)
- Ignore operations (monitoring matters)
- Claim perfect consistency (think about tradeoffs)
- Forget about healthcare requirements

## Medical Coding System Design - Quick Overview

**Key Components:**

```
┌──────────────────────────────────────┐
│ Hospital Charting System             │
└──────────────────────────────────────┘
              ↓
┌──────────────────────────────────────┐
│ API Gateway (rate limiting)          │
└──────────────────────────────────────┘
              ↓
┌──────────────────────────────────────┐
│ Coding Service                       │
│ - Routing, caching                   │
└──────────────────────────────────────┘
              ↓
┌──────────────────────────────────────┐
│ ML Model Service | Rule Engine       │
└──────────────────────────────────────┘
              ↓
┌──────────────────────────────────────┐
│ Data Layer (Postgres, Redis, Audit)  │
└──────────────────────────────────────┘
```

**Key Decisions:**
- Cache similar visits (80/20 rule)
- ML model with fallback
- Confidence scoring (transparency)
- Audit logging (compliance)
- Fault tolerance (circuit breakers)

## Your Background Advantage

**You Have:**
- Microservices experience (real, not theory)
- Healthcare domain knowledge (understand compliance)
- Full-stack perspective (see bottlenecks anywhere)
- ASP.NET Core expertise (aligns with tech stack)
- CQRS/Event Sourcing experience (matches their needs)

**Use This To:**
- Show experience with real systems
- Demonstrate healthcare thinking
- Explain tradeoffs from experience
- Build confidence (you know this stuff)

## Preparation Checklist

- [ ] Understand CQRS pattern deeply
- [ ] Understand Event Sourcing + events
- [ ] Know Outbox pattern (transactional events)
- [ ] Know Saga pattern (distributed transactions)
- [ ] Practice system design conversation
- [ ] Have examples of real systems you've built
- [ ] Prepare for scale questions
- [ ] Understand ML model serving
- [ ] Think about healthcare compliance
- [ ] Prepare 3-5 technical questions

## Quick Start

**If you have 30 minutes:**
1. Read System-Design.md (medical coding system)
2. Read Patterns.md (key patterns overview)
3. Practice explaining one pattern out loud

**If you have 60 minutes:**
1. Read all four files
2. Practice designing medical coding system (conversation)
3. Practice failure scenarios

**If you have 2+ hours:**
1. Read everything
2. Practice full interview (Scenarios.md)
3. Prepare your questions
4. Anticipate follow-ups

## Success Metrics

**During Interview, You'll Demonstrate:**

✅ Architectural thinking (not just coding)  
✅ Healthcare domain awareness  
✅ Microservices expertise (with tradeoffs)  
✅ Full-stack perspective  
✅ Operational thinking (monitoring, reliability)  
✅ Thoughtful questions  

**Result:**
- Interviewer thinks "This person knows their stuff"
- Clear technical depth
- Healthcare + enterprise experience
- Ready to contribute immediately

