#nullable enable

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace EHRPlatform.Common.Shared.Localization;

/// <summary>
/// EHR Platform localization configuration — English (default) and Arabic (RTL).
///
/// Usage in microservice Program.cs:
///   builder.Services.AddEHRLocalization();
///   ...
///   app.UseEHRLocalization();
///
/// Locale selection order:
///   1. Accept-Language request header
///   2. Query string ?culture=ar
///   3. Cookie .AspNetCore.Culture
///   4. Default: en-US
/// </summary>
public static class LocalizationExtensions
{
    public static readonly CultureInfo English = new("en-US");
    public static readonly CultureInfo Arabic   = new("ar-SA");

    public static readonly CultureInfo[] SupportedCultures = [English, Arabic];

    /// <summary>Register localization services.</summary>
    public static IServiceCollection AddEHRLocalization(this IServiceCollection services)
    {
        services.AddLocalization(opts => opts.ResourcesPath = "Resources");

        services.Configure<RequestLocalizationOptions>(opts =>
        {
            opts.DefaultRequestCulture = new RequestCulture(English);
            opts.SupportedCultures      = SupportedCultures;
            opts.SupportedUICultures    = SupportedCultures;

            // Accept-Language header has highest priority after explicit query string/cookie
            opts.RequestCultureProviders =
            [
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider()
            ];
        });

        return services;
    }

    /// <summary>Wire up localization middleware. Call before UseRouting.</summary>
    public static IApplicationBuilder UseEHRLocalization(this IApplicationBuilder app)
    {
        app.UseRequestLocalization();
        return app;
    }
}

