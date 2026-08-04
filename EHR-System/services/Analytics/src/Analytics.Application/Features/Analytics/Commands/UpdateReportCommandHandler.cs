namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Repositories;

/// <summary>
/// Handler for updating report
/// </summary>
public class UpdateReportCommandHandler : IRequestHandler<UpdateReportCommand, UpdateReportResponse>
{
    private readonly IReportRepository _reportRepository;
    private readonly ILogger<UpdateReportCommandHandler> _logger;

    public UpdateReportCommandHandler(
        IReportRepository reportRepository,
        ILogger<UpdateReportCommandHandler> logger)
    {
        _reportRepository = reportRepository;
        _logger = logger;
    }

    public async Task<UpdateReportResponse> Handle(
        UpdateReportCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating report: {ReportId}", command.ReportId);

        try
        {
            var report = await _reportRepository.GetByIdAsync(command.ReportId);
            if (report == null)
            {
                return new UpdateReportResponse(
                    Success: false,
                    Message: "Report not found");
            }

            // Update properties if provided
            if (!string.IsNullOrWhiteSpace(command.Name))
                report.Name = command.Name;

            if (!string.IsNullOrWhiteSpace(command.Description))
                report.Description = command.Description;

            if (!string.IsNullOrWhiteSpace(command.Configuration))
                report.Configuration = command.Configuration;

            if (command.IsScheduled.HasValue)
                report.IsScheduled = command.IsScheduled.Value;

            if (!string.IsNullOrWhiteSpace(command.ScheduleCron))
                report.ScheduleCron = command.ScheduleCron;

            report.UpdatedAt = DateTime.UtcNow;

            await _reportRepository.UpdateAsync(report);

            _logger.LogInformation("Report updated successfully: {ReportId}", command.ReportId);

            return new UpdateReportResponse(
                Success: true,
                Message: "Report updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating report: {ReportId}", command.ReportId);
            return new UpdateReportResponse(
                Success: false,
                Message: $"Failed to update report: {ex.Message}");
        }
    }
}
