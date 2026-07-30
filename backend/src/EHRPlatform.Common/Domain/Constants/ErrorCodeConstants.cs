#nullable enable

namespace EHRPlatform.Common.Domain.Constants;

/// <summary>
/// Standard error codes for all services.
/// Used for client-side error handling and logging.
/// </summary>
public static class ErrorCode
{
    // Validation errors (4xx)
    public const string ValidationError = "VALIDATION_ERROR";
    public const string InvalidRequest = "INVALID_REQUEST";
    public const string MissingRequired = "MISSING_REQUIRED";
    public const string InvalidFormat = "INVALID_FORMAT";
    public const string InvalidValue = "INVALID_VALUE";

    // Authentication & Authorization (40x)
    public const string Unauthorized = "UNAUTHORIZED";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string TokenExpired = "TOKEN_EXPIRED";
    public const string TokenInvalid = "TOKEN_INVALID";
    public const string Forbidden = "FORBIDDEN";
    public const string InsufficientPermissions = "INSUFFICIENT_PERMISSIONS";

    // Resource errors (40x)
    public const string NotFound = "NOT_FOUND";
    public const string ResourceNotFound = "RESOURCE_NOT_FOUND";
    public const string AlreadyExists = "ALREADY_EXISTS";
    public const string Conflict = "CONFLICT";

    // Business rule violations (422)
    public const string BusinessRuleViolation = "BUSINESS_RULE_VIOLATION";
    public const string InvalidOperation = "INVALID_OPERATION";
    public const string InvalidStatus = "INVALID_STATUS";
    public const string InvalidTransition = "INVALID_TRANSITION";
    public const string DuplicateEntry = "DUPLICATE_ENTRY";

    // HIPAA/Compliance errors (403)
    public const string HIPAAViolation = "HIPAA_VIOLATION";
    public const string AccessDenied = "ACCESS_DENIED";
    public const string ComplianceViolation = "COMPLIANCE_VIOLATION";
    public const string AuditViolation = "AUDIT_VIOLATION";

    // Server errors (5xx)
    public const string InternalError = "INTERNAL_ERROR";
    public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
    public const string TimeoutError = "TIMEOUT_ERROR";
    public const string ExternalServiceError = "EXTERNAL_SERVICE_ERROR";
    public const string DatabaseError = "DATABASE_ERROR";

    // Specific domain errors
    public const string PatientNotFound = "PATIENT_NOT_FOUND";
    public const string AppointmentNotFound = "APPOINTMENT_NOT_FOUND";
    public const string InvoiceNotFound = "INVOICE_NOT_FOUND";
    public const string AppointmentConflict = "APPOINTMENT_CONFLICT";
    public const string InsufficientBalance = "INSUFFICIENT_BALANCE";
    public const string InsuranceClaimFailed = "INSURANCE_CLAIM_FAILED";
    public const string PrescriptionInvalid = "PRESCRIPTION_INVALID";
    public const string DrugInteraction = "DRUG_INTERACTION";
}

/// <summary>
/// Standard HTTP status code mappings.
/// </summary>
public static class HttpStatusMap
{
    public const int Ok = 200;
    public const int Created = 201;
    public const int Accepted = 202;
    public const int BadRequest = 400;
    public const int Unauthorized = 401;
    public const int Forbidden = 403;
    public const int NotFound = 404;
    public const int Conflict = 409;
    public const int UnprocessableEntity = 422;
    public const int InternalServerError = 500;
    public const int BadGateway = 502;
    public const int ServiceUnavailable = 503;
    public const int GatewayTimeout = 504;
}

