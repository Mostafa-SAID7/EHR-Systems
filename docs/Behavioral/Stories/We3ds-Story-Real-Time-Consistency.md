# Story: Real-Time Data Consistency in Ecommerce

**From:** we3ds (Multi-vendor ecommerce platform)  
**Best For:** "Technical challenge", "Scalability problem", "Real-time systems"  
**Time:** 5 minutes  
**Key Skill:** Problem-solving under constraints, systems thinking, performance optimization

---

## SITUATION

You were building a multi-vendor ecommerce marketplace. Critical problem emerged: **Inventory inconsistency**.

**The Problem:**
- Customer A browsing product → See 5 items in stock
- Customer B browsing same product → Also sees 5 items
- Both customers buy 1 item at same time
- Inventory should go: 5 → 3 (sold 2 items)
- But sometimes went: 5 → 4 (only 1 sold recorded)
- Or worse: Goes to -1 (oversold)

**Why This Is Serious:**
- Overselling = Vendor promises items that don't exist
- Broken trust = Customer refunds, disputes, chargebacks
- Lost revenue = Costs money
- At peak times (flash sales): 1000s of concurrent users buying
- Database contention created race conditions

**Scale Context:**
- Peak traffic: 100+ concurrent users on same product
- Sale events: 500+ concurrent orders/minute
- Multiple database instances (read replicas, sharding)
- Consistency had to be maintained across all instances

---

## TASK

Ensure inventory accuracy under concurrent load:
1. Prevent overselling (guarantee never negative)
2. Maintain performance (fast checkout)
3. Provide real-time visibility (customers see accurate stock)
4. Handle peak traffic (Black Friday scale)

---

## ACTION

### Phase 1: Understanding the Problem

**Diagnosed Root Cause:**
```
Current flow:
1. Read inventory: 5 items
2. Start transaction
3. [100ms delay while processing other logic]
4. Another user reads inventory: still 5 items
5. Both users try to buy
6. Both queries write back: 4 items (should be 3)
```

**Why This Happened:**
- Checking inventory separate from purchasing
- Gap between check and purchase allowed race condition
- Read replicas had slight lag (eventual consistency)
- At scale, even milliseconds matter

### Phase 2: Solution Design

**Option A: Database-Level Locks**
- Lock inventory row during purchase
- Pro: Guaranteed consistency
- Con: Performance tanks (queries queue up, checkout slow)
- Con: Doesn't work across sharded databases
- ❌ Rejected

**Option B: Optimistic Locking**
- Don't lock, just track versions
- On update: "If version=5, set to 4"
- If version changed, retry
- Pro: Better performance than locking
- Con: Retries under high load (unpredictable)
- ❌ Partially rejected (used as backup)

**Option C: Event-Driven Inventory Updates** ✅ Chosen
- Purchase creates event (OrderCreatedEvent)
- Inventory service subscribes to events
- Updates inventory asynchronously (guaranteed order)
- Database has "inventory_version" for optimistic locking

**Option D (Complement): Deterministic Allocation**
- Inventory reserved before checkout completes
- Customer can't complete checkout without reservation
- Timeout: Release reservation if not completed in 10 minutes

### Phase 3: Implementation

**Architecture:**
```
┌─────────────────────────────────────┐
│ Customer clicks "Buy Now"           │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│ System: Try to RESERVE inventory    │
│ - Atomically decrements inventory   │
│ - Uses optimistic locking (version) │
│ - If fails: Show "Out of stock"     │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│ Reservation Successful              │
│ - Hold reserved for 10 min          │
│ - Send to payment processing        │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│ Payment Processing                  │
│ - Charge customer card              │
│ - If successful: Create Order       │
│ - If failed: Release reservation    │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│ Order Created Event                 │
│ - Publish to message queue          │
│ - Inventory updated (immutable)     │
└─────────────────────────────────────┘
```

**Key Insight:** Reservation happens FIRST, before payment. Payment failure doesn't affect inventory.

**Database Implementation:**
```sql
-- Inventory table
CREATE TABLE inventory (
  product_id UUID,
  quantity INT,
  reserved INT,    -- NEW: Track reservations
  version INT,     -- For optimistic locking
  updated_at TIMESTAMP
);

-- Reserve inventory (atomic operation)
UPDATE inventory
SET 
  reserved = reserved + 1,
  version = version + 1
WHERE 
  product_id = $1 
  AND (quantity - reserved) >= 1  -- Only if available
  AND version = $2;  -- Optimistic lock

-- If UPDATE affected 0 rows: Reservation failed (out of stock)
-- If UPDATE affected 1 row: Reservation succeeded
```

**Message Queue (Kafka):**
```
OrderCreated event:
{
  orderId: "order-123",
  productId: "prod-456",
  quantity: 1,
  createdAt: "2025-07-26T10:30:00Z"
}

Inventory service subscribes:
- Receives OrderCreated event
- Decrements inventory quantity
- Publishes InventoryUpdated event
- Other services (search, recommendations) subscribe to InventoryUpdated
```

### Phase 4: Handling Edge Cases

**Case 1: Payment fails after reservation**
```
- Reservation held for 10 minutes
- If no OrderCreated event in 10 min: Auto-release
- Customer sees "Reservation expired, please try again"
- Inventory available for other customers
```

**Case 2: Network lag (inventory update delayed)**
```
- Real inventory: 0 items (reserved all)
- Customer might see: 1 item (lag in updates)
- Addresses by: Reservation system prevents overselling
  (Reservation fails if reserved + 1 > available)
```

**Case 3: Database replica lag**
```
- Customer A on read replica sees 5 items
- Buys 1 → Reservation on primary
- Customer A's inventory service might read from replica
- Solution: Always read inventory from primary for reservations
- Use replica only for displaying "Stock Available" UI
```

**Case 4: Peak load (1000 concurrent orders)**
```
- Without this system: Race conditions, overselling
- With this system:
  - Reservations serialize on primary database
  - Payment processing happens in parallel
  - Inventory updates happen via Kafka (unbuffered)
  - Result: Handles load, maintains consistency
```

### Phase 5: Monitoring & Alerts

**Metrics:**
- Reservation success rate (% of attempts that reserve)
- Reservation timeout rate (% that expire without purchase)
- Inventory accuracy (actual stock vs system)
- Purchase completion rate (reservations → actual orders)

**Alerts:**
- Alert if reservation success rate drops below 95% (too many conflicts)
- Alert if inventory goes negative (bug somewhere)
- Alert if large gap between reserved + quantity (reconciliation issue)
- Alert if Kafka lag > 1 minute (inventory updates delayed)

**Dashboard:**
- Real-time inventory levels by product
- Concurrent reservations
- Payment processing latency
- Inventory vs reserved vs available breakdown

---

## RESULT

### Quantified Outcomes

| Metric | Before | After |
|--------|--------|-------|
| **Overselling** | ~2% of peak orders | 0% (eliminated) |
| **Inventory Accuracy** | 94% (drifted due to errors) | 99.9% (reconciled hourly) |
| **Checkout Speed** | Fast (but wrong) | Still fast (~200ms) |
| **Peak Load** | Started failing at 200 req/sec | Handles 1000+ req/sec |
| **Customer Disputes** | 50+ per day (overselling) | <1 per day (other reasons) |

### Qualitative Improvements

- **Vendor Trust:** Vendors confident inventory is accurate
- **Customer Experience:** No more "Out of stock after payment"
- **Operational Simplicity:** No manual inventory reconciliation needed
- **Scalability:** System handles peak load without overselling

### Technical Learnings

1. **Consistency Models Matter**
   - Strong consistency needed for inventory
   - Eventual consistency OK for product details
   - Different consistency models for different data types

2. **Deterministic Allocation**
   - Reserve before committing (not after)
   - Timeouts for cleanup
   - Better than hoping for consistency

3. **Event-Driven Architecture**
   - Separates reservation (fast, consistent) from recording (can be async)
   - Allows independent scaling
   - Easy to audit (event trail)

4. **Operational Awareness**
   - Must monitor inventory health
   - Must track reconciliation
   - Must alert on anomalies

---

## How to Tell This Story

### Opening
"At we3ds, we had a critical problem: inventory consistency at scale. Customers could buy products that didn't exist (overselling)."

### The Problem
"In a multi-vendor marketplace, multiple customers buying same product simultaneously. Without careful handling, inventory would go negative or skip updates."

### The Challenge
"Simple locking would hurt performance. Eventual consistency would allow overselling. We needed something better."

### The Solution
"We separated reservation from recording. Reservation happens atomically (guaranteed to work or fail clearly). Payment processing happens next. Then inventory updated via events."

### Technical Details
"Used optimistic locking for reservation, Kafka for updates, timeouts for cleanup. Result: Handles peak load, maintains consistency, maintains performance."

### Result
"Eliminated overselling completely. Went from 2% of peak orders overselling to 0%. Vendors trusted system."

---

## Key Talking Points

**"How do you handle real-time consistency?"**

"Inventory in ecommerce is like patient data in healthcare - must be accurate real-time. I've built systems that handle this:
- Reservation system (atomic)
- Event-driven updates (reliable)
- Monitoring (catch anomalies early)

Healthcare coding could use similar approach - reservations prevent errors, events ensure consistency."

**"How do you scale systems?"**

"Not just throwing more servers. Need smart architecture:
- Separate read/write concerns
- Use right consistency model for each data type
- Event-driven for scale without sacrificing consistency
- Monitor and alert on anomalies"

**"Tell me about a technical challenge you solved"**

"Inventory consistency at scale - solution involved database optimization, event streaming, monitoring. Similar problems across domains."

---

## Why This Story Works for TachyHealth

- **Relevant:** Medical coding data accuracy is similar to inventory accuracy
- **Shows Problem-Solving:** Didn't use simplistic locking, thought creatively
- **Scale Experience:** Handled 1000+ concurrent requests
- **Real-time Thinking:** Systems must handle peak loads
- **Ecommerce Background:** Shows you have diverse experience

---

## Connection to TachyHealth

When telling this story:

"At we3ds, I learned that consistency at scale requires thoughtful architecture. 
Medical coding accuracy has similar requirements - must be right real-time, 
must handle scale, must maintain performance.

The approaches transfer: Event-driven architecture, deterministic allocation, 
monitoring. The domain changes but the principles are the same."

