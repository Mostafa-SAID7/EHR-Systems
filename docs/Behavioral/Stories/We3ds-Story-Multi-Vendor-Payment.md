# Story: Multi-Vendor Payment Reconciliation at Scale

**From:** we3ds (Multi-vendor ecommerce platform)  
**Best For:** "Complex business logic", "Financial systems", "Problem-solving"  
**Time:** 5 minutes  
**Key Skill:** Business thinking, financial systems, system design

---

## SITUATION

As we3ds grew, payment system became increasingly complex and fragile.

**The Problem:**
```
Simple case (1 vendor):
- Customer pays $100 → Vendor gets $100

Real case (we3ds multi-vendor):
- Customer orders from 3 different vendors
  * Vendor A: $30 item
  * Vendor B: $50 item  
  * Vendor C: $20 item
  * Total: $100 + $10 platform fee

- Customer pays $110 (all 4 orders in cart)

- How to split payment?
  * Vendor A gets: $30 - platform fee share?
  * Vendor B gets: $50 - platform fee share?
  * Vendor C gets: $20 - platform fee share?
  * Platform gets: $10 + fees from vendors

- What if:
  * Payment fails halfway through? (partial charge)
  * One vendor cancels order? (need refund)
  * One vendor has return? (partial refund)
  * Payment processor issue? (money stuck in limbo)
```

**At Scale (we3ds growth):**
- 1000s of orders/day
- Multiple payment methods (credit card, COD, wallet)
- 100s of vendors with different fee structures
- Different regional requirements (tax, fees, regulations)
- Manual reconciliation taking hours weekly
- Financial discrepancies appearing regularly
- Vendor disputes about payment amounts

**Impact:**
- Vendors losing trust (payment discrepancies)
- Manual reconciliation (expensive, error-prone)
- Accounting chaos (audits difficult)
- Platform at financial risk (money handling)

---

## TASK

Design payment system that:
1. Handles multi-vendor payment splitting correctly
2. Maintains accurate financial records
3. Recovers from failures (payment processor down, network issues)
4. Provides vendor visibility (what they're owed)
5. Enables efficient reconciliation

---

## ACTION

### Phase 1: Analyzing Current System

**Discovered Problems:**

1. **Naive Payment Splitting**
   - Split payment across vendors before confirming payment
   - If one vendor charge failed, whole order failed
   - Money stuck in vendor accounts

2. **Lost Events**
   - Order created → Payment charged → Vendor notified
   - If notification failed: Vendor never got paid
   - Manual investigation required

3. **Refund Chaos**
   - Customer refund requested
   - Manual process to find which vendor payment to refund
   - No audit trail
   - Disputes with vendors ("Did we refund you or not?")

4. **Tax/Fee Complexity**
   - Different vendors pay different commission percentages
   - Regional taxes vary
   - Manually calculating each vendor's amount
   - Easy to get wrong

### Phase 2: Solution Design

**Payment Flow Redesign:**

```
Step 1: CHARGE CUSTOMER (single charge)
├─ Charge full amount from customer
├─ If succeeds: Continue
└─ If fails: Stop (nothing owed to vendors)

Step 2: RECORD PAYMENT (immutable audit trail)
├─ Log: Order ID, amount, timestamp, method
├─ Log: Vendor splits (A=$25, B=$45, C=$20)
├─ Log: Platform fees
└─ All immutable (audit trail)

Step 3: CALCULATE PAYOUTS (deterministic)
├─ Per vendor: Calculate owed amount
├─ Deduct platform fees (pre-determined)
├─ Deduct chargebacks/disputes (if any)
├─ Result: Exact amount owed to vendor

Step 4: PUBLISH EVENTS
├─ PaymentReceivedEvent
├─ PayoutCalculatedEvent (per vendor)
└─ Other systems subscribe (accounting, vendor portal)

Step 5: SCHEDULE PAYOUT
├─ Queue payout for next settlement
├─ Multiple vendors → Single batch file
├─ Send to payment processor
└─ Record payout execution

Step 6: HANDLE RETURNS/REFUNDS
├─ Customer requests return
├─ Vendor approves return
├─ System calculates: How to refund customer? From which vendor payout?
├─ Execute refund
└─ Adjust vendor payout accordingly
```

### Phase 3: Implementation

**Database Design:**

```sql
-- Immutable payment record
CREATE TABLE payments (
  payment_id UUID PRIMARY KEY,
  order_id UUID,
  customer_id UUID,
  amount DECIMAL,
  method ENUM('card', 'wallet', 'cod'),
  status ENUM('pending', 'charged', 'failed'),
  charged_at TIMESTAMP,
  created_at TIMESTAMP,
  CONSTRAINT immutable CHECK (created_at IS NOT NULL)
);

-- Per-vendor calculation
CREATE TABLE payment_splits (
  split_id UUID PRIMARY KEY,
  payment_id UUID REFERENCES payments,
  vendor_id UUID,
  item_amount DECIMAL,  -- What the items cost
  platform_fee DECIMAL, -- Platform's cut (pre-determined %)
  tax_amount DECIMAL,   -- Applicable taxes
  net_amount DECIMAL,   -- item_amount - platform_fee - tax
  status ENUM('calculated', 'queued', 'paid', 'disputed'),
  calculated_at TIMESTAMP,
  CONSTRAINT immutable_split CHECK (calculated_at IS NOT NULL)
);

-- Audit trail (event log)
CREATE TABLE payment_events (
  event_id UUID PRIMARY KEY,
  payment_id UUID,
  event_type ENUM('charged', 'split_calculated', 'payout_queued', 'payout_sent', 'refund_requested', 'refund_processed'),
  data JSONB,  -- Details of the event
  created_at TIMESTAMP
);
```

**Payout Logic:**

```
Vendor Payout Calculation:
├─ Start with all payment_splits for vendor
├─ Sum: item_amount - platform_fee - tax = vendor owes customer
├─ Deduct: Any returned items (refunded)
├─ Deduct: Any chargebacks/disputes
├─ Result: Exact amount to payout vendor
└─ All auditable (event trail)
```

**Event Publishing:**

```
System publishes events:
- PaymentChargedEvent
- PaymentSplitCalculatedEvent (per vendor)

Other systems subscribe:
- Accounting: Track revenue
- Vendor Portal: Vendors see what they'll be paid
- Analytics: Understand payment flows
- Reconciliation: Automate verification
```

**Refund Handling:**

```
Customer requests return:
1. Order identifies vendors involved
2. For each vendor involved:
   - Vendor approves/rejects return
   - If approved: Calculate refund
     * Refund amount = item cost (not full amount)
     * Platform fee stays with platform (non-refundable)
     * Tax refunded if applicable
3. Execute refund to customer
4. Adjust vendor payout (subtract refunded amount)
5. Publish RefundProcessedEvent
```

### Phase 4: Handling Edge Cases

**Case 1: Payment Processor Timeout**
- Charge might have succeeded but we didn't get confirmation
- Solution: Idempotent payment IDs
  * "Only charge this once, even if you see it twice"
  * Prevents duplicate charges
  * Safe retry logic

**Case 2: Vendor Cancels After Payment**
- Customer paid, vendor cancels part of order
- Solution: Reverse part of payment split
  * Remove cancelled item from split
  * Recalculate vendor payout
  * Refund customer
  * Event trail shows what happened

**Case 3: Regional Tax Complexity**
- Different regions have different tax rates
- Some vendors tax-exempt
- Solution: Calculate at payment time, store in split
  * No recalculation needed later
  * Audit trail shows tax applied
  * Consistent with government records (tax time)

**Case 4: Bulk Refund (Flash Sale Issue)**
- 1000 customers bought defective item from one vendor
- Need to refund all
- Solution: Batch refund processing
  * Identify all affected payments
  * Calculate total refund
  * Execute batch refund
  * Update all vendor splits
  * Audit trail shows bulk refund reason

### Phase 5: Reconciliation Automation

**Daily Reconciliation:**

```
1. Query all payments charged yesterday
2. For each payment:
   - Verify: Amount charged = Sum of splits
   - Verify: Platform fee + vendor net = total charged
   - Verify: All splits have corresponding vendor
   - Alert if: Discrepancy found

3. Query all payouts scheduled
4. Verify: Scheduled amount matches calculated amount

5. If all verified: Green light for payout execution
6. If discrepancy: Manual review queue (with details)
```

**Vendor Portal:**

```
Vendor dashboard shows:
- All their orders from past month
- Payment received from each order
- Platform fees deducted (transparent)
- Taxes applied
- Current payout owed
- Historical payout record

Builds trust through transparency.
```

---

## RESULT

### Quantified Outcomes

| Metric | Before | After |
|--------|--------|-------|
| **Manual Reconciliation Time** | 4 hours/week | 15 min/week (automated) |
| **Payment Discrepancies** | 20-30/week | <1/week |
| **Vendor Disputes** | 5-10/week | <2/week |
| **Payout Accuracy** | 95% (manual) | 99.9% (automated) |
| **Audit Trail** | Partial | Complete (immutable) |
| **Refund Processing** | 1-2 days (manual) | Automated (same day) |

### Qualitative Improvements

- **Vendor Trust:** Complete transparency on payment calculation
- **Accounting:** Clear audit trail for financial audits
- **Operational:** Manual work eliminated
- **Scalability:** System doesn't break as orders grow
- **Compliance:** Tax/fee calculations consistent and auditable

### Technical Learnings

1. **Immutability Matters**
   - Payment records must be immutable
   - Audit trail must be complete
   - "Who did what, when" must be traceable

2. **Event-Driven for Financial Systems**
   - Events create audit trail
   - Multiple systems can react (accounting, vendor portal, etc.)
   - Easy to debug (replay events)

3. **Deterministic Calculation**
   - Complex logic, but deterministic
   - Same input always produces same output
   - Easy to verify, easy to audit

4. **Separation of Concerns**
   - Charging (happens once, immutable)
   - Splitting (deterministic calculation)
   - Payouts (batch processing)
   - Refunds (reverse of splits)
   - Each concern handled independently

---

## How to Tell This Story

### Opening
"At we3ds, we handled payments for multi-vendor marketplace. Payment system started simple but became increasingly complex as business grew."

### The Problem
"Multi-vendor means: Customer pays once, multiple vendors need to get paid. With platform fees, taxes, refunds, returns - the logic exploded."

### The Mistake
"Initially tried splitting payment before confirming it worked. If one vendor's charge failed, whole thing failed. Money got stuck."

### The Solution
"Redesigned: Charge customer first (single, atomic), then split payment deterministically, then payout vendors. Complete audit trail."

### Impact
"Vendor disputes dropped from 5-10/week to <2/week. Manual reconciliation time dropped from 4 hours to 15 minutes. System became trustworthy."

---

## Key Talking Points

**"How do you approach complex business logic?"**

"I think through edge cases systematically:
- What are all the states the system can be in?
- What happens when things go wrong?
- How do we recover?
- How do we audit?

Then I design for those scenarios, not just the happy path."

**"How do you design financial systems?"**

"Immutability, audit trails, deterministic calculations:
- Every action recorded (audit trail)
- Same input always same output (deterministic)
- Previous state never changes (immutability)
- Easy to verify, easy to audit"

**"What's your approach to payments/complex transactions?"**

"In ecommerce, payment splitting taught me to think about:
- Atomicity (all or nothing)
- Determinism (same input = same output)
- Auditability (track everything)
- Failure recovery (what if something breaks mid-transaction?)"

---

## Why This Story Works for TachyHealth

- **Relevant:** Medical coding billing is similar - multiple stakeholders, complex calculations
- **Shows Business Thinking:** Understood vendor needs, built for trust
- **Shows Complexity Handling:** Multi-vendor payment complex, coding + insurance also complex
- **Shows Reliability:** Financial systems can't fail
- **Ecommerce Experience:** Shows diverse background

---

## Connection to TachyHealth

When telling this story:

"In ecommerce payments, I learned that complex business logic requires:
- Clear calculation (deterministic, auditable)
- Complete tracking (who did what, when)
- Vendor transparency (builds trust)

Medical coding billing has similar requirements:
- Hospital needs to know exactly why they get paid X amount
- Insurance audits require complete tracking
- Transparency builds trust

The principles transfer across domains."

