# Transactions & Concurrency Control

## ACID Properties

```
A - Atomicity     → All or nothing
C - Consistency   → Data valid before and after
I - Isolation     → Concurrent transactions don't interfere
D - Durability    → Once committed, persists
```

---

## Transactions - Atomic Operations

**Transaction:** Group multiple operations that succeed together or fail together.

```csharp
// ❌ Without transaction - Risk of inconsistency
var user = await context.Users.FirstAsync(u => u.Id == 1);
user.Balance -= 100; // Transfer out

var recipient = await context.Users.FirstAsync(u => u.Id == 2);
recipient.Balance += 100; // Transfer in

await context.SaveChangesAsync();
// If crash between operations: Money disappeared!
```

### Using Transaction

```csharp
// ✅ With transaction - Atomic
using var transaction = await context.Database.BeginTransactionAsync();
try
{
    var user = await context.Users.FirstAsync(u => u.Id == 1);
    user.Balance -= 100;
    
    var recipient = await context.Users.FirstAsync(u => u.Id == 2);
    recipient.Balance += 100;
    
    await context.SaveChangesAsync();
    await transaction.CommitAsync();
    // Both changes committed together
}
catch
{
    await transaction.RollbackAsync();
    // Both changes rolled back if any error
    throw;
}
```

### Using TransactionScope

```csharp
using var scope = new TransactionScope(
    TransactionScopeAsyncFlowOption.Enabled);
try
{
    // Operations
    await context.SaveChangesAsync();
    scope.Complete(); // Commit
}
catch
{
    // Auto rollback when TransactionScope disposed
    throw;
}
```

---

## Isolation Levels

Defines how concurrent transactions interact.

```
SERIALIZABLE
↑ Most isolation (slowest, prevents phantom reads)
REPEATABLE READ
SNAPSHOT
READ COMMITTED
↓ Least isolation (fastest, allows dirty reads)
READ UNCOMMITTED
```

### Dirty Read (Reading uncommitted data)

```csharp
// Transaction 1              Transaction 2
var user = new User
{
    Balance = 1000
};
context.Users.Add(user);
await context.SaveChangesAsync();
                           var u = await context.Users.FirstAsync();
                           Console.WriteLine(u.Balance); // 1000
                           
// Oops, rollback!
transaction.Rollback();
                           // Transaction 2 read data that didn't exist!
```

**Fix: Use READ_COMMITTED (default in SQL Server)**

### Non-Repeatable Read

```csharp
// Transaction 1              Transaction 2
var user = await context
    .Users.FirstAsync(u => u.Id == 1);
Console.WriteLine(user.Balance); // 1000
                           var u = await context.Users.FirstAsync(u => u.Id == 1);
                           u.Balance += 100;
                           await context.SaveChangesAsync();
                           
var user2 = await context
    .Users.FirstAsync(u => u.Id == 1);
Console.WriteLine(user2.Balance); // 1100
// Same query returned different value!
```

**Fix: Use SERIALIZABLE or REPEATABLE_READ**

---

## Optimistic Locking

**Assumption:** Conflicts are rare. Check for conflicts at save time.

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; } // Auto-incremented on each update
}
```

### How It Works

```csharp
// User A reads
var user = await context.Users.FirstAsync(u => u.Id == 1);
// RowVersion: 0x00000000000001

// User B reads
var userB = await context.Users.FirstAsync(u => u.Id == 1);
// RowVersion: 0x00000000000001

// User B updates first
userB.Name = "Changed by B";
await context.SaveChangesAsync();
// UPDATE Users SET Name = 'Changed by B', RowVersion = 0x00000000000002
// WHERE Id = 1 AND RowVersion = 0x00000000000001

// User A tries to update
user.Name = "Changed by A";
await context.SaveChangesAsync();
// UPDATE Users SET Name = 'Changed by A', RowVersion = 0x00000000000003
// WHERE Id = 1 AND RowVersion = 0x00000000000001
// ❌ Fails! RowVersion is now 0x00000000000002, not 0x00000000000001

// Exception thrown:
// DbUpdateConcurrencyException
```

### Handling Conflict

```csharp
try
{
    var user = await context.Users.FirstAsync(u => u.Id == 1);
    user.Name = "New Name";
    await context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    // Another user modified this record
    var databaseValues = await ex.Entries[0].GetDatabaseValuesAsync();
    var databaseName = databaseValues["Name"];
    
    // Option 1: Keep your changes
    await ex.Entries[0].ReloadAsync();
    user.Name = "New Name";
    await context.SaveChangesAsync();
    
    // Option 2: Show user what changed
    throw new OptimisticLockException(
        $"Record changed by another user. New value: {databaseName}");
}
```

---

## Pessimistic Locking

**Assumption:** Conflicts are common. Lock row while editing.

```csharp
using var transaction = await context.Database.BeginTransactionAsync(
    System.Data.IsolationLevel.Serializable); // Lock during transaction
try
{
    var user = await context.Users.FirstAsync(u => u.Id == 1);
    // Row is locked here
    
    user.Name = "New Name";
    await context.SaveChangesAsync();
    await transaction.CommitAsync();
}
finally
{
    await transaction.RollbackAsync();
    // Lock released
}
```

**Problems:**
- Blocks concurrent access
- Deadlock risk
- Performance issues

**Use optimistic instead for web apps.**

---

## Concurrency Tokens (Custom)

```csharp
public class Order
{
    public int Id { get; set; }
    public string Status { get; set; }
    
    [ConcurrencyCheck]
    public DateTime ModifiedAt { get; set; }
}

// In DbContext
protected override void OnModelCreating(ModelBuilder mb)
{
    mb.Entity<Order>()
        .Property(o => o.ModifiedAt)
        .IsConcurrencyToken();
}
```

---

## Interview Q&A

**Q: What's the difference between optimistic and pessimistic locking?**

A:
- **Optimistic:** Assume conflicts rare, check at save (use RowVersion). For web.
- **Pessimistic:** Lock row during edit, blocks concurrent access. For desktop apps.

**Q: How does RowVersion work?**

A: EF auto-increments RowVersion on each update. When saving, checks if RowVersion matches. If not, conflict detected.

**Q: What's a transaction?**

A: Group of operations that succeed together or fail together. Ensures consistency if crash during operation.

**Q: Isolation levels - what do they do?**

A: Define how concurrent transactions see each other's data:
- READ_UNCOMMITTED: Fast, dirty reads possible
- READ_COMMITTED: Default, clean reads only
- SERIALIZABLE: Slow, complete isolation

**Q: How to handle concurrency exception?**

A:
```csharp
catch (DbUpdateConcurrencyException ex)
{
    // Reload fresh data
    await ex.Entries[0].ReloadAsync();
    // Retry or show user
}
```
