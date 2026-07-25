# Story 3: Collaboration - Cross-Team Alignment

**Best For:** "Working with different perspectives", "Team conflict", "Compromise/negotiation"  
**Time:** 5 minutes  
**Key Skill:** Listening, diplomacy, systems thinking, win-win solutions

---

## SITUATION

Your company had multiple backend services but no clear ownership model. Two camps with different priorities formed:

**Camp A - Backend Team:**
- "We should own all backend services"
- Pro: Consistency, shared learnings, infrastructure expertise
- Con: Slower feature development, bottleneck for product teams

**Camp B - Product Teams:**
- "Each product should own their services end-to-end"
- Pro: Faster development, clear ownership
- Con: Inconsistency, duplicate effort, isolated silos

**Context:**
- Performance degrading (ownership unclear who fixes)
- On-call burden distributed (everyone responsible = nobody responsible)
- Technology choices diverging (different teams using different solutions)
- Tension escalating (meetings getting unproductive)

---

## TASK

As senior engineer, facilitate a decision that would:
1. Work organizationally (both sides could accept)
2. Work technically (good architecture still)
3. Enable speed (product teams unblocked)
4. Maintain consistency (backend team's core concern)
5. Build culture (not destroy team morale)

---

## ACTION

### Phase 1: Understanding Both Perspectives

**Listened to Backend Team:**
```
"We have deep expertise in infrastructure and distributed systems.
If each product team owns their services, we'll have 5 different 
solutions to the same problem.

Consistency matters - coding practices, deployment, monitoring, 
security. We should maintain standards."
```

**Listened to Product Teams:**
```
"We need to ship features faster. Waiting for backend team is 
a blocker. We understand our domain; we should own it.

Fast iteration, fast deployment, direct accountability - that's 
what we need to compete."
```

**Identified Real Concerns (Not just opinions):**

Backend Team's Core Concerns:
- Consistency in patterns, frameworks, deployment
- Operational burden (if everyone does things differently, chaos)
- Quality (no random technology choices)

Product Teams' Core Concerns:
- Velocity (can't wait for someone else)
- Decision-making speed (don't want to ask permission)
- Clear responsibility (know who owns problems)

### Phase 2: Finding Middle Ground

**Proposed Hybrid Model:**
```
PRODUCT TEAMS OWN:
- Business logic
- Domain modeling
- Feature development
- On-call responsibility
- Deployment decisions

INFRASTRUCTURE TEAM PROVIDES:
- Service deployment framework (boilerplate)
- Monitoring/alerting templates (standards)
- Shared libraries (auth, caching, logging)
- Security/compliance patterns
- Database migration strategy
- Runbook templates

GOVERNANCE:
- Architecture review board (cross-functional)
- Tech stack consistency guidelines (not requirements)
- Shared runbooks for common operations
- Architecture decision records (ADRs) for visibility
```

**Why This Works:**

For Backend Team:
- ✓ Consistency (through frameworks and templates)
- ✓ Standards maintained (governance + library approach)
- ✓ Architecture visibility (ADRs, review board)
- ✓ Operational burden managed (good tooling)

For Product Teams:
- ✓ Autonomy (own your services)
- ✓ Velocity (don't wait for approval on every decision)
- ✓ Clear ownership (you own success/failure)
- ✓ Learning opportunity (learn while shipping)

### Phase 3: Building Support

**1. Presented to Both Teams Together**
```
"Here's a model where you both get what matters:

Backend: You get consistency through platforms and governance.
Product: You get autonomy and velocity.

This is win-win. Let's try it."
```

**2. Addressed Concerns Proactively**

Backend Team Question: "Won't we have duplicate solutions?"
Answer: "Not if we own the frameworks and libraries. You're providing the platform; they're building on it."

Product Team Question: "Will we still be blocked?"
Answer: "Review board is lightweight. We're reviewing architecture, not approving every deploy. If your service is consistent with framework, it's approved."

**3. Got Commitment**

Backend Team: "If you provide good frameworks, we'll buy in"
Product Team: "If you don't slow us down, we'll follow your standards"

### Phase 4: Implementation

**Phase 4a: Infrastructure Team Execution**

Built shared platforms:
1. **Service Template** - Boilerplate for new services (done right)
   - Logging setup (centralized, structured)
   - Monitoring setup (metrics, alerts)
   - Security patterns (auth, secrets)
   - Database layer (migrations, connection pooling)

2. **Deployment Pipeline** - One-click deploy
   - Build artifact
   - Run tests
   - Deploy to staging
   - Deploy to production
   - Rollback if needed
   Result: Friction gone. Product teams can deploy without waiting.

3. **Monitoring Template** - Consistent metrics and alerts
   - Golden signals (latency, error rate, saturation)
   - Service-specific metrics
   - Alert definitions
   - Dashboard templates

4. **Logging Aggregation** - Search across services
   - Structured logging
   - Correlation IDs for tracing requests
   - Searchable by service, timestamp, level

**Phase 4b: Product Teams Adoption**

- Started with pilot team (Appointment Service)
- They used template, deployed service in 2 weeks (vs 6 weeks before)
- No blockers from backend
- Rest of teams followed

**Phase 4c: Governance**

- Weekly architecture meeting (30 min)
- Design reviews before implementation (lightweight)
- Retrospectives on decisions (learn from each other)
- ADRs for visibility

---

## RESULT

### Organizational Outcomes

| Aspect | Before | After | Impact |
|--------|--------|-------|--------|
| **Development Speed** | 6 weeks to new service | 2 weeks | 3x faster |
| **Team Autonomy** | Low (wait for approval) | High (own decisions) | Morale improved |
| **Consistency** | Low (everyone doing their thing) | Medium (through templates) | Maintainability improved |
| **On-Call Experience** | Chaos (unclear who fixes what) | Clear (service owner fixes) | Fewer incidents |
| **Knowledge Sharing** | Siloed | Connected (via ADRs, reviews) | Learning accelerated |

### Quantified Metrics

- **Deployment time**: 6 weeks to 2 weeks (new services)
- **Feature velocity**: +40% (teams autonomous)
- **Operational issues**: -30% (clear ownership)
- **Architecture consistency**: +60% (frameworks help)

### Qualitative Improvements

- **Backend Team**: "Frameworks give us leverage. We influenced architecture without being blockers"
- **Product Teams**: "We can own our destiny now"
- **Culture**: Shifted from conflict to partnership
- **Decision Making**: Faster, more collaborative

---

## LEARNING

### What Went Well

1. **Listened to both sides** - Understood real concerns, not just positions
2. **Found actual win-win** - Didn't compromise both into mediocrity
3. **Backend team pivoted** - From "controlling everything" to "providing platforms"
4. **Product teams respected governance** - Understood consistency has value
5. **Tools enabled change** - Good frameworks, dashboards, deployment pipeline

### What Was Challenging

1. **Backend team skepticism** - Took time to convince them this would work
2. **Product team discipline** - Needed some reminders about architecture standards
3. **Governance overhead** - Weekly meetings felt slow to some

### What I'd Do Differently

1. **Sooner framework development** - Build templates before full rollout
2. **Clearer success metrics** - Define what "good" looks like upfront
3. **More communication** - Uncertainty lingered until we saw results

### Key Insight

Best solutions come from understanding all perspectives. The answer wasn't "Backend team is right" or "Product teams are right" - it was recognizing what each actually needed and designing a system that served both.

---

## How to Tell This Story

### Opening
"We had a fundamental disagreement about how to organize our engineering. Backend team wanted centralization, product teams wanted speed. I needed to find a solution that worked for both."

### The Tension
"Backend team had valid concerns about consistency. Product teams had valid concerns about speed. It wasn't about who was right - they both were."

### The Solution
"I proposed a hybrid: Product teams own services, backend team provides platforms. We started with templates, deployment pipeline, monitoring framework."

### The Buy-in
"I had to convince backend team that good frameworks could enforce consistency without being blockers. I had to convince product teams to follow standards."

### The Result
"Deployment time went from 6 weeks to 2 weeks. Autonomy increased, consistency improved. Both sides got what mattered."

---

## Follow-up Questions to Ask

After telling this story:
- "How do you navigate organizational decisions where stakeholders disagree?"
- "Tell me about a time you had to get buy-in from skeptical team"
- "How do you balance autonomy with consistency?"
- "What's your approach to governance?"

---

## Why This Story Works for TachyHealth

- **Relevant**: Series A companies have similar organizational tensions
- **Shows Diplomacy**: You didn't just pick a winner
- **Shows Systems Thinking**: Saw organizational + technical aspects
- **Pragmatic**: Found solution that worked, not perfect solution
- **Leadership**: Influenced without authority

