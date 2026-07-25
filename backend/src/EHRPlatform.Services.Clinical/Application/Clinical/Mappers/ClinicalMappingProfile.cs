using Mapster;
using EHRPlatform.Services.Clinical.Domain.Entities;
using EHRPlatform.Services.Clinical.Application.Clinical.Responses;

namespace EHRPlatform.Services.Clinical.Application.Clinical.Mappers;

/// <summary>
/// Mapster registration profile for Clinical entities.
/// Replaces AutoMapper with Mapster for consistency with the rest of the codebase.
/// </summary>
public class ClinicalMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ClinicalNote, ClinicalNoteResponse>();
    }
}
