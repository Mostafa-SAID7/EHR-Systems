namespace EHRPlatform.Services.Clinical.Application;

using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Commands;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Queries;

/// <summary>
/// Dependency injection configuration for Clinical Application layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Clinical Application services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR for CQRS - auto-registers all handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        // Explicitly register Clinical handlers
        // Commands
        services.AddScoped<IRequestHandler<CreateClinicalNoteCommand, Contracts.Responses.ClinicalNoteResponse>, CreateClinicalNoteCommandHandler>();
        services.AddScoped<IRequestHandler<UpdateSOAPCommand, Unit>, UpdateSOAPCommandHandler>();
        services.AddScoped<IRequestHandler<RecordVitalsCommand, Unit>, RecordVitalsCommandHandler>();
        services.AddScoped<IRequestHandler<AddDiagnosisCommand, Unit>, AddDiagnosisCommandHandler>();
        services.AddScoped<IRequestHandler<AddProcedureCommand, Unit>, AddProcedureCommandHandler>();
        services.AddScoped<IRequestHandler<FinalizeClinicalNoteCommand, Unit>, FinalizeClinicalNoteCommandHandler>();

        // Queries
        services.AddScoped<IRequestHandler<GetClinicalNoteQuery, Contracts.Responses.ClinicalNoteResponse>, GetClinicalNoteQueryHandler>();
        services.AddScoped<IRequestHandler<GetPatientClinicalTimelineQuery, PaginatedResponse<Contracts.Responses.ClinicalNoteResponse>>, GetPatientClinicalTimelineQueryHandler>();
        services.AddScoped<IRequestHandler<GetVitalSignsTimelineQuery, VitalSignsTimelineResponse>, GetVitalSignsTimelineQueryHandler>();
        
        return services;
    }
}
