using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Features.Analytics.Dtos.Responses;
using Mapster;

namespace EHRPlatform.Services.Analytics.Features.Analytics.Queries;

/// <summary>Get a single report by ID. Single Responsibility: Fetch and project one report record.</summary>
public class GetReportQueryHandler : IQueryHandler<GetReportQuery, ReportResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetReportQueryHandler> _logger;
    public GetReportQueryHandler(IUnitOfWork unitOfWork, ILogger<GetReportQueryHandler> logger) { _unitOfWork = unitOfWork; _logger = logger; }
    public async Task<ReportResponseDto> Handle(GetReportQuery request, CancellationToken ct)
    {
        var repo = _unitOfWork.Repository<Report>();
        var report = await repo.FirstOrDefaultAsync(q => q.Where(r => r.Id == request.ReportId), ct)
            ?? throw new InvalidOperationException($"Report {request.ReportId} not found");
        return report.Adapt<ReportResponseDto>();
    }
}
