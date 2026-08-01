using EHRPlatform.BuildingBlocks.Common.Data;
using MongoDB.Bson.Serialization.Attributes;

namespace EHRPlatform.Services.Patient.Data.Documents;

/// <summary>
/// MongoDB document for a Patient.
/// Embeds Allergies and Conditions directly — avoids JOIN queries and
/// reflects the natural "patient-as-document" shape.
/// EntityId = Patient domain Id (Guid).
/// </summary>
public sealed class PatientDocument : MongoBaseDocument
{
    [BsonElement("firstName")]  public string FirstName  { get; set; } = string.Empty;
    [BsonElement("lastName")]   public string LastName   { get; set; } = string.Empty;
    [BsonElement("email")]      public string Email      { get; set; } = string.Empty;
    [BsonElement("phoneNumber")]public string PhoneNumber{ get; set; } = string.Empty;
    [BsonElement("dateOfBirth")][BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime DateOfBirth { get; set; }
    [BsonElement("gender")]     public string Gender     { get; set; } = string.Empty;
    [BsonElement("mrn")]        public string MRN        { get; set; } = string.Empty;
    [BsonElement("bloodType")]  public string? BloodType { get; set; }
    [BsonElement("emergencyContact")] public string? EmergencyContact { get; set; }
    [BsonElement("emergencyPhone")]   public string? EmergencyPhone   { get; set; }
    [BsonElement("status")]     public string Status     { get; set; } = "Active";

    /// <summary>Embedded allergies — no JOIN required.</summary>
    [BsonElement("allergies")]
    public List<AllergyEmbedded> Allergies { get; set; } = new();

    /// <summary>Embedded conditions — no JOIN required.</summary>
    [BsonElement("conditions")]
    public List<ConditionEmbedded> Conditions { get; set; } = new();
}

public sealed class AllergyEmbedded
{
    [BsonElement("allergen")]  public string Allergen { get; set; } = string.Empty;
    [BsonElement("severity")]  public string Severity { get; set; } = string.Empty;
    [BsonElement("notes")]     public string? Notes   { get; set; }
    [BsonElement("addedAt")][BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}

public sealed class ConditionEmbedded
{
    [BsonElement("condition")]   public string Condition   { get; set; } = string.Empty;
    [BsonElement("icd10Code")]   public string ICD10Code   { get; set; } = string.Empty;
    [BsonElement("onsetDate")][BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? OnsetDate    { get; set; }
    [BsonElement("resolvedDate")][BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? ResolvedDate { get; set; }
}

