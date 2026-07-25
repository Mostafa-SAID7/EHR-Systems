# SQL Performance Tuning

## Query Execution Plan Analysis

### Step 1: Enable Actual Plan (SQL Server)

```sql
-- In SQL Server Management Studio
Ctrl + L  -- Display Actual Execution Plan

SELECT u.Name, COUNT(o.OrderId) as OrderCount
FROM Users u
LEFT JOIN Orders o ON u.Id = o.UserId
WHERE u.Status = 'Active'
GROUP BY u.Id, u.Name;
```

### Step 2: Read Plan Indicators

```
Green Checkmark = Efficient
Yellow Triangle = Warning (inefficient)
Red X = Critical problem

Common Issues:
- Table Scan (reads all rows) → Add index
- Sort Operation → Add index matching ORDER BY
- Nested Loop (expensive join) → Review join condition
- Spill to Disk → Increase memory or simplify query
```

---

## Index Strategies

### Missing Indexes

```sql
-- SQL Server: Find missing indexes
SELECT 
    CONVERT(DECIMAL(18,2), migs.user_seeks * migs.avg_total_user_cost 
        * migs.avg_user_impact * (migs.user_seeks + migs.user_scans 
        + migs.user_lookups)) AS improvement_measure
    , mid.equality_columns
    , mid.inequality_columns
    , mid.included_columns
FROM sys.dm_db_missing_index_groups mig
JOIN sys.dm_db_missing_index_group_details migd ON mig.index_handle = migd.index_handle
JOIN sys.dm_db_missing_index_details mid ON migd.index_handle = mid.index_handle
JOIN sys.dm_db_missing_index_groups_stats migs ON mig.index_group_id = migs.group_handle
ORDER BY improvement_measure DESC;
```

### Index Fragmentation

```sql
-- Check fragmentation
SELECT 
    OBJECT_NAME(ips.object_id) AS TableName,
    i.name AS IndexName,
    ips.avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
JOIN sys.indexes i ON ips.object_id = i.object_id 
    AND ips.index_id = i.index_id;

-- Rebuild if > 30% fragmented
ALTER INDEX IndexName ON TableName REBUILD;

-- Reorganize if 10-30% fragmented
ALTER INDEX IndexName ON TableName REORGANIZE;
```

---

## Common Performance Issues

### Problem: Slow Login Query

```sql
-- ❌ SLOW - Function on indexed column
SELECT * FROM Users 
WHERE UPPER(Email) = UPPER('user@example.com');

-- ✅ FAST - Direct comparison
SELECT * FROM Users 
WHERE Email = 'user@example.com';
-- Email must be stored consistently (lowercase)
```

### Problem: Cartesian Product

```sql
-- ❌ SLOW - Cartesian explosion
SELECT u.*, o.*, i.*
FROM Users u
JOIN Orders o ON u.Id = o.UserId
JOIN OrderItems i ON o.Id = i.OrderId
WHERE u.Id = 1;

Result: 1 user × 10 orders × 100 items = 1000 rows (duplicated data)

-- ✅ FAST - Separate queries
SELECT u.* FROM Users WHERE Id = 1;
SELECT o.* FROM Orders WHERE UserId = 1;
SELECT i.* FROM OrderItems WHERE OrderId IN (...);
```

### Problem: Missing WHERE Clause

```sql
-- ❌ SLOW - Scans all 1 million rows
SELECT * FROM Users
ORDER BY CreatedAt DESC
LIMIT 10;

-- ✅ FAST - Uses index
SELECT * FROM Users 
WHERE Status = 'Active'
ORDER BY CreatedAt DESC
LIMIT 10;
```

---

## Optimization Techniques

### 1. Use DISTINCT Carefully

```sql
-- ❌ SLOW
SELECT DISTINCT u.* FROM Users u
JOIN Orders o ON u.Id = o.UserId;

-- ✅ FAST - Explicit join
SELECT u.* FROM Users u
WHERE EXISTS (SELECT 1 FROM Orders WHERE UserId = u.Id);
```

### 2. Avoid OR in WHERE

```sql
-- ❌ SLOW - Multiple index seeks
SELECT * FROM Users 
WHERE Status = 'Active' 
   OR Status = 'Pending'
   OR Status = 'Inactive';

-- ✅ FAST - Single index operation
SELECT * FROM Users 
WHERE Status IN ('Active', 'Pending', 'Inactive');
```

### 3. Batch Operations

```sql
-- ❌ SLOW - 1000 individual queries
FOR i = 1 TO 1000
    INSERT INTO Users VALUES (...)

-- ✅ FAST - Single batch
INSERT INTO Users VALUES
    (...),
    (...),
    (...) -- All 1000 at once
```

---

## Interview Q&A

**Q: How do you identify slow queries?**

A:
1. Enable Query Store (SQL Server) or slow query log (MySQL)
2. Look for queries with high execution time
3. View execution plan - look for table scans
4. Add missing indexes

**Q: Table Scan vs Index Seek?**

A:
- Table Scan: Reads every row (slow for large tables)
- Index Seek: Uses index to find rows directly (fast)

**Q: When does an index hurt performance?**

A:
- INSERT/UPDATE/DELETE operations (must update index)
- Too many indexes on single table (optimizer confusion)
- Low cardinality columns (bool, status with few values)
