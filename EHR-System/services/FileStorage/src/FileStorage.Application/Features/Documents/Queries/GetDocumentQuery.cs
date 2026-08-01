using MediatR;
using EHRPlatform.Services.FileStorage.Contracts.Responses;

namespace EHRPlatform.Services.FileStorage.Application.Features.Documents.Queries;

/// <summary>
/// Get document by ID query.
/// </summary>
public record GetDocumentQuery(Guid DocumentId) : IRequest<DocumentResponseDto?>
{
}
