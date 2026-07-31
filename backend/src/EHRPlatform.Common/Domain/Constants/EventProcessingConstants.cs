#nullable enable

namespace EHRPlatform.Common.Domain.Constants;

/// <summary>
/// Event processing and background job configuration constants.
/// Defines polling intervals, topics, and retry settings.
/// Single responsibility: Define event processing constants only.
/// </summary>
public static class EventProcessingConstants
{
    // ── Outbox Processing ─────────────────────────────────────────────────

    /// <summary>Poll interval in seconds for outbox event processing.</summary>
    public const int OutboxPollIntervalSeconds = 5;

    /// <summary>Maximum retry attempts for outbox event publishing.</summary>
    public const int OutboxMaxRetryAttempts = 5;

    // ── Dead Letter Queue ──────────────────────────────────────────────────

    /// <summary>Poll interval in seconds for dead letter queue processing.</summary>
    public const int DlqPollIntervalSeconds = 30;

    /// <summary>Prefix for dead letter queue topic names.</summary>
    public const string DlqTopicPrefix = "dlq";

    /// <summary>Maximum time to retain DLQ messages (in days).</summary>
    public const int DlqRetentionDays = 30;

    // ── CDC (Change Data Capture) ──────────────────────────────────────────

    /// <summary>Poll interval in seconds for CDC processing.</summary>
    public const int CdcPollIntervalSeconds = 10;

    /// <summary>Batch size for CDC change events.</summary>
    public const int CdcBatchSize = 100;
}
