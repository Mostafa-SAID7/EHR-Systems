#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data;
using EHRPlatform.Services.Identity.Application.Identity.DTOs.Responses;
using EHRPlatform.Services.Identity.Application.Identity.Mappers;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Features.Users.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Identity.Features.Users.Handlers;

/// <summary>
/// Handler for get users query (paginated).
/// Retrieves paginated list of users with optional filtering.
/// Supports caching for performance.
/// </summary>
public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, GetUsersResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly IdentityMapper _mapper;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    public GetUsersQueryHandler(
        IUnitOfWork uow,
        IdentityMapper mapper,
        ILogger<GetUsersQueryHandler> logger)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handle get users query.
    /// </summary>
    public async Task<GetUsersResponse> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get users query: page {PageNumber}, pageSize {PageSize}, search {SearchTerm}",
            request.PageNumber, request.PageSize, request.SearchTerm);

        var userRepo = _uow.Repository<User>();

        // Build predicate for filtering
        IQueryable<User> query = userRepo.AsQueryable();

        // Filter by active status if specified
        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        // Filter by search term
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(searchLower) ||
                u.FirstName.ToLower().Contains(searchLower) ||
                u.LastName.ToLower().Contains(searchLower));
        }

        // Get total count
        int totalCount = await query.CountAsync();

        // Apply pagination
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var response = _mapper.MapToListDto(users, totalCount, request.PageNumber, request.PageSize);

        _logger.LogInformation("Users retrieved: {Count} of {Total}", users.Count, totalCount);

        return response;
    }
}


