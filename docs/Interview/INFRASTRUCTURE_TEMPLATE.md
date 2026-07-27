# Infrastructure Project Documentation Template

This template provides a structured way to document complex infrastructure/platform features for interviews. Use this template when creating documentation for a new major component.

---

## Folder Structure

Create a new folder with this naming convention: `NN_COMPONENT_NAME`

Example:
- `01_TAG_INFRASTRUCTURE` ✓
- `02_AUDIT_LOGGING` (next project)
- `03_CACHING_STRATEGY` (future project)

### Inside Your Component Folder

```
02_YOUR_COMPONENT/
├── INDEX.md                 ← Start here (navigation)
├── BENEFITS.md              ← Business value & metrics
├── CRITICAL_POINTS.md       ← Design decisions & trade-offs
├── ARCHITECTURE.md          ← System design & components
├── FIXES.md                 ← Improvements made
├── BUGS.md                  ← Known issues & workarounds
├── EXAMPLES.md              (optional) Usage examples
└── ROADMAP.md               (optional) Future plans
```

---

## File Descriptions & Templates

### 1. INDEX.md
**Purpose**: Navigation hub and interview prep guide  
**Length**: 2-3 pages  
**Audience**: Everyone (executives to developers)

**Template Structure**:
- Quick navigation table (5 docs, purpose, length)
- Document purposes (what, who, questions answered)
- Interview scenarios (3-4 realistic questions with talking points)
- Quick stats reference (key metrics)
- Creation checklist
- Tips for interview success

**Why it matters**: Helps anyone quickly understand the component and prepare talking points

---

### 2. BENEFITS.md
**Purpose**: Articulate business & technical value  
**Length**: 8-12 pages  
**Audience**: Managers, PMs, hiring teams, executives

**Template Structure**:

```markdown
# [Component] - Benefits

## Executive Summary
[2-3 sentence overview of value delivered]

---

## Business Benefits

### 1. [Benefit Name]
**Before**: [Old way]
**After**: [New way]
**Impact**: [Quantified results]

### 2. [Next Benefit]
...

---

## Technical Benefits

### 1. [Design Pattern Used]
**Problem**: [What was hard]
**Solution**: [How we solved it]
**Benefits**: [Concrete outcomes]

---

## Performance Benefits

### 1. [Performance Metric]
**Before**: [Old numbers]
**After**: [New numbers]
**Improvement**: [X% or Xex faster]

---

## Developer Experience Benefits

### 1. [DX Improvement]
**How it helps**: [Specific developer pain point solved]
**Example**: [Code snippet]

---

## Business Value Summary
[Table comparing benefits with ROI impact]

---

## Quote-Worthy Benefits
[3-4 concise statements suitable for emails/talks]
```

**Key Metrics to Include**:
- Lines of code saved/reduced
- Performance improvements (% or multiplier)
- Development time reduction
- Scalability improvements
- Compliance/audit benefits

---

### 3. CRITICAL_POINTS.md
**Purpose**: Design decisions and trade-offs  
**Length**: 6-10 pages  
**Audience**: Architects, tech leads, curious developers

**Template Structure**:

```markdown
# [Component] - Critical Points

## Key Decisions

### Decision #1: [What Choice Was Made]

**Question**: [What problem needed solving?]

**Options Considered**:
1. [Option A]: Pros/Cons
2. [Option B]: Pros/Cons
3. [Option C]: Pros/Cons

**Decision**: [What we chose and why]

**Implications**: [What this enables/constrains]

---

### Decision #2: [Next Key Decision]
...

---

## Important Trade-Offs

### Trade-Off #1: [Soft Delete vs Hard Delete]

**What we gave up**: [Cost]

**What we gained**: [Benefit]

**Why**: [Reasoning]

**When it matters**: [Use cases]

---

## Known Limitations

### Limitation #1: [What can't it do?]

**Impact**: [What breaks?]

**Workaround**: [How to work around it]

**Future plan**: [When will this be fixed?]

---

## Edge Cases & Gotchas

### Gotcha #1: [Common mistake]

**What happens**: [The problem]

**Why**: [Root cause]

**How to avoid**: [Prevention]

---

## Performance Considerations

### Bottleneck #1: [What's slow?]

**Current**: [Baseline]

**Optimization opportunity**: [What could be better]

**Priority**: [High/Medium/Low]

---

## Scaling Scenarios

### Scenario: [When you have X amount of data]

**Behavior**: [What happens]

**Recommendations**: [What to do]
```

**Key Sections**:
- Why each key decision was made (not just what)
- Trade-offs with pros/cons
- Honest limitations
- Gotchas that developers need to know
- Performance characteristics at scale

---

### 4. ARCHITECTURE.md
**Purpose**: System design and technical deep-dive  
**Length**: 12-15 pages  
**Audience**: Developers, architects, technical interviewers

**Template Structure**:

```markdown
# [Component] - Architecture & Design

## System Overview

[ASCII diagram showing component and how it connects to rest of system]

---

## Core Components

### 1. [Main Interface/Abstraction]
**Location**: [File path]
**Purpose**: [What does it do?]
**Key Methods**: [List methods]

### 2. [Main Implementation]
**Location**: [File path]
**Responsibility**: [What's its job?]
**Dependencies**: [What does it need?]

### 3. [Supporting Component]
...

---

## Design Patterns Used

### Pattern #1: [Pattern Name]
**Why**: [Problem it solves]
**How**: [How it's implemented]
**Benefits**: [What you gain]

---

## Data Model

### Entity #1: [Entity Name]
```csharp
// Show entity structure
```

### Entity #2: [Next Entity]
...

---

## CQRS/Event Sourcing (if applicable)

### Commands
- [Command A]
- [Command B]

### Queries
- [Query A]
- [Query B]

### Events
- [Event A]
- [Event B]

---

## Data Flow Diagrams

### Operation #1: [Operation Name]

[ASCII flow diagram showing request → processing → response]

### Operation #2: [Next Operation]
...

---

## Dependency Injection

[Show DI registration code]

---

## Database Schema

[Show key tables, relationships, indexes]

---

## Error Handling

[Show exception hierarchy]

---

## Extension Points

### How to add feature X
[Step-by-step guide]

### How to add new variant
[Step-by-step guide]
```

**Key Sections**:
- Clear visual diagrams
- Entity/object structure
- Data flow for common operations
- DI setup
- Error handling strategy
- How to extend

---

### 5. FIXES.md
**Purpose**: Real improvements made and lessons learned  
**Length**: 8-12 pages  
**Audience**: Developers, QA, future maintainers

**Template Structure**:

```markdown
# [Component] - Fixes & Improvements

## Recent Fixes (v1.X+)

### Fix #1: [Problem Solved]

**Date**: [When]
**Affected Versions**: [< 1.X]
**Impact**: [What was broken]

#### Problem
[Describe the issue]

#### Solution Implemented
[Show code before/after]

#### Performance Improvement
[Quantify the improvement]

#### Files Modified
[List files and changes]

#### Testing Added
[What tests were added]

---

### Fix #2: [Next Fix]
...

---

## Improvement Log

### Q[X] 2024 Improvements
- [x] Completed improvement #1
- [x] Completed improvement #2
- [ ] Planned improvement #1

---

## Migration Guide

### For users of [component]

```bash
# How to upgrade
# Breaking changes (if any)
# Verification steps
```

---

## Known Limitations (Post-Fix)

[Table of remaining issues with workarounds]
```

**Key Sections**:
- Real, specific improvements (not generic claims)
- Code examples showing before/after
- Quantified performance gains
- How to upgrade
- What limitations remain

---

### 6. BUGS.md
**Purpose**: Known issues, regression prevention, QA reference  
**Length**: 6-10 pages  
**Audience**: QA, Support, future developers, hiring team

**Template Structure**:

```markdown
# [Component] - Known Bugs & Issues

## Active Bugs

### Bug #1: [Title]

**Severity**: Critical | High | Medium | Low
**Status**: New | Investigating | In Progress | Blocked | Resolved
**Reported**: [Date]

#### Description
[What is the bug?]

#### Root Cause
[Why does it happen?]

#### Steps to Reproduce
[How to see it?]

#### Impact
[What breaks?]

#### Proposed Fix
[How to fix it?]

---

### Bug #2: [Next Bug]
...

---

## Performance Issues

### Issue: [Title]
**Impact**: [What happens?]
**Fix Applied**: [When was it fixed?]

---

## Regression Risks

### Risk: [What could break?]
- **Impact**: [What breaks?]
- **Mitigation**: [How to prevent?]

---

## Test Coverage Gaps

[Table showing areas with low test coverage]

---

## Bug Tracking Template

For future bugs:
[Template to copy]
```

**Key Sections**:
- Specific bugs with severity/status
- How to reproduce
- Root cause analysis
- Proposed fixes
- Regression risks
- Test coverage gaps

---

## Usage Guidelines

### When Creating New Infrastructure Docs

1. **Start with INDEX.md**
   - Define the component
   - Plan your talking points
   - List key metrics

2. **Write BENEFITS.md**
   - Lead with business value
   - Quantify everything
   - Include killer quotes

3. **Write CRITICAL_POINTS.md**
   - Explain design decisions
   - Be honest about trade-offs
   - Show you thought it through

4. **Write ARCHITECTURE.md**
   - Provide technical foundation
   - Include diagrams
   - Show extension points

5. **As you work, maintain FIXES.md**
   - Document improvements
   - Track lessons learned
   - Show evolution

6. **As you discover issues, maintain BUGS.md**
   - Log known issues
   - Prioritize fixes
   - Track regressions

7. **Update INDEX.md**
   - Add interview scenarios
   - Include final stats
   - Create quick reference

### For Interview Prep

1. Read your INDEX.md
2. Review the talking points
3. Internalize key metrics
4. Practice explaining decisions
5. Be ready for deep-dive questions

### For Code Review/Onboarding

1. New team members read ARCHITECTURE.md
2. They check BUGS.md for gotchas
3. They review FIXES.md for patterns
4. They reference CRITICAL_POINTS.md for design context

---

## Example: Completed Template

See `01_TAG_INFRASTRUCTURE/` folder for a completed example with all 6 documents fully populated.

---

## Notes

- Keep documents focused and scannable
- Use metrics and data, not opinions
- Be honest about limitations
- Update regularly as the component evolves
- Use these for actual interviews (they're not just documentation)

