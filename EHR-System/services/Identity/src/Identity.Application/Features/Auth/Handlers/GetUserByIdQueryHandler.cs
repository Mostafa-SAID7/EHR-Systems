#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.BuildingBlocks.SharedKernel.Exceptions;
using EHRPlatform.Services.Identity.Contracts.Responses;
using EHRPlatform.Services.Identity.Application.Mappers;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Features.Auth.Queries;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Identity.Application.Features.Auth.Handlers;

/// <summary>
/// Handler for get user by ID query.
/// Retrieves user details with roles and permissions.
/// Supports caching for performance.
/// </summary>
public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserResponseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IdentityMapper _mapper;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(
        IUnitOfWork uow,
        IdentityMapper mapper,
        ILogger<GetUserByIdQueryHandler> logger)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handle get user by ID query.
    /// </summary>
    public async Task<UserResponseDto> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get user query for user: {UserId}", request.UserId);

        var userRepo = _uow.Repository<User>();
        var user = await userRepo.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        if (!user.IsActive)
        {
            _logger.LogWarning("Get user attempted on inactive user: {UserId}", request.UserId);
            throw new UnauthorizedException("User account is inactive");
        }

        var response = _mapper.MapToResponse(user);

        _logger.LogInformation("User retrieved: {UserId}", request.UserId);

        return response;
    }
}




