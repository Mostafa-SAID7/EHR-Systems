namespace EHRPlatform.Services.Billing.Domain.Enums;

/// <summary>
/// Prior authorization lifecycle status.
/// Many procedures (surgeries, high-cost drugs, advanced imaging) require
/// payer approval before services are rendered.
/// </summary>
public enum PriorAuthStatus
{
    /// <summary>Authorization request submitted to payer, awaiting decision.</summary>
    Requested = 0,

    /// <summary>Payer approved — service may be rendered. Store auth number.</summary>
    Approved = 1,

    /// <summary>Payer denied — clinical appeal may be initiated.</summary>
    Denied = 2,

    /// <summary>Approval window has passed; must re-request before billing.</summary>
    Expired = 3,

    /// <summary>Procedure/drug does not require prior authorization for this payer.</summary>
    NotRequired = 4,

    /// <summary>Request is under clinical review by payer's medical team.</summary>
    PendingClinicalReview = 5
}
