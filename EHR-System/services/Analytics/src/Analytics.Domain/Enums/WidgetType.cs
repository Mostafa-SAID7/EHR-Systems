namespace EHRPlatform.Services.Analytics.Domain.Enums;

/// <summary>
/// Enumeration for widget types - specifies visualization type
/// </summary>
public enum WidgetType
{
    /// <summary>KPI card showing single metric</summary>
    KPI = 1,
    
    /// <summary>Line chart for time series data</summary>
    LineChart = 2,
    
    /// <summary>Bar chart for categorical data</summary>
    BarChart = 3,
    
    /// <summary>Pie chart for distribution data</summary>
    PieChart = 4,
    
    /// <summary>Gauge chart for range values</summary>
    Gauge = 5,
    
    /// <summary>Table/Grid widget</summary>
    Table = 6,
    
    /// <summary>Area chart</summary>
    AreaChart = 7,
    
    /// <summary>Scatter plot</summary>
    ScatterPlot = 8,
    
    /// <summary>Heat map</summary>
    HeatMap = 9,
    
    /// <summary>Text/Markdown widget</summary>
    Text = 10,
    
    /// <summary>Custom HTML widget</summary>
    CustomHTML = 11
}
