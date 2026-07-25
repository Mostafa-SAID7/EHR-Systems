# Migrations - Database Schema Versioning

## What is a Migration?

A migration is a **version-controlled snapshot** of your database schema.

```
Entity Models (C#)
    ↓
Migration (auto-generated file)
    ↓
Database Schema (SQL)
```

---

## Migration Workflow

### 1. Create Migration

```bash
dotnet ef migrations add AddUserRoleColumn
```

Generates migration file: `{timestamp}_AddUserRoleColumn.cs`

```csharp
public partial class AddUserRoleColumn : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Role",
            table: "Users",
            type: "nvarchar(max)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Role",
            table: "Users");
    }
}
```

### 2. Apply Migration

```bash
dotnet ef database update
```

Executes `Up()` method → changes applied to database

### 3. Rollback Migration

```bash
dotnet ef database update PreviousMigrationName
```

Executes `Down()` method → reverts changes

---

## Common Migration Scenarios

### Add Column

```csharp
// Model change
public class User
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } // New field
}

// Generate migration
// dotnet ef migrations add AddPhoneNumberColumn

public override void Up(MigrationBuilder mb)
{
    mb.AddColumn<string>(
        name: "PhoneNumber",
        table: "Users",
        type: "nvarchar(20)",
        nullable: true);
}

public override void Down(MigrationBuilder mb)
{
    mb.DropColumn("PhoneNumber", "Users");
}
```

### Remove Column

```csharp
// Model change - remove property
public class User
{
    public int Id { get; set; }
    // public string PhoneNumber { get; set; } // Removed
}

// dotnet ef migrations add RemovePhoneNumberColumn

public override void Up(MigrationBuilder mb)
{
    mb.DropColumn("PhoneNumber", "Users");
}

public override void Down(MigrationBuilder mb)
{
    mb.AddColumn<string>("PhoneNumber", "Users", nullable: true);
}
```

### Rename Column

```csharp
public override void Up(MigrationBuilder mb)
{
    mb.RenameColumn("OldName", "Users", "NewName");
}

public override void Down(MigrationBuilder mb)
{
    mb.RenameColumn("NewName", "Users", "OldName");
}
```

### Add Index

```csharp
public class User
{
    [Index(nameof(Email), IsUnique = true)]
    public string Email { get; set; }
}

// dotnet ef migrations add AddEmailIndex

public override void Up(MigrationBuilder mb)
{
    mb.CreateIndex(
        name: "IX_Users_Email",
        table: "Users",
        column: "Email",
        unique: true);
}

public override void Down(MigrationBuilder mb)
{
    mb.DropIndex("IX_Users_Email", "Users");
}
```

### Add Foreign Key Relationship

```csharp
public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; } // New FK
    public User User { get; set; }  // Navigation
}

public override void Up(MigrationBuilder mb)
{
    mb.AddColumn<int>(
        name: "UserId",
        table: "Orders",
        nullable: false);

    mb.CreateIndex(
        name: "IX_Orders_UserId",
        table: "Orders",
        column: "UserId");

    mb.AddForeignKey(
        name: "FK_Orders_Users_UserId",
        table: "Orders",
        column: "UserId",
        principalTable: "Users",
        principalColumn: "Id");
}
```

---

## Migration Best Practices

### ✅ Good Practices

```bash
# Descriptive names
dotnet ef migrations add AddUserPhoneAndAddress

# One logical change per migration
dotnet ef migrations add AddPhoneColumn
dotnet ef migrations add AddAddressColumn

# Run locally first
dotnet ef database update

# Review generated migration before deploying
# Check .cs file for correctness
```

### ❌ Bad Practices

```bash
# Generic names
dotnet ef migrations add Update # Bad!

# Multiple unrelated changes
dotnet ef migrations add AddColumnAndRemoveTable # Avoid

# Don't modify migration files manually
# Let EF regenerate if changes needed
```

---

## Handling Existing Database

### Option 1: Scaffold from Existing Database

```bash
dotnet ef dbcontext scaffold "connection-string" Microsoft.EntityFrameworkCore.SqlServer -o Models
```

Generates DbContext and entities from existing schema

### Option 2: Create Initial Migration from Current Schema

```csharp
// Configure your entities as they currently are

// Then:
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## Seeding Data with Migrations

```csharp
public partial class SeedDefaultRoles : Migration
{
    protected override void Up(MigrationBuilder mb)
    {
        // Insert default roles
        mb.Sql(@"
            INSERT INTO Roles (Name, Description) VALUES
            ('Admin', 'Administrator'),
            ('Doctor', 'Medical Doctor'),
            ('Nurse', 'Nurse'),
            ('Patient', 'Patient')
        ");
    }

    protected override void Down(MigrationBuilder mb)
    {
        mb.Sql("DELETE FROM Roles WHERE Name IN ('Admin', 'Doctor', 'Nurse', 'Patient')");
    }
}
```

---

## Production Deployment

### Generate SQL Script

```bash
dotnet ef migrations script
# Generates SQL that can be reviewed before running
```

### Idempotent Script (Safe for Repeated Runs)

```bash
dotnet ef migrations script --idempotent
# Checks if migration already applied before running
```

### Apply Migrations in Production

```bash
dotnet ef database update --connection "prod-connection-string"
```

---

## Troubleshooting Migrations

### Migration Not Found

```bash
# Check available migrations
dotnet ef migrations list

# Remove last unapplied migration
dotnet ef migrations remove
```

### Database Out of Sync

```bash
# View current schema in database
dotnet ef dbcontext info

# Revert to specific point
dotnet ef database update SpecificMigrationName

# Update to latest
dotnet ef database update
```

### Merge Conflicts in Migrations

```csharp
// When two developers create migrations simultaneously
// Manual Resolution:
// 1. Combine both Up() methods
// 2. Create new migration
// 3. Delete conflicting ones

dotnet ef migrations add MergePreviousMigrations
```

---

## Interview Q&A

**Q: What's a migration?**

A: A version-controlled file that defines database schema changes. Contains Up() to apply changes and Down() to rollback.

**Q: How do you rollback a migration?**

A: `dotnet ef database update PreviousMigrationName` executes Down() method.

**Q: Can you modify migration files?**

A: If not yet applied to database, yes. If already applied, create new migration instead.

**Q: How to handle migrations in production?**

A: Generate SQL script first (`dotnet ef migrations script`), review it, then apply to production safely.

**Q: What if database and code are out of sync?**

A: Check with `dotnet ef dbcontext info`. Revert to matching point or regenerate migrations.
