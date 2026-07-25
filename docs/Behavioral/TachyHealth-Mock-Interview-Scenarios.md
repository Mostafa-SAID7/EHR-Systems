# TachyHealth Mock Interview Scenarios

Practice scenarios based on TachyHealth's actual products and challenges. Use these to prepare for real interviews.

---

## Mock Interview 1: Medical Coding Automation

### Interviewer Introduction
"Hi Mostafa, thanks for joining. We're hiring for a senior backend engineer to work on our medical coding automation service. This service processes visit notes from thousands of hospitals and returns suggested ICD-10 and CPT codes. Let me ask you a few questions."

---

### Question 1: "Tell me about your experience with healthcare systems"

#### What They're Evaluating
- Do you understand healthcare domain complexity?
- Can you communicate across technical and healthcare contexts?
- Are you aware of compliance requirements?

#### Your Answer (Adapted from Your Stories)
```
"I spent the last [X] years building an EHR platform, which gave me deep 
healthcare experience. Here's what I learned:

DOMAIN COMPLEXITY:
The EHR system I worked on had multiple services: Billing, Appointments, 
Audit, Notifications, Identity. What made healthcare different:

1. Correctness is non-negotiable
   - In typical SaaS, slow data sync is annoying
   - In healthcare, incorrect data has patient impact
   - This influenced every architectural decision

2. Compliance is baked in, not bolted on
   - HIPAA requires audit trails for every operation
   - We implemented OutboxEvent pattern to guarantee event publishing
   - Every change tracked, auditable, reversible

3. Domain expertise matters
   - I worked with billing teams to understand revenue cycle
   - Learned ICD-10 and CPT basics
   - Understood why medical coding accuracy directly impacts hospital revenue
   - This context helped me design better systems

DIRECTLY RELEVANT TO YOUR SYSTEM:
Your medical coding automation is solving a real pain point I saw:
- Coders manually assign codes to visits
- It's expensive, error-prone, and repetitive
- Automating this is genuinely valuable

I'm excited about this because I understand both:
- The technical challenge (ML model serving, accuracy vs speed)
- The domain challenge (coding rules are complex, accuracy matters)
"
```

**Why This Works:**
- Specific examples from your actual experience
- Shows you understand healthcare is different
- Demonstrates domain learning
- Connects your background to their problem

---

### Question 2: "How would you design the coding service for scale?"

#### What They're Evaluating
- Can you think through architectural decisions?
- Do you understand the tension between accuracy and performance?
- Can you handle healthcare-specific constraints?

#### Your Answer
```
"Let me clarify requirements first:

CLARIFYING QUESTIONS:
- Scale: How many hospitals, visits per day? (You mentioned 100 hospitals, 1M visits/month?)
- Latency: Is this real-time suggestion during charting, or batch coding?
- Accuracy: What's acceptable accuracy? What happens if suggestion is wrong?
- Geographic: Are we serving MENA only or global?

[After they answer: 100 hospitals, real-time, 99%+ accuracy, MENA focus]

ARCHITECTURAL APPROACH:

┌─────────────────────────────────────────────────┐
│ Hospital Charting System                        │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│ API Gateway (Rate limiting per hospital)        │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│ Coding Service (Orchestrator)                   │
│ - Request routing                               │
│ - Cache hit/miss                                │
│ - Concurrency control                           │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│ Parallel Processing:                            │
│                                                 │
│ ┌──────────┬──────────┬──────────┐             │
│ │ ML Model │ Rule     │ Feedback │             │
│ │ Service  │ Engine   │ Engine   │             │
│ └──────────┴──────────┴──────────┘             │
│                                                 │
│ Each returns suggestions with confidence       │
│ Service merges results, scores, returns        │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│ Data Layer:                                     │
│ - Redis: Cache suggestions, model versions      │
│ - Postgres: Visit codes, coder feedback         │
│ - Audit: Every coding decision logged           │
└─────────────────────────────────────────────────┘

KEY DESIGN DECISIONS:

1. CACHING FOR PERFORMANCE
   Problem: Can't run ML model on every request (2-3s latency)
   Solution: Similar visits likely get similar codes
   
   Implementation:
   - Hash visit characteristics (chief complaint, age, gender)
   - Check cache: "Have we coded similar visit recently?"
   - Hit: Return cached codes (instant)
   - Miss: Run full pipeline
   
   Why: Healthcare has Zipfian distribution
   - 80% of visits are common presentations
   - 20% unusual
   - Cache handles majority fast

2. ML MODEL OPTIMIZATION
   Problem: ML model inference takes 500-1000ms
   Solution: Multiple approaches:
   
   A. Faster model
      - Use distilled model (smaller, faster)
      - Acceptable accuracy tradeoff
      - Still confident on common codes
   
   B. Parallel suggestions
      - Get ML suggestion
      - Get rule-based suggestion
      - Get feedback from coder history
      - Merge in parallel (don't add latency)
   
   C. Async fallback
      - Return rule-based suggestion immediately (100ms)
      - ML suggestion in background
      - Push update when ready (WebSocket)
      - Coder gets fast response + better suggestion later

3. CONFIDENCE SCORING
   Problem: How does coder know if suggestion trustworthy?
   Solution: Confidence score
   
   Implementation:
   - Model returns confidence (0-100)
   - If confidence < 70: Mark as low-confidence
   - Coder reviews carefully
   - If confidence > 95: Can auto-accept (optional)
   
   Why: Transparent about model uncertainty
   Healthcare needs to know when to trust automation

4. HANDLING FAILURES
   Problem: ML model down, database slow, network issues
   Solution: Graceful degradation
   
   ML down:
   - Fallback to rule engine
   - Still gives suggestions, less smart
   - Alert team to fix ML
   
   Database slow:
   - Cache result in memory
   - Async write to database
   - No data loss (in-memory backup)
   
   Network issues:
   - Return last known good result
   - Mark as cached, not live
   - Retry in background

5. AUDIT & COMPLIANCE
   Problem: HIPAA requires audit trail
   Solution: Every action logged
   
   What we log:
   - Visit sent for coding
   - Suggestions returned
   - Coder action (accepted/rejected/modified)
   - Final codes submitted
   - Any corrections later
   
   Why: Hospital needs audit trail, compliance verification

PERFORMANCE TARGETS:
- p50: 200ms (cache hit mostly)
- p95: 500ms (includes ML inference)
- p99: 2s (includes retries, failures)
- Goal: 99%+ suggestion accuracy

SCALING PATH:
- Phase 1: Single service (< 10 hospitals)
- Phase 2: Cache optimization (10-50 hospitals)
- Phase 3: Model optimization (50-200 hospitals)
- Phase 4: Distributed inference (200+ hospitals)
"
```

**Why This Works:**
- Shows you understand performance tradeoffs
- Mentions healthcare compliance naturally
- Acknowledges real challenges (ML latency, failure handling)
- Provides concrete solutions

---

### Question 3: "What would concern you most about this system?"

#### What They're Evaluating
- Do you think critically?
- Do you ask good clarifying questions?
- Can you balance multiple concerns?

#### Your Answer
```
"Great question. Several concerns:

1. MODEL ACCURACY & BIAS
   My biggest concern: Is the model trained on representative data?
   
   Real risk:
   - If trained mostly on US hospital data, will it work in MENA?
   - Different patient populations, different coding practices
   - Different equipment availability, medication choices
   
   Impact:
   - Wrong codes = wrong billing = hospital loses revenue
   - Worse: Consistent bias toward certain diagnoses
   
   How to mitigate:
   - Start with pilot in one region
   - Measure accuracy by region, by provider type
   - Have human review before production deployment
   - Implement feedback loop: coder corrections improve model
   - Monitor for geographic bias

2. CODER OVER-RELIANCE
   Risk: Coders become over-reliant on suggestions
   
   What could happen:
   - Coders accept suggestions without review
   - Fewer catches of wrong codes
   - System error amplified (lots of wrong codes)
   
   Mitigations:
   - Confidence scoring: Only high-confidence auto-accept
   - Audit reviews: Random sample of coder decisions
   - Mandatory review % (at least 20% must review carefully)
   - Alerts: Flag unusual patterns (coder accepting 100% = concerning)

3. DATA FRESHNESS & RULES
   Risk: Medical coding rules change frequently
   
   What could happen:
   - New code released (ICD-10 updates)
   - System still suggests old code
   - Or suggests codes that no longer apply
   
   Mitigations:
   - Subscribe to coding updates
   - Version rules explicitly
   - Monitor for rule deprecation warnings
   - Have process to update quickly

4. HOSPITAL-SPECIFIC RULES
   Risk: Each hospital has slightly different practices
   
   What could happen:
   - Hospital A codes aggressively (more revenue)
   - Hospital B codes conservatively (less risk)
   - System suggestions offend one hospital's practices
   
   Mitigations:
   - Customer-specific models
   - or customer-specific rules override
   - Allow customization without forking system

5. EXPLAINABILITY
   Risk: Coders can't understand why model suggested code
   
   What could happen:
   - Coder rejects good suggestion because unexplained
   - Coder accepts bad suggestion because model is 'magic'
   
   Mitigation:
   - Show reasoning: 'This visit mentions chest pain + 
     positive EKG → Suggested ICD-10 I21.2 (STEMI)'
   - Link to similar visit examples
   - Show confidence: 'Model 87% confident'

How I'd prioritize:
1. Model accuracy (most important - business critical)
2. Explainability (enables adoption)
3. Customization (enables multiple customer types)
4. Bias detection (ethical + business risk)
"
```

**Why This Works:**
- Shows you think deeply about implications
- Demonstrates domain knowledge
- Balances technical and business concerns
- Pragmatic approach (prioritization)

---

### Question 4: "How would you measure success?"

#### Your Answer
```
"Several metrics depending on stakeholder:

FOR HOSPITAL (Business):
- Coding time per visit: Reduce from 10 min to 3-5 min
- Coder productivity: More visits coded per day
- Revenue impact: Fewer missed codes = more revenue
- Compliance: Accuracy meets audit requirements

FOR TACHYHEALTH (Product):
- Adoption: % of visits using automation
- Accuracy: % of suggestions accepted without change
- Performance: p95 < 500ms latency
- Cost: Inference cost per suggestion

FOR PATIENTS (Ethical):
- Coding accuracy: Wrong codes shouldn't impact care
- Billing accuracy: Bills reflect actual services
- Privacy: HIPAA compliance maintained

IMPLEMENTATION:
Dashboard tracking:
- Real-time: Latency, accuracy, error rates
- Daily: Adoption %, coding time reduction
- Weekly: Cost per suggestion, model performance by region
- Monthly: Customer satisfaction, adoption growth

Red flags to monitor:
- Latency creeping up (model getting slower?)
- Accuracy dropping (model drift?)
- Adoption flat (customers don't trust system?)
- Cost rising (scaling poorly?)

A/B testing:
- Test new model versions on 10% of customers first
- Measure accuracy improvement before rollout
- Canary deployment: Easy to roll back if issues
"
```

---

## Mock Interview 2: Revenue Cycle Management

### Interviewer Introduction
"Now let's talk about broader revenue cycle challenges. You'll work on a system that manages the entire journey from patient visit to payment received. Walk me through how you'd approach this."

---

### Question 1: "Describe the data flow through revenue cycle"

#### Your Answer
```
"Good question. Let me walk through the flow:

PATIENT VISIT (t=0)
├─ Patient comes to hospital
├─ Doctor documents visit (notes, diagnosis, treatment)
├─ System captures:
│  ├─ Patient demographics
│  ├─ Visit details (chief complaint, procedures)
│  └─ Provider information

MEDICAL CODING (t=1, same day or next)
├─ Coding service suggests ICD-10, CPT codes
├─ Coder reviews and finalizes codes
├─ System stores codes + coder ID + timestamp

CLAIM GENERATION (t=2, within 24h ideally)
├─ Validation: Do codes + patient + provider combination make sense?
├─ Insurance determination: Which insurance responsible?
├─ Charge master lookup: What's the price for these procedures?
├─ Claim assembly: Create claim document
├─ Quality check: Any missing info? Any obvious errors?

CLAIM SUBMISSION (t=3, 24-72h)
├─ Route to correct insurance
├─ Submit via insurance API or EDI
├─ Record submission timestamp
├─ Get reference number from insurance

CLAIM TRACKING (t=4, ongoing weeks to months)
├─ Poll insurance for status updates
├─ Handle responses:
│  ├─ Approved → Next step: Send to payment processing
│  ├─ Denied → Queue for review team, prepare appeal
│  ├─ Pending → Continue polling

PAYMENT PROCESSING (t=5, weeks after approval)
├─ Insurance sends payment
├─ Identify which claim payment covers
├─ Post payment to accounts receivable
├─ Match expected vs actual
│  ├─ If payment less than expected: Why? (Patient deductible, plan limits)
│  ├─ If payment more than expected: Error? (Unusual)

RECONCILIATION & FOLLOW-UP (t=6+)
├─ Reconcile claimed amount vs received
├─ Identify shortfalls
├─ Determine next action:
│  ├─ Patient bill for remaining (if applicable)
│  ├─ Write off (contractual adjustment)
│  ├─ Resubmit (if denied incorrectly)
├─ Generate reports for finance team

AUDIT & COMPLIANCE
├─ Every step logged with timestamp + actor
├─ Financial reconciliation
├─ Fraud detection (unusual patterns?)

TIMELINE:
t=0 (Visit): Immediate
t=1 (Coding): 1-2 days
t=2 (Claim Gen): 1-2 days
t=3 (Submit): 3-7 days (batch processing)
t=4 (Track): 1-8 weeks (depends on insurance)
t=5 (Payment): 1-4 weeks after approval
t=6 (Reconcile): Ongoing

KEY CHALLENGES:
- Different insurance companies: Different APIs, rules
- Claim denials: 10-30% initial denial rate
- Payment mismatches: Insurance pays less than expected
- Timing: Long cycle means cash flow impact
- Compliance: Must track everything for audit
"
```

---

### Question 2: "How would you handle claim denials?"

#### Your Answer
```
"Claim denials are 15-30% of claims initially. This is a huge operational burden.

DENIAL REASONS:
Common denials:
1. Coding: Code invalid for diagnosis
2. Medical necessity: Insurance doesn't cover this service
3. Pre-auth: Needed pre-authorization (not obtained)
4. Duplicate: Looks like duplicate of previous claim
5. Documentation: Missing required documentation

HANDLING DENIALS:

Pipeline:
Denied Claim
   ↓
[Categorize denial reason]
   ├─ If coding error → Fix codes, resubmit
   ├─ If documentation missing → Collect docs, resubmit
   ├─ If medical necessity → Review with provider, may not resubmit
   ├─ If pre-auth → Request pre-auth if possible
   └─ If valid denial → Write off

AUTOMATION:
Some denials can be auto-categorized:
- Coding errors: Obvious (code not valid)
- Duplicates: Check against previous submission
- Documentation: Missing fields can be identified

But most need human review:
- Medical necessity decisions: Provider + coder review
- Complex pre-auth: Obtain documentation, submit appeal

Alert thresholds:
- If denial rate > 20% for provider: Alert
- If denial rate > 30% for specific code: Alert
- If denial never successfully appeals: Flag

WORKFLOW:
1. Denial received from insurance
2. Auto-categorize if possible
3. Route to appropriate team:
   - Coding errors → Billing team
   - Pre-auth → Admin team
   - Medical necessity → Clinical review team
4. Team takes action (resubmit, appeal, write-off)
5. Track outcome

SYSTEM REQUIREMENTS:
- Denial tracking database
- Rules engine for auto-categorization
- Workflow routing
- Appeal history (some appeals succeed eventually)
- Metrics: Denial rate, appeal success rate
"
```

---

### Question 3: "What are potential security/compliance issues?"

#### Your Answer
```
"Revenue cycle deals with sensitive data + financial transactions. Multiple risks:

SECURITY RISKS:

1. PII Exposure
   Claim includes: Patient name, SSN, DOB, MRN, address
   Risk: Breach = HIPAA violation = huge penalties + liability
   
   Mitigation:
   - Encrypt at rest (AES-256)
   - Encrypt in transit (TLS 1.3)
   - Minimize who can see full PII
   - Audit access logs

2. Financial Data Tampering
   Risk: Bad actor modifies claim amounts to steal
   
   Mitigation:
   - Change audit trail (immutable append-only log)
   - Digital signatures on claims
   - Access control (who can modify claims?)
   - Segregation of duties (coder codes, biller bills)

3. Insurance Integration
   Risk: Attackers intercept API calls to insurance
   
   Mitigation:
   - Mutual TLS between systems
   - Rate limiting (detect unusual submission volumes)
   - IP whitelisting where possible
   - Insurance connection monitoring

COMPLIANCE RISKS:

1. HIPAA
   Requirement: Any use/disclosure of PHI must be documented
   
   Implementation:
   - Log every access to claim data
   - Attribute action to user (who accessed when)
   - Retention: Keep logs for audit
   - Ability to answer: 'Who accessed patient X data on date Y?'

2. Financial Reporting
   Requirement: Revenue cycle must be accurate for financial statements
   
   Implementation:
   - Claim reconciliation
   - Discrepancy investigation
   - Proper accounting (recognize revenue when, how)
   - Audit trail for financial review

3. Insurance Fraud Prevention
   Requirement: Hospital responsible for submitting accurate claims
   
   Implementation:
   - Coding accuracy checks
   - Duplicate detection
   - Unusual pattern detection (unusual for provider)
   - Medical necessity review

CONTROLS I'D IMPLEMENT:
- Encryption at rest & in transit
- Access controls (role-based)
- Audit logging (immutable)
- Regular security audits
- Incident response plan
- Business continuity (backups, recovery)
"
```

---

## Mock Interview 3: Rapid Fire Technical Questions

### Question 1: "Design for 10x growth"
```
Current: 100 hospitals, 1M visits/month
Target: 1000 hospitals, 10M visits/month

Key changes:

Database:
- Postgres sharding (by hospital ID)
- Read replicas for queries
- Backup strategy

API:
- Load balancer (distribute traffic)
- Auto-scaling (spin up more servers as load increases)
- Rate limiting per customer

Processing:
- Message queue (Kafka) for async work
- Worker pools that scale independently
- Cache (Redis) optimization critical now

Monitoring:
- Must monitor everything (can't debug at scale)
- Alerts for anomalies
- Performance metrics by customer (fairness)
```

### Question 2: "What's your approach to database optimization?"
```
Problem: Queries getting slower as data grows

Approach:
1. Profile first (find actual bottleneck)
2. Indexes (most common fix)
3. Query optimization (refactor slow queries)
4. Caching (Redis for hot data)
5. Sharding (if database still bottleneck)
6. Read replicas (for read-heavy workloads)

In healthcare context:
- Can't lose data (backups, replication)
- Audit trail must be accurate (careful with deletions)
- Compliance affects what optimizations we can do
```

### Question 3: "How do you handle idempotency in payments?"
```
Problem: Payment submitted twice accidentally. 
Hospital charged twice? System allows this?

Solution: Idempotency keys

Implementation:
- Client generates UUID for each operation
- Submit: {idempotencyKey: uuid, amount: 100}
- Server: Check if UUID seen before
  - If yes: Return previous result (don't double-charge)
  - If no: Process, store UUID, return result

Why:
- Networks fail, requests get retried
- Humans accidentally click submit twice
- Idempotency prevents disasters
```

---

## Common Questions & Your Answers

### Q: "Why are you interested in TachyHealth?"
```
"Three reasons:

1. HEALTHCARE + EMERGING MARKET
Your medical coding automation solves a real problem I saw in my EHR work.
But more: You're focusing on MENA, an underserved market. I'm more excited
about being early in emerging markets than competing in saturated US.

2. TECHNICAL DEPTH
Medical coding automation is genuinely complex (ML, compliance, integration).
Not another CRUD app. The challenge appeals to me.

3. ALIGNMENT
Series A, institutional backing (Al-Tawuniya), revenue-generating. You're not
vapor ware - you're solving real problems for real customers. That matters to me.
"
```

### Q: "What's your biggest weakness?"
```
"I tend to over-engineer systems early. On my first microservices project,
I implemented full CQRS + Event Sourcing when simpler solution would work.

I've learned: Ask 'what problem are we solving?' before architecture.
Start simple, add complexity when pain emerges.

For TachyHealth: Series A stage means pragmatism matters. I'll bring technical
rigor where needed but avoid gold-plating.
"
```

### Q: "Tell me about conflict with a colleague"
```
[Use Story 3: Collaboration - Backend team vs Product teams]

"On my last project, teams disagreed on ownership model.
Backend wanted centralized control, Product wanted speed.

I listened to both, found middle ground:
- Product teams owned their services (velocity goal)
- Infrastructure team owned platforms (consistency goal)

Both got what mattered. Result: Better culture, better outcomes.
"
```

---

## Practice Tips

1. **Record yourself answering** - Play back, look for:
   - Rambling (get to point faster)
   - Technical jargon overload (explain for business audience)
   - Missing key details (specific examples help)

2. **Practice out loud** - Don't just think through answers
   - You'll catch awkward phrasing
   - Get comfortable with technical content

3. **Ask clarifying questions** - Shows you think critically
   - "Let me make sure I understand..."
   - "What constraints matter most?"

4. **Use concrete numbers** - Better than vague
   - "Reduced latency 70% (2s → 600ms)" 
   - vs "Made system much faster"

5. **End with your question** - Keeps conversation alive
   - After answering, ask: "How does this compare to your current approach?"
   - Shows genuine interest

---

## Final Checklist

- [ ] Understand medical coding basics (ICD-10, CPT, accuracy importance)
- [ ] Understand revenue cycle complexity
- [ ] Be ready for system design questions
- [ ] Know your STAR stories by heart
- [ ] Practice out loud
- [ ] Prepare questions about TachyHealth
- [ ] Remember: Healthcare is different (correctness, compliance)
- [ ] Show enthusiasm for problem domain
- [ ] Demonstrate technical depth
- [ ] Show you work well with others

**You've got this. 🎯**

