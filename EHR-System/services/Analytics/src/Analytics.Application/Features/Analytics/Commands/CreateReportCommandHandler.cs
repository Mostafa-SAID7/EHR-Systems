namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;
using EHRPlatform.BuildingBlocks.Security.CurrentUser;

/// <summary>
/// Handler for creating report
/// </summary>
public class CreateReportCommandHandler : IRequestHandler<CreateReportCommand, CreateReportResponse>
{
    private readonly IReportRepository _reportRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateReportCommandHandler> _logger;

    public CreateReportCommandHandler(
        IReportRepository reportRepository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<CreateReportCommandHandler> logger)
    {
        _reportRepository = reportRepository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<CreateReportResponse> Handle(
        CreateReportCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating report: {ReportName}", command.Name);

        try
        {
            var tenantId = _tenantContext.TenantId;
            if (tenantId == 0)
            {
                return new CreateReportResponse(
                    Success: false,
                    Message: "Tenant context not available");
            }

            var report = new Report
            {
                Id = Guid.NewGuid(),
                Name = command.Name,
                Description = command.Description,
                ReportType = command.ReportType,
                Configuration = command.Configuration,
                IsScheduled = command.IsScheduled,
                ScheduleCron = command.ScheduleCron,
                CreatedBy = _currentUserService.GetUserId(),
                CreatedAt = DateTime.UtcNow,
                TenantId = tenantId
            };

            var savedReport = await _reportRepository.AddAsync(report);

            _logger.LogInformation("Report created successfully: {ReportId}", savedReport.Id);

            return new CreateReportResponse(
                Success: true,
                Message: "Report created successfully",
                ReportId: savedReport.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating report: {ReportName}", command.Name);
            return new CreateReportResponse(
                Success: false,
                Message: $"Failed to create report: {ex.Message}");
        }
    }
}
