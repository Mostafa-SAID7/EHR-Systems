namespace EHRPlatform.Common.Data.Models;

using EHRPlatform.Common.Shared.Utilities.Helpers;

/// <summary>
/// Elasticsearch search document for the Patient index.
/// Kept in Common so multiple services (Patient, Clinical, Gateway) can reference it.
/// </summary>
public sealed class PatientSearchDocument
{
    public string Id         { get; set; } = string.Empty;
    public string FirstName  { get; set; } = string.Empty;
    public string LastName   { get; set; } = string.Empty;
    public string FullName   { get; set; } = string.Empty;
    public string Email      { get; set; } = string.Empty;
    public string MRN        { get; set; } = string.Empty;
    public string Gender     { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Status     { get; set; } = string.Empty;
    public DateTime IndexedAt { get; set; } = DateTimeHelper.UtcNow;
}

