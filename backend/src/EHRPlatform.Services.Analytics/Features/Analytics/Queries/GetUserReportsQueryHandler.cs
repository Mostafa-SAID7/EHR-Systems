using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Features.Analytics.Dtos.Responses;
using Mapster;

namespace EHRPlatform.Services.Analytics.Features.Analytics.Queries;

/// <summary>Get all reports for a user. Single Responsibility: Retrieve and project all reports owned by the specified user.</summary>
public class GetUserReportsQueryHandler : IQueryHandler<GetUserReportsQuery, List<ReportResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUserReportsQueryHandler> _logger;
    public GetUserReportsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetUserReportsQueryHandler> logger) { _unitOfWork = unitOfWork; _logger = logger; }
    public async Task<List<ReportResponseDto>> Handle(GetUserReportsQuery request, CancellationToken ct)
    {
        var repo = _unitOfWork.Repository<Report>();
        var reports = await repo.ToListAsync(q => q.Where(r => r.UserId == request.UserId), ct);
        return reports.Adapt<List<ReportResponseDto>>();
    }
}

