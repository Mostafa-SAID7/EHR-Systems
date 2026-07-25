# Story 2: Failure & Recovery - Cache Invalidation Bug

**Best For:** "Tell me about a time you failed", "Overcame adversity", "What did you learn?"  
**Time:** 5 minutes  
**Key Skill:** Humility, problem-solving, domain awareness, prevention thinking

---

## SITUATION

You were optimizing performance for patient search in healthcare system. Doctors were frustrated with slow search times when looking up patient records during patient visits.

**Context:**
- Patient search taking 2-3 seconds (poor UX in busy clinic)
- Doctors complained: "System is slowing me down"
- Database queries inefficient (N+1 queries loading related data)
- Need to maintain HIPAA compliance during optimization

---

## TASK

Improve patient search performance while maintaining HIPAA compliance and, critically, data accuracy.

---

## ACTION

### Phase 1: Diagnosis & Optimization

**Profiled the queries:**
- Found N+1 problem: searching for patient, then loading appointments, billing, medical history separately
- Each patient search triggered 5-10 additional queries
- Added indexes: helped but not enough (still 1.5-2s)

**Optimization Strategy:**
1. Added caching layer (Redis) for patient searches
2. Set TTL (Time To Live): **1 hour** (seemed reasonable for "stable" data)
3. Cached full patient records with related data
4. **Result: 2s → 200ms** ✓ Performance improved dramatically
5. Cache hit rate: 80%+ (coders search same patients repeatedly)

**Team Celebration:**
- Deployment successful
- Testing showed good performance  
- Cache hit rate exceeded expectations
- Team happy with improvement

### Phase 2: The Problem Emerges

**Real-world scenario:**
```
1. 9:00 AM - Doctor sees patient in clinic
   - Enters visit notes in system
   - Prescribes new medication

2. System updates prescription in database (done)

3. Pharmacy checks patient record
   - System still returns cached result from 8:00 AM
   - Cached prescription: old medication (not new one)
   - **SERIOUS**: Pharmacy thinks patient still on old medication

4. 4:00 PM - Patient arrives at pharmacy
   - Pharmacist has old medication in system
   - Patient received wrong medication
   - **POTENTIAL PATIENT HARM**
```

**Impact:**
- Medication error in healthcare context = serious
- If patient had allergy to old medication or conflict with new one = dangerous
- Compliance violation (patient safety affected)
- Trust erosion (hospital can't trust our system for medication data)

### Phase 3: Recovery

**Immediate Action (within 1 hour):**
1. Identified the bug (cache TTL too long for medication data)
2. Deployed emergency patch:
   - Reduced cache TTL from 1 hour to 5 minutes
   - Added explicit cache invalidation on prescription updates
   - Monitored for similar issues

**Root Cause Analysis:**
- **Root cause**: I optimized for performance without understanding domain freshness requirements
- **Assumption I made**: "Patient data is stable" (wrong for healthcare)
- **What I missed**: Some data (medications) changes frequently and must be current
- **Why it happened**: Didn't involve clinical staff in optimization decisions

**Proper Solution (week 2):**

1. **Worked with clinical team** to understand freshness requirements:
   - Medications: Must be current (cache invalidate on change)
   - Patient demographics: 1 hour TTL acceptable
   - Appointments: 15-minute TTL
   - Medical history: 1 hour TTL (less frequently updated)

2. **Implemented intelligent caching:**
   - Different TTL by data type
   - Automatic invalidation on writes (medications, prescriptions)
   - Business logic: "On medication update, invalidate patient cache"
   - Monitoring: cache age, staleness events, hit ratios by data type

3. **Added safeguards:**
   - Cache miss fallthrough to database (never return stale critical data)
   - Feature flag to disable caching (emergency override)
   - Alerts if cache stale > threshold (e.g., medication cache > 1 min old)
   - Added checks: "Can we serve this from cache?" per data type

4. **Monitoring & Alerts:**
   - Dashboard showing cache performance by domain
   - Alert if medication cache age > 1 minute
   - Metrics: hit rate, miss rate, age distribution
   - Alerts for unusual patterns (cache always stale = problem)

5. **Process Improvements:**
   - Defined checklist before optimization: "What data freshness do we need?"
   - Added to code review: "Have you considered freshness requirements?"
   - Involved domain experts (clinical staff) before optimization decisions
   - Added cache strategy to architecture documentation
   - Team training: "How to optimize without breaking healthcare requirements"

**Result of proper solution:**
- Performance: 200ms response time maintained ✓
- Reliability: No more stale medication data issues ✓
- Scalability: Caching still provides major improvement ✓
- Process: Team now includes freshness requirements in optimization planning ✓

---

## RESULT

### Immediate Outcomes
- **Fixed**: Cache invalidation works properly
- **Prevented**: No actual patient harm (caught before serious incident)
- **Learned**: Domain requirements drive technical decisions

### Quantified Improvements
| Metric | Before Fix | After Fix |
|--------|-----------|-----------|
| Response time | 200ms | 250ms (slight increase, acceptable) |
| Cache hit rate | 80% | 75% (slightly lower, safer) |
| Stale data incidents | 1 per month | 0 (prevented) |
| Cache-related alerts | 0 | [monitoring enables proactive response] |

### Process Improvements
- ✅ Checklist created for optimization decisions
- ✅ Clinical team involved in architecture decisions
- ✅ Monitoring built in from start
- ✅ Alert thresholds defined
- ✅ Team trained on domain requirements

---

## LEARNING

### What I Learned

**1. Healthcare is Different**
- In typical SaaS, eventual consistency is acceptable
- In healthcare, some data must be current
- Optimization has domain implications

**2. Always Involve Domain Experts**
- I assumed "patient data is stable" without validating
- Clinical staff would have immediately said "medications must be current"
- Save time + prevent errors by involving them early

**3. Optimization Without Understanding = Risk**
- Performance optimization can break functionality
- Must understand requirements before optimizing
- "Fast but wrong" is worse than "slow but correct" in healthcare

**4. Monitoring Prevents Recurrence**
- Added monitoring to catch similar issues
- Alerts on stale data enable proactive response
- Observability is non-negotiable in production

### What I'd Do Differently

1. **Before optimization**: Ask domain experts about freshness requirements
2. **During optimization**: Document assumptions about data freshness
3. **After optimization**: Monitor for data staleness, not just cache hits
4. **Process**: Add freshness requirement to every optimization checklist

### Key Insight

Technical decisions have domain consequences. In healthcare, correctness beats speed. Optimization without understanding domain requirements is dangerous.

---

## How to Tell This Story

### Opening
"I was optimizing patient search performance and made a decision that seemed right technically but had domain consequences."

### The Problem
"Patient search was taking 2-3 seconds. I added caching, got it down to 200ms. Great performance improvement."

### The Mistake
"I set cache TTL to 1 hour for all patient data. But medication data changes frequently - doctors prescribe new medications during clinic visits."

### The Consequence
"A patient's medication updated in morning, but pharmacy saw cached version from hours earlier. Almost gave patient wrong medication."

### Recovery
"We immediately reduced TTL, added invalidation, and deployed a fix. But the real fix was involving clinical staff in the decision."

### Learning
"I learned that in healthcare, correctness matters more than speed. Performance optimization has domain implications. Always involve domain experts."

---

## Follow-up Questions to Ask

After telling this story:
- "How do you approach optimization in regulated environments?"
- "What's your monitoring strategy?"
- "How do you validate data freshness requirements?"
- "Tell me about a time you prevented a similar issue"

---

## Why This Story Works for TachyHealth

- **Relevant**: Medical coding system has similar freshness requirements
- **Shows Humility**: You admit mistake, didn't blame others
- **Shows Learning**: Changed process, not just quick fix
- **Healthcare Awareness**: Understand compliance/data accuracy implications
- **Practical Thinking**: Involved domain experts, added monitoring

---

## Talking Points

**If they ask "What did you learn?"**
- "In healthcare, correctness > speed"
- "Always understand domain requirements before optimizing"
- "Involve domain experts in technical decisions"
- "Monitor for domain implications, not just performance"

**If they ask "How would you prevent this?"**
- "Checklist: What freshness does this data need?"
- "Monitoring: Alert if data stale beyond threshold"
- "Process: Clinical team reviews optimization decisions"
- "Communication: Document assumptions about data"

