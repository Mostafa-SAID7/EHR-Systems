using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;
using EHRPlatform.Services.Patient.Application.Features.MedicalHistory.Commands;
using EHRPlatform.Services.Patient.Application.Features.MedicalHistory.Queries;

namespace EHRPlatform.Services.Patient.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR - auto-scans assembly for handlers
        services.AddMediatR(typeof(DependencyInjection));
        
        // Register AutoMapper
        services.AddAutoMapper(typeof(DependencyInjection));

        // Explicitly register MedicalHistory handlers
        // Commands
        services.AddScoped<IRequestHandler<AddMedicalHistoryCommand, AddMedicalHistoryResponse>, AddMedicalHistoryCommandHandler>();
        services.AddScoped<IRequestHandler<UpdateMedicalHistoryCommand, UpdateMedicalHistoryResponse>, UpdateMedicalHistoryCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteMedicalHistoryCommand, DeleteMedicalHistoryResponse>, DeleteMedicalHistoryCommandHandler>();

        // Queries
        services.AddScoped<IRequestHandler<GetPatientMedicalHistoryQuery, GetPatientMedicalHistoryResponse>, GetPatientMedicalHistoryQueryHandler>();
        services.AddScoped<IRequestHandler<SearchMedicalHistoryQuery, SearchMedicalHistoryResponse>, SearchMedicalHistoryQueryHandler>();
        
        return services;
    }
}
