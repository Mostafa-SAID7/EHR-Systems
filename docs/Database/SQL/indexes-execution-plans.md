# Indexes & Execution Plans

## What is an Index?

```sql
-- Without index - full table scan
SELECT * FROM Users WHERE Email = 'user@example.com';
-- Scans all 1 million rows = SLOW

-- With index - direct lookup
CREATE INDEX IX_Users_Email ON Users(Email);
SELECT * FROM Users WHERE Email = 'user@example.com';
-- Uses index, finds row instantly = FAST
```

---

## Clustered Index

```sql
-- One per table (primary key is usually clustered)
CREATE CLUSTERED INDEX PK_Users ON Users(Id);

-- Affects physical row order
-- Table sorted by Id
-- Fastest access path
```

---

## Non-Clustered Index

```sql
-- Multiple allowed per table
CREATE NONCLUSTERED INDEX IX_Users_Email ON Users(Email);
CREATE NONCLUSTERED INDEX IX_Users_Status_CreatedAt ON Users(Status, CreatedAt);

-- Doesn't affect row order
-- Separate lookup structure
-- Good for WHERE and JOIN conditions
```

---

## Composite Index

```sql
-- Index on multiple columns
CREATE NONCLUSTERED INDEX IX_Orders_UserId_Date 
ON Orders(UserId, CreatedAt);

-- Good for queries:
SELECT * FROM Orders 
WHERE UserId = 5 AND CreatedAt > '2024-01-01';
-- Uses composite index efficiently
```

---

## Execution Plan

### Without Index (❌ Slow)
```
┌─────────────────────────────┐
│ Table Scan                  │
│ Reads ALL 1,000,000 rows   │
│ Cost: 100%                  │
└─────────────────────────────┘
```

### With Index (✅ Fast)
```
┌─────────────────────────────┐
│ Seek on Index              │
│ Reads 1 row (direct lookup)|
│ Cost: 0.01%                 │
└─────────────────────────────┘
```

---

## Query Optimization Tips

### ❌ Bad Query
```sql
SELECT * FROM Users
WHERE UPPER(Email) = 'USER@EXAMPLE.COM';
-- Function on indexed column disables index
```

### ✅ Good Query
```sql
SELECT * FROM Users
WHERE Email = 'user@example.com';
-- Direct comparison uses index
```

---

## Interview Q&A

**Q: Clustered vs Non-Clustered Index?**

A:
- Clustered: 1 per table, affects physical order, primary key
- Non-Clustered: Multiple allowed, separate structure, fast lookups

**Q: When to add index?**

A:
- WHERE clause columns
- JOIN ON columns
- ORDER BY columns (if large result set)
- NOT on low-cardinality columns (bool, status with few values)

**Q: Index drawbacks?**

A:
- Slows INSERT/UPDATE/DELETE (must update index)
- Uses disk space
- Too many indexes = slow queries (optimizer confusion)

**Q: How to find slow queries?**

A: Enable Query Store, check execution plans, look for Table Scans
