namespace EHRPlatform.Common.Application.Features.Healthcare.Messaging;

/// <summary>
/// HL7 / X12 EDI message builder for healthcare insurance claims and clinical messaging.
/// </summary>
public interface IHl7MessageBuilder
{
    /// <summary>Builds X12 EDI 837 Health Care Claim transaction set string.</summary>
    string BuildX12_837(Guid claimId, string payerId, string memberId, decimal amount, IEnumerable<string> cptCodes);

    /// <summary>Builds HL7 v2.x ADT^A08 (Update Patient Information) message string.</summary>
    string BuildHl7AdtA08(Guid patientId, string patientName, DateTime dob, string gender);
}
