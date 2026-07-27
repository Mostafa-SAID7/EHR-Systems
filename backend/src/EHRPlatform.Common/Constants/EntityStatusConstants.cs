#nullable enable

namespace EHRPlatform.Common.Constants;

/// <summary>
/// Standard status values for Patient entity.
/// </summary>
public static class PatientStatus
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";
    public const string Transferred = "Transferred";
    public const string Deceased = "Deceased";

    public static readonly List<string> All = new() { Active, Inactive, Transferred, Deceased };
}

/// <summary>
/// Standard status values for Appointment entity.
/// </summary>
public static class AppointmentStatus
{
    public const string Scheduled = "Scheduled";
    public const string Confirmed = "Confirmed";
    public const string CheckedIn = "CheckedIn";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string NoShow = "NoShow";

    public static readonly List<string> All = new() { Scheduled, Confirmed, CheckedIn, Completed, Cancelled, NoShow };
    public static readonly List<string> Active = new() { Scheduled, Confirmed, CheckedIn };
    public static readonly List<string> Terminal = new() { Completed, Cancelled, NoShow };
}

/// <summary>
/// Standard status values for Invoice entity.
/// </summary>
public static class InvoiceStatus
{
    public const string Draft = "Draft";
    public const string Submitted = "Submitted";
    public const string PartiallyPaid = "PartiallyPaid";
    public const string Paid = "Paid";
    public const string Cancelled = "Cancelled";
    public const string Refunded = "Refunded";
    public const string Overdue = "Overdue";

    public static readonly List<string> All = new() { Draft, Submitted, PartiallyPaid, Paid, Cancelled, Refunded, Overdue };
    public static readonly List<string> Billable = new() { Submitted, PartiallyPaid, Paid };
}

/// <summary>
/// Standard status values for ClinicalNote entity.
/// </summary>
public static class ClinicalNoteStatus
{
    public const string Draft = "Draft";
    public const string Finalized = "Finalized";
    public const string Locked = "Locked";
    public const string Archived = "Archived";

    public static readonly List<string> All = new() { Draft, Finalized, Locked, Archived };
    public static readonly List<string> Editable = new() { Draft };
}

/// <summary>
/// Standard status values for Prescription entity.
/// </summary>
public static class PrescriptionStatus
{
    public const string Draft = "Draft";
    public const string Active = "Active";
    public const string Completed = "Completed";
    public const string Discontinued = "Discontinued";
    public const string Filled = "Filled";

    public static readonly List<string> All = new() { Draft, Active, Completed, Discontinued, Filled };
}

/// <summary>
/// Standard status values for InsuranceClaim entity.
/// </summary>
public static class ClaimStatus
{
    public const string Draft = "Draft";
    public const string Submitted = "Submitted";
    public const string Accepted = "Accepted";
    public const string Denied = "Denied";
    public const string Approved = "Approved";
    public const string Paid = "Paid";

    public static readonly List<string> All = new() { Draft, Submitted, Accepted, Denied, Approved, Paid };
}

/// <summary>
/// Standard notification statuses.
/// </summary>
public static class NotificationStatus
{
    public const string Pending = "Pending";
    public const string Sent = "Sent";
    public const string Delivered = "Delivered";
    public const string Read = "Read";
    public const string Failed = "Failed";

    public static readonly List<string> All = new() { Pending, Sent, Delivered, Read, Failed };
}

/// <summary>
/// Appointment types.
/// </summary>
public static class AppointmentType
{
    public const string Office = "Office";
    public const string Telehealth = "Telehealth";
    public const string Phone = "Phone";
    public const string Emergency = "Emergency";

    public static readonly List<string> All = new() { Office, Telehealth, Phone, Emergency };
}

/// <summary>
/// Payment methods.
/// </summary>
public static class PaymentMethod
{
    public const string CreditCard = "CreditCard";
    public const string Check = "Check";
    public const string ACH = "ACH";
    public const string Insurance = "Insurance";
    public const string Cash = "Cash";

    public static readonly List<string> All = new() { CreditCard, Check, ACH, Insurance, Cash };
}

/// <summary>
/// Gender values.
/// </summary>
public static class Gender
{
    public const string Male = "Male";
    public const string Female = "Female";
    public const string Other = "Other";
    public const string Prefer = "Prefer";

    public static readonly List<string> All = new() { Male, Female, Other, Prefer };
}

/// <summary>
/// Reminder methods.
/// </summary>
public static class ReminderMethod
{
    public const string Email = "Email";
    public const string SMS = "SMS";
    public const string InApp = "InApp";
    public const string Phone = "Phone";

    public static readonly List<string> All = new() { Email, SMS, InApp, Phone };
}

/// <summary>
/// Clinical encounter types.
/// </summary>
public static class EncounterType
{
    public const string Office = "Office";
    public const string Telehealth = "Telehealth";
    public const string Emergency = "Emergency";
    public const string Hospital = "Hospital";
    public const string Home = "Home";

    public static readonly List<string> All = new() { Office, Telehealth, Emergency, Hospital, Home };
}

/// <summary>
/// Severity levels for allergies and conditions.
/// </summary>
public static class SeverityLevel
{
    public const string Mild = "Mild";
    public const string Moderate = "Moderate";
    public const string Severe = "Severe";
    public const string Critical = "Critical";

    public static readonly List<string> All = new() { Mild, Moderate, Severe, Critical };
}

/// <summary>
/// Blood types.
/// </summary>
public static class BloodType
{
    public const string OPositive = "O+";
    public const string ONegative = "O-";
    public const string APositive = "A+";
    public const string ANegative = "A-";
    public const string BPositive = "B+";
    public const string BNegative = "B-";
    public const string ABPositive = "AB+";
    public const string ABNegative = "AB-";

    public static readonly List<string> All = new() { OPositive, ONegative, APositive, ANegative, BPositive, BNegative, ABPositive, ABNegative };
}
