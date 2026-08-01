namespace EHRPlatform.Services.Terminology.Application.Features.Codes.Commands;

using MediatR;
using EHRPlatform.Services.Terminology.Application.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for MapCodesCommand - Finds mappings between code systems.
/// </summary>
public class MapCodesCommandHandler : IRequestHandler<MapCodesCommand, MapCodesResponse>
{
    private readonly ICodeMappingService _mappingService;
    private readonly ILogger<MapCodesCommandHandler> _logger;

    public MapCodesCommandHandler(
        ICodeMappingService mappingService,
        ILogger<MapCodesCommandHandler> logger)
    {
        _mappingService = mappingService;
        _logger = logger;
    }

    public async Task<MapCodesResponse> Handle(MapCodesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Mapping codes from {SourceSystem} to {TargetSystem} for code {CodeId}", 
            request.SourceCodeSystem, request.TargetCodeSystem, request.SourceCodeId);

        var mappings = await _mappingService.GetMappingsAsync(
            request.SourceCodeId,
            request.SourceCodeSystem,
            request.TargetCodeSystem,
            cancellationToken);

        return new MapCodesResponse
        {
            SourceCodeId = request.SourceCodeId,
            SourceCodeSystem = request.SourceCodeSystem,
            TargetCodeSystem = request.TargetCodeSystem,
            Mappings = mappings
        };
    }
}
