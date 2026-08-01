using Mapster;
using EHRPlatform.BuildingBlocks.Common.Application.Mapping;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Billing.Application.Claims.Mappers;

/// <summary>
/// Claim Mapper
/// Single Responsibility: Convert between InsuranceClaim domain model and DTOs.
/// Handles only Claims feature mappings.
/// </summary>
public class ClaimMapper : MappingServiceBase<InsuranceClaim, ClaimResponseDto>
{
    public ClaimMapper(ILogger<ClaimMapper> logger) : base(logger)
    {
    }

    /// <summary>
    /// Map single claim to response DTO.
    /// </summary>
    public ClaimResponseDto MapToResponseDto(InsuranceClaim claim)
    {
        return MapSingleToDto(claim);
    }

    /// <summary>
    /// Map collection of claims to response DTO list.
    /// </summary>
    public List<ClaimResponseDto> MapToResponseDtoList(ICollection<InsuranceClaim> claims)
    {
        Logger.LogDebug("Mapping {Count} claims to response DTO list", claims.Count);
        return claims.Adapt<List<ClaimResponseDto>>();
    }
}


