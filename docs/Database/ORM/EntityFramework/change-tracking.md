# Change Tracking - How EF Knows What Changed

## How Change Tracking Works

```
1. Load Entity from Database
   ↓
   EF creates SNAPSHOT of properties
   ↓
2. Modify Entity in Memory
   ↓
   EF compares current state with SNAPSHOT
   ↓
3. SaveChanges()
   ↓
   EF generates UPDATE SQL for changed properties
```

---

## Snapshot Mechanism

```csharp
var user = context.Users.First(u => u.Id == 1);
// Snapshot: { Id: 1, Email: "old@test.com", Name: "Ahmed", Status: "Active" }

user.Email = "new@test.com";
user.Status = "Inactive";
// Current: { Id: 1, Email: "new@test.com", Name: "Ahmed", Status: "Inactive" }

await context.SaveChangesAsync();
// EF compares: Email and Status changed
// Generated SQL: UPDATE Users SET Email = 'new@test.com', Status = 'Inactive' WHERE Id = 1
```

---

## Entity States

```csharp
var entry = context.Entry(user);
Console.WriteLine(entry.State); // Returns EntityState

// Five states:

// 1. Detached - Not tracked by any DbContext
var newUser = new User { Email = "new@test.com" };
Console.WriteLine(context.Entry(newUser).State); // Detached

// 2. Added - New entity, will be inserted
await context.Users.AddAsync(newUser);
Console.WriteLine(context.Entry(newUser).State); // Added
await context.SaveChangesAsync(); // INSERT executed

// 3. Unchanged - Loaded but not modified
var user = context.Users.First();
Console.WriteLine(context.Entry(user).State); // Unchanged

// 4. Modified - Loaded and changed
user.Email = "modified@test.com";
Console.WriteLine(context.Entry(user).State); // Modified
await context.SaveChangesAsync(); // UPDATE executed

// 5. Deleted - Marked for deletion
context.Users.Remove(user);
Console.WriteLine(context.Entry(user).State); // Deleted
await context.SaveChangesAsync(); // DELETE executed
```

---

## Tracking vs NoTracking

### With Tracking (Default)

```csharp
var users = context.Users
    .Where(u => u.Active)
    .ToList();
// EF creates snapshots for ALL users in memory

users[0].Name = "Changed";
await context.SaveChangesAsync(); // UPDATE executed
```

**Memory Usage:** High (snapshots for every entity)
**CPU Usage:** Medium (change detection)
**When to use:** When modifying data

### Without Tracking

```csharp
var users = context.Users
    .AsNoTracking() // Don't track
    .Where(u => u.Active)
    .ToList();
// EF does NOT create snapshots

users[0].Name = "Changed";
await context.SaveChangesAsync(); // Nothing happens!
```

**Memory Usage:** Low (no snapshots)
**CPU Usage:** Low (no change detection)
**When to use:** Read-only queries, reporting

---

## Performance Impact

### Scenario: Load 10,000 Users

```csharp
// ❌ WITH TRACKING - Slow
var users = context.Users
    .Where(u => u.Active)
    .ToList();
// Time: ~500ms (includes change tracking)
// Memory: 50 MB (10,000 snapshots)

// ✅ WITHOUT TRACKING - Fast
var users = context.Users
    .AsNoTracking()
    .Where(u => u.Active)
    .ToList();
// Time: ~50ms (no change tracking)
// Memory: 5 MB (no snapshots)
```

---

## Batch Update - Change Tracking Issue

```csharp
// ❌ SLOW - Loads all entities
var users = context.Users
    .Where(u => u.Status == "Pending")
    .ToList();

foreach (var user in users)
{
    user.Status = "Active";
}
await context.SaveChangesAsync();
// Time: Load 1000 users + track + update = 5 seconds
// Memory: High (1000 snapshots)

// ✅ FAST - Direct SQL update
await context.Users
    .Where(u => u.Status == "Pending")
    .ExecuteUpdateAsync(s => s.SetProperty(u => u.Status, "Active"));
// Time: < 100ms (direct SQL)
// Memory: Minimal
```

---

## Checking What Changed

```csharp
var user = context.Users.First(u => u.Id == 1);
user.Email = "new@test.com";
user.Name = "Ahmed";

// Get original values
var entry = context.Entry(user);
var originalEmail = entry.OriginalValues["Email"]; // "old@test.com"
var originalName = entry.OriginalValues["Name"];   // "Ali"

// Get current values
var currentEmail = entry.CurrentValues["Email"];   // "new@test.com"
var currentName = entry.CurrentValues["Name"];     // "Ahmed"

// What properties changed?
var changedProperties = entry.GetDatabaseValues()
    .Properties
    .Where(p => !Equals(
        entry.OriginalValues[p.Name],
        entry.CurrentValues[p.Name]))
    .Select(p => p.Name)
    .ToList();
// Result: ["Email", "Name"]
```

---

## Disable Tracking Globally

```csharp
// Program.cs
builder.Services.AddDbContext<EHRDbContext>(options =>
    options
        .UseSqlServer("connection-string")
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking) // Default NoTracking
);

// Now all queries default to NoTracking
var users = context.Users.ToList(); // No tracking

// Explicitly enable tracking when needed
var users = context.Users
    .AsTracking()
    .ToList(); // Track this one
```

---

## Interview Q&A

**Q: What's the difference between tracking and no-tracking?**

A: 
- **Tracking**: EF stores snapshots to detect changes. More memory, slower, needed for updates.
- **NoTracking**: No snapshots, faster, less memory, for read-only queries.

**Q: How does EF know what changed?**

A: When entity loaded, EF creates snapshot of all properties. On SaveChanges, compares current values with snapshot. Changed properties get UPDATE SQL.

**Q: What happens if I modify an entity without calling SaveChanges?**

A: Nothing persists to database. Changes are only in memory. Next SaveChanges will detect them if entity still tracked.

**Q: Performance impact of large result sets?**

A: With tracking: 10,000 entities = 10,000 snapshots in memory = ~50MB + CPU overhead. Use AsNoTracking() for read-only.

**Q: When should you use AsNoTracking?**

A:
- Read-only queries (reports, dashboards)
- Large result sets
- Display data only
- Performance-critical queries
