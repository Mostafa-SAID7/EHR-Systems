namespace EHRPlatform.Common.Infrastructure.Caching;

/// <summary>
/// Cache TTL (Time-To-Live) policies for different data types.
/// Defines how long different cached data should persist in Redis.
/// </summary>
public static class CacheTTLPolicy
{
    /// <summary>
    /// Session and temporary data (1 minute).
    /// For: Active user sessions, temporary computation results.
    /// </summary>
    public static TimeSpan ShortLived = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Frequently used data (5 minutes).
    /// For: Patient searches, appointment lists, current vital signs.
    /// </summary>
    public static TimeSpan Standard = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Medium-retention data (15 minutes).
    /// For: Patient demographics, appointment schedules, medication lists.
    /// </summary>
    public static TimeSpan MediumLived = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Long-retention data (1 hour).
    /// For: Reference data (ICD-10 codes, CPT codes), configuration, static lookups.
    /// </summary>
    public static TimeSpan LongLived = TimeSpan.FromHours(1);

    /// <summary>
    /// Very long retention (6 hours).
    /// For: Rarely changing master data, provider schedules, facility information.
    /// </summary>
    public static TimeSpan VeryLongLived = TimeSpan.FromHours(6);
}

