using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;
using EHRPlatform.Services.FileStorage.Application.Features.Documents.Commands;
using EHRPlatform.Services.FileStorage.Application.Features.Documents.Handlers;
using EHRPlatform.Services.FileStorage.Application.Features.Documents.Queries;

namespace EHRPlatform.Services.FileStorage.Application;

/// <summary>
/// Dependency Injection configuration for FileStorage Service Application Layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers FileStorage service application services
    /// </summary>
    public static IServiceCollection AddFileStorageApplicationServices(
        this IServiceCollection services)
    {
        // MediatR registration - auto-scans assembly for handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        // Explicitly register document handlers
        // Queries
        services.AddScoped<IRequestHandler<GetDocumentQuery, Contracts.Responses.DocumentResponseDto?>, GetDocumentQueryHandler>();
        
        // Commands
        services.AddScoped<IRequestHandler<UploadDocumentCommand, UploadDocumentResponse>, UploadDocumentCommandHandler>();
        services.AddScoped<IRequestHandler<ScanDocumentCommand, ScanDocumentResponse>, ScanDocumentCommandHandler>();
        
        return services;
    }
}
