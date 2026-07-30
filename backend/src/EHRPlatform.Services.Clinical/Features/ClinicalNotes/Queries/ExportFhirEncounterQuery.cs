using MediatR;
using EHRPlatform.Services.Clinical.Application.Mappers;
using EHRPlatform.Services.Clinical.Domain.Entities;
using EHRPlatform.Common.Data.Abstractions;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Queries;

public record ExportFhirEncounterQuery(Guid NoteId) : IRequest<string>;

public class ExportFhirEncounterQueryHandler : IRequestHandler<ExportFhirEncounterQuery, string>
{
    private readonly IUnitOfWork _unitOfWork;

    public ExportFhirEncounterQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(ExportFhirEncounterQuery request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<ClinicalNote>();
        var note = await repo.FirstOrDefaultAsync(
            q => q.Where(n => n.Id == request.NoteId),
            cancellationToken);

        if (note == null)
            throw new InvalidOperationException($"Clinical Note {request.NoteId} not found.");

        return FhirEncounterMapper.ToFhirR4BundleJson(note);
    }
}
