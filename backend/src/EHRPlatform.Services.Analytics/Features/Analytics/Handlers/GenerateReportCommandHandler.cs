using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Common.Data.Implementations;
using EHRPlatform.Services.Analytics.Application.Analytics.Responses;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Features.Analytics.Commands;
using Mapster;

namespace EHRPlatform.Services.Analytics.Features.Analytics.Handlers;

public class GenerateReportCommandHandler : ICommandHandler<GenerateReportCommand, ReportResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GenerateReportCommandHandler> _logger;

    public GenerateReportCommandHandler(IUnitOfWork unitOfWork, ILogger<GenerateReportCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ReportResponse> Handle(GenerateReportCommand command, CancellationToken ct)
    {
        _logger.LogInformation("Generating report {ReportId}", command.ReportId);
        var reportRepo = _unitOfWork.Repository<Report>();
        var report = await reportRepo.FirstOrDefaultAsync(q => q.Where(r => r.Id == command.ReportId), ct)
            ?? throw new InvalidOperationException($"Report {command.ReportId} not found");

        var execution = new ReportExecution
        {
            Id = Guid.NewGuid(), 
            ReportId = report.Id,
            ExecutedAt = DateTime.UtcNow, 
            Status = "Completed", 
            RecordCount = 0
        };
        var execRepo = _unitOfWork.Repository<ReportExecution>();
        await execRepo.AddAsync(execution, ct);
        report.LastGeneratedAt = DateTime.UtcNow;
        await reportRepo.UpdateAsync(report, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Report generated {ExecutionId}", execution.Id);
        return report.Adapt<ReportResponse>();
    }
}


