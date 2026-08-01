namespace EHRPlatform.Services.FileStorage.Contracts.Requests;

/// <summary>
/// Request DTO for uploading a document.
/// </summary>
public class UploadDocumentRequestDto
{
    public Guid PatientId { get; set; }
    public string Category { get; set; } = "Other"; // LabResult, Prescription, Imaging, Note, etc.
    public string Classification { get; set; } = "PHI"; // PHI, Public, Confidential
    public string? Description { get; set; }
    public IFormFile File { get; set; } = null!;
}
