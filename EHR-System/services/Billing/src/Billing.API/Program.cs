using Serilog;
using EHRPlatform.Services.Billing.Persistence;
using EHRPlatform.Services.Billing.Application.Features.Invoicing.Mappers;
using EHRPlatform.Services.Billing.Application.Features.Invoicing.Handlers;
using EHRPlatform.BuildingBlocks.Common.Caching;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;
using EHRPlatform.BuildingBlocks.Security.CurrentUser;
using EHRPlatform.BuildingBlocks.EventBus.Broker;
using MediatR;
using Microsoft.EntityFrameworkCore;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Logging ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, config) =>
        config.ReadFrom.Configuration(ctx.Configuration));

    // ── Controllers & Swagger ─────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "Billing Service API", Version = "v1" });
    });

    // ── Database (PostgreSQL) ─────────────────────────────────────────────────
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
        ?? throw new InvalidOperationException("Database connection string not configured");

    builder.Services.AddDbContext<BillingContext>((sp, options) =>
    {
        options.UseNpgsql(connectionString, npgOptions =>
        {
            npgOptions.MigrationsHistoryTable("__EFMigrationsHistory", "billing");
        });
    }, ServiceLifetime.Scoped);

    // ── MediatR - CQRS ────────────────────────────────────────────────────────
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
        typeof(GetInvoiceByNumberQueryHandler).Assembly));

    // ── Application Services ──────────────────────────────────────────────────
    builder.Services.AddScoped<InvoiceMapper>();

    // ── Authorization ─────────────────────────────────────────────────────────
    builder.Services.AddAuthorization();

    // ── Building-Blocks Services Integration ───────────────────────────────────
    builder.Services.AddScoped<ICacheService, CacheService>();
    builder.Services.AddScoped<ITenantContext, TenantContext>();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddScoped<IMessageBroker, MessageBroker>();

    // ── CORS ──────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
        options.AddPolicy("AllowAll", p => p
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));

    // ── Health Checks ─────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<BillingContext>("postgres-billing", tags: ["db", "postgres"]);

    // ── Build ─────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Apply Migrations ──────────────────────────────────────────────────────
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BillingContext>();
            Log.Information("Applying database migrations...");
            await context.Database.MigrateAsync();
            Log.Information("Database migrations applied successfully");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error applying database migrations");
        if (app.Environment.IsProduction())
            throw;
    }

    // ── Middleware ────────────────────────────────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();
    app.UseCors("AllowAll");
    app.UseRouting();
    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("🏥 EHR Billing Service starting on port 5007");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Billing Service terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
