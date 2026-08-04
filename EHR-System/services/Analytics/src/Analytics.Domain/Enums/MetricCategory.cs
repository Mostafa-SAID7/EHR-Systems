namespace EHRPlatform.Services.Analytics.Domain.Enums;

/// <summary>
/// Enumeration for metric categories - classifies type of business metrics
/// </summary>
public enum MetricCategory
{
    /// <summary>Patient-related metrics</summary>
    Patients = 1,
    
    /// <summary>Appointment-related metrics</summary>
    Appointments = 2,
    
    /// <summary>Clinical data metrics</summary>
    Clinical = 3,
    
    /// <summary>Billing and financial metrics</summary>
    Billing = 4,
    
    /// <summary>Revenue and payment metrics</summary>
    Revenue = 5,
    
    /// <summary>Pharmacy-related metrics</summary>
    Pharmacy = 6,
    
    /// <summary>System performance metrics</summary>
    System = 7,
    
    /// <summary>User activity metrics</summary>
    UserActivity = 8,
    
    /// <summary>Custom business metrics</summary>
    Custom = 9
}
