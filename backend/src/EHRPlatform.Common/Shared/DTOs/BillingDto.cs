using System;

namespace EHRPlatform.Common.Shared.DTOs
{
    /// <summary>
    /// Shared DTO for Invoice Communication
    /// </summary>
    public class InvoiceDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }  // e.g., "Draft", "Issued", "Paid", "Overdue", "Cancelled"
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
    }

    /// <summary>
    /// Shared DTO for Payment Communication
    /// </summary>
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid PatientId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }  // e.g., "Credit Card", "Insurance", "Cash"
        public string Status { get; set; }         // e.g., "Pending", "Completed", "Failed", "Refunded"
        public DateTime PaymentDate { get; set; }
    }

    /// <summary>
    /// Event: Invoice Generated
    /// Published by Billing Service when invoice is created
    /// Subscribed by: Notification (send invoice to patient), Audit, Analytics
    /// </summary>
    public class InvoiceGeneratedEvent
    {
        public Guid InvoiceId { get; set; }
        public Guid PatientId { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Payment Received
    /// Published by Billing Service when payment is processed
    /// Subscribed by: Notification (send receipt), Audit, Analytics, Patient Service
    /// </summary>
    public class PaymentReceivedEvent
    {
        public Guid PaymentId { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid PatientId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Invoice Overdue
    /// Published by Billing Service when invoice payment is overdue
    /// Subscribed by: Notification (send reminder), Audit
    /// </summary>
    public class InvoiceOverdueEvent
    {
        public Guid InvoiceId { get; set; }
        public Guid PatientId { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal OutstandingAmount { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Payment Failed
    /// Published by Billing Service when payment processing fails
    /// Subscribed by: Notification (notify patient of failure), Audit
    /// </summary>
    public class PaymentFailedEvent
    {
        public Guid PaymentId { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid PatientId { get; set; }
        public string FailureReason { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
