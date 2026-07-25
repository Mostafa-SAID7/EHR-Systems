# SQL: Joins, Aggregations, CTEs

## INNER JOIN

```sql
SELECT u.Id, u.Name, o.OrderId, o.Amount
FROM Users u
INNER JOIN Orders o ON u.Id = o.UserId;

-- Returns: Only users with orders
-- Result: 
-- 1 | Ahmed | 101 | 500
-- 1 | Ahmed | 102 | 200
-- 2 | Ali   | 103 | 300
```

---

## LEFT JOIN (LEFT OUTER JOIN)

```sql
SELECT u.Id, u.Name, o.OrderId, o.Amount
FROM Users u
LEFT JOIN Orders o ON u.Id = o.UserId;

-- Returns: ALL users, with orders if they have them
-- Result:
-- 1 | Ahmed | 101 | 500
-- 1 | Ahmed | 102 | 200
-- 2 | Ali   | 103 | 300
-- 3 | Sara  | NULL | NULL  ← No orders
```

---

## RIGHT JOIN

```sql
SELECT u.Id, u.Name, o.OrderId, o.Amount
FROM Users u
RIGHT JOIN Orders o ON u.Id = o.UserId;

-- Returns: ALL orders, with users if they have them
-- Rarely used in modern SQL
```

---

## FULL OUTER JOIN

```sql
SELECT u.Id, u.Name, o.OrderId, o.Amount
FROM Users u
FULL OUTER JOIN Orders o ON u.Id = o.UserId;

-- Returns: ALL users AND ALL orders (matched where possible)
-- Note: SQL Server supports, MySQL doesn't
```

---

## GROUP BY & AGGREGATIONS

```sql
SELECT 
    u.Id,
    u.Name,
    COUNT(o.OrderId) AS OrderCount,
    SUM(o.Amount) AS TotalAmount,
    AVG(o.Amount) AS AvgAmount,
    MAX(o.Amount) AS MaxAmount,
    MIN(o.Amount) AS MinAmount
FROM Users u
LEFT JOIN Orders o ON u.Id = o.UserId
GROUP BY u.Id, u.Name;

-- Result:
-- 1 | Ahmed | 2 | 700 | 350 | 500 | 200
-- 2 | Ali   | 1 | 300 | 300 | 300 | 300
-- 3 | Sara  | 0 | NULL | NULL | NULL | NULL
```

---

## HAVING (Filter Groups)

```sql
SELECT 
    u.Id,
    u.Name,
    COUNT(o.OrderId) AS OrderCount,
    SUM(o.Amount) AS TotalAmount
FROM Users u
LEFT JOIN Orders o ON u.Id = o.UserId
GROUP BY u.Id, u.Name
HAVING COUNT(o.OrderId) > 0  -- Only users with at least 1 order
    AND SUM(o.Amount) > 500;  -- Total > 500

-- Result:
-- 1 | Ahmed | 2 | 700
```

**WHERE vs HAVING:**
```
WHERE    - Filters BEFORE grouping (on individual rows)
HAVING   - Filters AFTER grouping (on aggregated results)
```

---

## CTE (Common Table Expression)

```sql
-- Define CTE (temporary result set)
WITH UserOrderSummary AS (
    SELECT 
        u.Id,
        u.Name,
        COUNT(o.OrderId) AS OrderCount,
        SUM(o.Amount) AS TotalAmount
    FROM Users u
    LEFT JOIN Orders o ON u.Id = o.UserId
    GROUP BY u.Id, u.Name
)
-- Use CTE
SELECT 
    Id,
    Name,
    OrderCount,
    TotalAmount
FROM UserOrderSummary
WHERE OrderCount > 0
ORDER BY TotalAmount DESC;
```

**Multiple CTEs:**
```sql
WITH 
UserStats AS (
    SELECT UserId, COUNT(*) AS OrderCount
    FROM Orders
    GROUP BY UserId
),
HighValueUsers AS (
    SELECT UserId FROM UserStats WHERE OrderCount > 5
)
SELECT u.* FROM Users u
WHERE u.Id IN (SELECT UserId FROM HighValueUsers);
```

---

## Interview Q&A

**Q: INNER JOIN vs LEFT JOIN?**

A:
- INNER: Only matching records (users with orders)
- LEFT: All left table + matching (all users, orders if exist)

**Q: When use GROUP BY?**

A: When aggregating data (COUNT, SUM, AVG)

**Q: WHERE vs HAVING?**

A:
- WHERE: Before grouping (fast)
- HAVING: After grouping (slower, but necessary for aggregate filtering)
