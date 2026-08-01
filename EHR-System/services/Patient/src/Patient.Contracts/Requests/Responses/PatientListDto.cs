namespace EHRPlatform.Services.Patient.Application.Patients.Responses;

/// <summary>
/// Paginated list of patients response DTO.
/// </summary>
public class PatientListDto
{
    public List<PatientResponse> Items { get; set; } = new();
    public int Total { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (Total + PageSize - 1) / PageSize;
}
