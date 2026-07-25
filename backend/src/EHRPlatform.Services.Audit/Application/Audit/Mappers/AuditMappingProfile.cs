using Mapster;
using EHRPlatform.Services.Audit.Domain.Entities;
using EHRPlatform.Services.Audit.Application.Audit.Responses;

namespace EHRPlatform.Services.Audit.Application.Audit.Mappers;

/// <summary>
/// Mapster registration profile for Audit entity mappings.
/// </summary>
public class AuditMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AuditEntry, AuditEntryResponse>()
            .Map(dest => dest.Details, src => src.ChangeDetails);

        config.NewConfig<AccessLog, AccessLogResponse>()
            .Map(dest => dest.AccessedAt, src => src.AccessedAt);
    }
}
