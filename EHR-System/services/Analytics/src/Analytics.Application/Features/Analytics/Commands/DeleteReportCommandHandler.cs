namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Repositories;

/// <summary>
/// Handler for deleting report
/// </summary>
public class DeleteReportCommandHandler : IRequestHandler<DeleteReportCommand, DeleteReportResponse>
{
    private readonly IReportRepository _reportRepository;
    private readonly ILogger<DeleteReportCommandHandler> _logger;

    public DeleteReportCommandHandler(
        IReportRepository reportRepository,
        ILogger<DeleteReportCommandHandler> logger)
    {
        _reportRepository = reportRepository;
        _logger = logger;
    }

    public async Task<DeleteReportResponse> Handle(
        DeleteReportCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting report: {ReportId}", command.ReportId);

        try
        {
            var report = await _reportRepository.GetByIdAsync(command.ReportId);
            if (report == null)
            {
                return new DeleteReportResponse(
                    Success: false,
                    Message: "Report not found");
            }

            await _reportRepository.DeleteAsync(command.ReportId);

            _logger.LogInformation("Report deleted successfully: {ReportId}", command.ReportId);

            return new DeleteReportResponse(
                Success: true,
                Message: "Report deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting report: {ReportId}", command.ReportId);
            return new DeleteReportResponse(
                Success: false,
                Message: $"Failed to delete report: {ex.Message}");
        }
    }
}
