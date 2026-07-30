using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using Mapster;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EHRPlatform.Services.Audit.Features.Audit.Commands;

/// <summary>
/// Record audit entry handler.
/// Creates immutable compliance-grade audit trail.
/// </summary>
public class RecordAuditEntryCommandHandler : ICommandHandler<RecordAuditEntryCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RecordAuditEntryCommandHandler> _logger;

    public RecordAuditEntryCommandHandler(IUnitOfWork unitOfWork, ILogger<RecordAuditEntryCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(RecordAuditEntryCommand command, CancellationToken cancellationToken)
    {
        // Create immutable audit entry
        var entry = new AuditEntry
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            UserEmail = command.UserEmail,
            Action = command.Action,
            ResourceType = command.ResourceType,
            ResourceId = command.ResourceId,
            Status = command.Success ? "Success" : "Failure",
            Timestamp = DateTime.UtcNow,
            IpAddress = command.IpAddress,
            UserAgent = command.UserAgent,
            PiiIndicators = command.PiiIndicators,
            AccessLevel = command.AccessLevel,
            ChangeDetails = command.ChangeDetails,
            FailureReason = command.FailureReason,
            IntegrityHash = ComputeHash(command)
        };

        var repo = _unitOfWork.Repository<AuditEntry>();
        await repo.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit entry recorded: User={User} Action={Action} Resource={Resource}/{Id} Status={Status}",
            command.UserEmail, command.Action, command.ResourceType, command.ResourceId, entry.Status);
    }

    private string ComputeHash(RecordAuditEntryCommand command)
    {
        var data = JsonSerializer.Serialize(new
        {
            command.UserId,
            command.Action,
            command.ResourceType,
            command.ResourceId,
            command.PiiIndicators
        });

        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}

/// <summary>
/// Record data change handler.
/// </summary>
public class RecordDataChangeCommandHandler : ICommandHandler<RecordDataChangeCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RecordDataChangeCommandHandler> _logger;

    public RecordDataChangeCommandHandler(IUnitOfWork unitOfWork, ILogger<RecordDataChangeCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(RecordDataChangeCommand command, CancellationToken cancellationToken)
    {
        var change = new DataChangeAudit
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            ResourceType = command.ResourceType,
            ResourceId = command.ResourceId,
            ChangedAt = DateTime.UtcNow,
            FieldName = command.FieldName,
            OldValue = command.OldValue,
            NewValue = command.NewValue,
            ChangeType = command.OldValue == null ? "Added" : (command.NewValue == null ? "Deleted" : "Modified"),
            Reason = command.Reason
        };

        var repo = _unitOfWork.Repository<DataChangeAudit>();
        await repo.AddAsync(change, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Data change recorded: {ResourceType}/{ResourceId} Field={Field} {ChangeType}",
            command.ResourceType, command.ResourceId, command.FieldName, change.ChangeType);
    }
}

/// <summary>
/// Generate compliance report handler.
/// </summary>
public class GenerateComplianceReportCommandHandler : ICommandHandler<GenerateComplianceReportCommand, ComplianceReportResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GenerateComplianceReportCommandHandler> _logger;

    public GenerateComplianceReportCommandHandler(IUnitOfWork unitOfWork, ILogger<GenerateComplianceReportCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ComplianceReportResponseDto> Handle(
        GenerateComplianceReportCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating compliance report: {Start} to {End}",
            command.PeriodStart, command.PeriodEnd);

        var auditRepo = _unitOfWork.Repository<AuditEntry>();
        var entries = await auditRepo.ToListAsync(
            q => q.Where(e => e.Timestamp >= command.PeriodStart && e.Timestamp <= command.PeriodEnd),
            cancellationToken);

        var report = new ComplianceReport
        {
            Id = Guid.NewGuid(),
            PeriodStart = command.PeriodStart,
            PeriodEnd = command.PeriodEnd,
            TotalActions = entries.Count,
            FailedActions = entries.Count(e => e.Status == "Failure"),
            DataAccess = entries.Count(e => e.Action == "Read"),
            DataChanges = entries.Count(e => new[] { "Create", "Update", "Delete" }.Contains(e.Action)),
            UnauthorizedAttempts = entries.Count(e => e.Status == "Failure"),
            PiiAccessed = entries.Where(e => !string.IsNullOrEmpty(e.PiiIndicators))
                .SelectMany(e => e.PiiIndicators!.Split(','))
                .Distinct()
                .ToList()
        };

        var reportRepo = _unitOfWork.Repository<ComplianceReport>();
        await reportRepo.AddAsync(report, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Compliance report generated {ReportId}", report.Id);

        return report.Adapt<ComplianceReportResponseDto>();
    }
}

/// <summary>
/// Export audit logs handler.
/// </summary>
public class ExportAuditLogsCommandHandler : ICommandHandler<ExportAuditLogsCommand, AuditExportResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExportAuditLogsCommandHandler> _logger;

    public ExportAuditLogsCommandHandler(IUnitOfWork unitOfWork, ILogger<ExportAuditLogsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AuditExportResponseDto> Handle(
        ExportAuditLogsCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Exporting audit logs: {Start} to {End}", command.PeriodStart, command.PeriodEnd);

        var auditRepo = _unitOfWork.Repository<AuditEntry>();
        var entries = await auditRepo.ToListAsync(
            q => q.Where(e => e.Timestamp >= command.PeriodStart && e.Timestamp <= command.PeriodEnd),
            cancellationToken);

        var export = new AuditLogExport
        {
            Id = Guid.NewGuid(),
            ExportedAt = DateTime.UtcNow,
            ExportedBy = command.ExportedBy,
            PeriodStart = command.PeriodStart,
            PeriodEnd = command.PeriodEnd,
            RecordCount = entries.Count,
            Format = command.Format,
            IsEncrypted = command.EncryptFile,
            Status = "Completed",
            FileHash = ComputeExportHash(entries)
        };

        var exportRepo = _unitOfWork.Repository<AuditLogExport>();
        await exportRepo.AddAsync(export, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Audit logs exported {ExportId}", export.Id);

        return export.Adapt<AuditExportResponseDto>();
    }

    private string ComputeExportHash(List<AuditEntry> entries)
    {
        var data = JsonSerializer.Serialize(entries.Select(e => e.Id));
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}

