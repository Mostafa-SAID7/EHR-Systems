using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Analytics.Application.Analytics.Responses;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Features.Analytics.Commands;
using Mapster;

namespace EHRPlatform.Services.Analytics.Features.Analytics.Handlers;

public class CreateReportCommandHandler : ICommandHandler<CreateReportCommand, ReportResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateReportCommandHandler> _logger;

    public CreateReportCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateReportCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ReportResponse> Handle(CreateReportCommand command, CancellationToken ct)
    {
        _logger.LogInformation("Creating report template {Name}", command.Name);
        var report = new Report
        {
            Id = Guid.NewGuid(), 
            UserId = command.UserId,
            Name = command.Name, 
            Description = command.Description,
            ReportType = command.ReportType,
            Schedule = command.Schedule
        };
        var repo = _unitOfWork.Repository<Report>();
        await repo.AddAsync(report, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return report.Adapt<ReportResponse>();
    }
}

