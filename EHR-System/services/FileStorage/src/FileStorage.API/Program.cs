using Serilog;
using Amazon.S3;
using EHRPlatform.Services.FileStorage.Persistence;
using EHRPlatform.Services.FileStorage.Application.Features.Documents.Mappers;
using EHRPlatform.Services.FileStorage.Application.Features.Documents.Handlers;
using EHRPlatform.Services.FileStorage.Infrastructure.Storage;
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
        c.SwaggerDoc("v1", new() { Title = "FileStorage Service API", Version = "v1" });
    });

    // ── Database (PostgreSQL) ─────────────────────────────────────────────────
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
        ?? throw new InvalidOperationException("Database connection string not configured");

    builder.Services.AddDbContext<FileStorageContext>((sp, options) =>
    {
        options.UseNpgsql(connectionString, npgOptions =>
        {
            npgOptions.MigrationsHistoryTable("__EFMigrationsHistory", "filestorage");
        });
    }, ServiceLifetime.Scoped);

    // ── AWS S3 Configuration ──────────────────────────────────────────────────
    var s3BucketName = builder.Configuration["AWS:S3:BucketName"]
        ?? Environment.GetEnvironmentVariable("AWS_S3_BUCKET")
        ?? "ehr-documents";

    builder.Services.AddAWSService<IAmazonS3>();
    builder.Services.AddScoped(sp =>
        new S3StorageService(
            sp.GetRequiredService<IAmazonS3>(),
            s3BucketName,
            sp.GetRequiredService<ILogger<S3StorageService>>()));

    // ── MediatR - CQRS ────────────────────────────────────────────────────────
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
        typeof(GetDocumentQueryHandler).Assembly));

    // ── Application Services ──────────────────────────────────────────────────
    builder.Services.AddScoped<DocumentMapper>();

    // ── CORS ──────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
        options.AddPolicy("AllowAll", p => p
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));

    // ── Health Checks ─────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<FileStorageContext>("postgres-filestorage", tags: ["db", "postgres"]);

    // ── Build ─────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Apply Migrations ──────────────────────────────────────────────────────
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<FileStorageContext>();
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

    Log.Information("🏥 EHR FileStorage Service starting on port 5008");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "FileStorage Service terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
