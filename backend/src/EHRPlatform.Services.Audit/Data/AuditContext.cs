using Microsoft.EntityFrameworkCore;
using EHRPlatform.BuildingBlocks.Common.Data.Contexts;
using EHRPlatform.BuildingBlocks.Common.Events;
using EHRPlatform.Services.Audit.Domain.Entities;

namespace EHRPlatform.Services.Audit.Data;

/// <summary>
/// DbContext for Audit Service.
/// Manages audit logs, access logs, compliance reports (HIPAA-compliant).
/// </summary>
public class AuditContext : BaseDbContext
{
    public AuditContext(DbContextOptions<AuditContext> options) : base(options) { }

    public DbSet<AuditEntry> AuditEntries { get; set; } = null!;
    public DbSet<AccessLog> AccessLogs { get; set; } = null!;
    public DbSet<DataChangeAudit> DataChangeAudits { get; set; } = null!;
    public DbSet<ComplianceReport> ComplianceReports { get; set; } = null!;
    public DbSet<AuditLogExport> AuditLogExports { get; set; } = null!;
    
    // ✓ Outbox Event Pattern
    public DbSet<OutboxEvent> OutboxEvents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditContext).Assembly);
    }
}

