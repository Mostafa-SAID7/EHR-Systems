using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EHRPlatform.Common.Infrastructure.Security;

/// <summary>
/// Extension methods for configuring JWT Bearer authentication in EHR microservices.
/// All services use the same JWT issuer/audience for seamless API Gateway forwarding.
/// </summary>
public static class JwtExtensions
{
    /// <summary>
    /// Add JWT Bearer authentication with standard EHR token validation parameters.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="jwtSecret">Symmetric signing key (must be 32+ characters).</param>
    /// <param name="issuer">Expected token issuer (default: "ehr-platform").</param>
    /// <param name="audience">Expected token audience (default: "ehr-api").</param>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        string jwtSecret,
        string issuer = "ehr-platform",
        string audience = "ehr-api")
    {
        if (string.IsNullOrWhiteSpace(jwtSecret))
            throw new ArgumentException("JWT secret must not be empty.", nameof(jwtSecret));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey        = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer          = true,
                ValidIssuer             = issuer,
                ValidateAudience        = true,
                ValidAudience           = audience,
                ValidateLifetime        = true,
                ClockSkew               = TimeSpan.FromMinutes(1)
            };

            // Allow SignalR to receive the token via query string (websocket limitation)
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();
        return services;
    }
}

