namespace EHRPlatform.Common.Integrations;

/// <summary>
/// FHIR R4 interoperability gateway interface for export/import of clinical data.
/// </summary>
public interface IFhirGateway
{
    /// <summary>Exports patient demographics as FHIR R4 Patient JSON resource.</summary>
    Task<string> ExportPatientAsync(Guid patientId, CancellationToken cancellationToken = default);

    /// <summary>Exports clinical note/encounter as FHIR R4 Encounter Bundle JSON resource.</summary>
    Task<string> ExportEncounterAsync(Guid clinicalNoteId, CancellationToken cancellationToken = default);
}
