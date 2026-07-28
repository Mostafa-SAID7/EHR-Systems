using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EHRPlatform.Services.Prescription.Data.Design;

/// <summary>
/// Design-time factory for EF Core CLI tooling (dotnet ef migrations add / update).
/// Only invoked by the EF tools — never used at runtime.
/// </summary>
public sealed class DesignTimeContextFactory : IDesignTimeDbContextFactory<PrescriptionContext>
{
    public PrescriptionContext CreateDbContext(string[] args)
    {
        var connStr = Environment.GetEnvironmentVariable("DESIGN_TIME_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=ehr_prescription_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<PrescriptionContext>()
            .UseNpgsql(connStr)
            .Options;

        return new PrescriptionContext(options);
    }
}
