namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;

/// <summary>
/// Handler for executing report
/// </summary>
public class ExecuteReportCommandHandler : IRequestHandler<ExecuteReportCommand, ExecuteReportResponse>
{
    private readonly IReportRepository _reportRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ExecuteReportCommandHandler> _logger;

    public ExecuteReportCommandHandler(
        IReportRepository reportRepository,
        ITenantContext tenantContext,
        ILogger<ExecuteReportCommandHandler> logger)
    {
        _reportRepository = reportRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<ExecuteReportResponse> Handle(
        ExecuteReportCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing report: {ReportId}", command.ReportId);

        try
        {
            var tenantId = _tenantContext.TenantId;

            // Get report
            var report = await _reportRepository.GetByIdAsync(command.ReportId);
            if (report == null)
            {
                return new ExecuteReportResponse(
                    Success: false,
                    Message: "Report not found");
            }

            // Create execution record
            var execution = new ReportExecution
            {
                Id = Guid.NewGuid(),
                ReportId = command.ReportId,
                StartedAt = DateTime.UtcNow,
                Status = "Running",
                Parameters = command.Parameters
            };

            // In production, this would queue an async job
            // For now, we'll mark as completed immediately
            execution.CompletedAt = DateTime.UtcNow;
            execution.Status = "Completed";
            execution.OutputLocation = $"reports/{command.ReportId}/{execution.Id}.csv";

            report.Executions ??= new();
            report.Executions.Add(execution);

            await _reportRepository.UpdateAsync(report);

            _logger.LogInformation("Report executed successfully: {ExecutionId}", execution.Id);

            return new ExecuteReportResponse(
                Success: true,
                Message: "Report executed successfully",
                ExecutionId: execution.Id,
                OutputLocation: execution.OutputLocation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing report: {ReportId}", command.ReportId);
            return new ExecuteReportResponse(
                Success: false,
                Message: $"Failed to execute report: {ex.Message}");
        }
    }
}
