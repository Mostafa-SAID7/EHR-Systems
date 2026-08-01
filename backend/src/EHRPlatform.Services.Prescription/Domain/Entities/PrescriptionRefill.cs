using EHRPlatform.BuildingBlocks.SharedKernel.Entities;

namespace EHRPlatform.Services.Prescription.Domain.Entities;

/// <summary>
/// Prescription refill request.
/// </summary>
public class PrescriptionRefill : BaseEntity
{
    public Guid PrescriptionId { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? DeniedAt { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, Approved, Denied, Dispensed
    public string? PharmacyId { get; set; }
    public string? DenialReason { get; set; }
    public Prescription Prescription { get; set; } = null!;
}


