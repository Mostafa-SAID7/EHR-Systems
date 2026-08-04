using EHRPlatform.Services.Analytics.Persistence;
using EHRPlatform.Services.Analytics.Infrastructure.Kafka;
using EHRPlatform.Services.Analytics.Infrastructure;
using EHRPlatform.BuildingBlocks.Common.Caching;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;
using EHRPlatform.BuildingBlocks.Security.CurrentUser;
using EHRPlatform.BuildingBlocks.EventBus.Broker;
using EHRPlatform.Observability.Telemetry;
using EHRPlatform.Observability.ErrorReporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MediatR;
using MassTransit;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("Analytics.Persistence")));
builder.Services.AddScoped<IAnalyticsDbContext>(sp => sp.GetRequiredService<AnalyticsDbContext>());

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    Assembly.Load("Analytics.Application")));

// Kafka/MassTransit
var kafkaHost = builder.Configuration["Kafka:Host"] ?? "kafka:9092";
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AnalyticsEventConsumer>();
    
    x.UsingKafka((context, cfg) =>
    {
        cfg.Host(kafkaHost);
        cfg.ReceiveEndpoint("analytics-service", e =>
        {
            e.ConfigureConsumer<AnalyticsEventConsumer>(context);
        });
        cfg.Subscribe<AnalyticsDomainEvent>();
    });
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtIssuer = builder.Configuration["Jwt:Issuer"];
        var jwtAudience = builder.Configuration["Jwt:Audience"];
        var jwtKey = builder.Configuration["Jwt:PublicKeyPath"];

        if (File.Exists(jwtKey))
        {
            var publicKey = File.ReadAllText(jwtKey);
            var rsa = System.Security.Cryptography.RSA.Create();
            rsa.ImportFromPem(publicKey.ToCharArray());

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(rsa),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        }
    });

// Authorization
builder.Services.AddAuthorization();

// Building-Blocks Services Integration
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IMessageBroker, MessageBroker>();

// Observability & Telemetry Services
builder.Services.AddSingleton<ApplicationMetrics>();
builder.Services.AddScoped<IPerformanceMonitor, PerformanceMonitor>();
builder.Services.AddScoped<ITelemetryService, TelemetryService>();
builder.Services.AddScoped<IErrorMetrics, ErrorMetrics>();
builder.Services.AddScoped<IErrorReporter, ErrorReporter>();

// Persistence Services
builder.Services.AddPersistenceServices();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AnalyticsDbContext>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Database migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
