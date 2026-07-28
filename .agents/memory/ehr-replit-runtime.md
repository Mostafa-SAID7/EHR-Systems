---
name: EHR Replit runtime setup
description: Replit-specific runtime and optional messaging requirements for the imported EHR platform.
---

The imported solution targets .NET 8, so Replit must declare the dotnet-8.0 module rather than relying on the older generated runtime. Angular's dev-server `allowedHosts` value is an array for the installed CLI schema.

When Kafka and RabbitMQ are not provisioned, backend services should start with messaging disabled or in-memory. Broker helpers that default blank configuration to localhost can fail during MassTransit bus validation even when the rest of the service is healthy.

**Why:** Replit provides PostgreSQL locally but does not automatically provide the platform's optional broker and search infrastructure; treating those services as mandatory makes an otherwise runnable local platform fail at startup.

**How to apply:** Keep PostgreSQL as the required startup dependency, gate external broker registration on explicit configuration, and preserve the external transport path when broker settings are supplied.

## dotnet-ef tool on Nix
`dotnet-ef` must be installed with `dotnet tool install --global dotnet-ef --version 8.0.0`, but the Nix-provided .NET SDK is not at the system default path. Always set before invoking:
```
export DOTNET_ROOT=/nix/store/1blv644vinali34masnw6g5fjjjaa4y6-dotnet-sdk-8.0.416/share/dotnet
export PATH="$PATH:$HOME/.dotnet/tools"
```
Without `DOTNET_ROOT`, `dotnet-ef` exits with "libhostfxr.so not found."

## EF migrations design-time factories
Every service has `Data/Design/DesignTimeContextFactory.cs` implementing `IDesignTimeDbContextFactory<TContext>`. It reads `DESIGN_TIME_CONNECTION_STRING` or falls back to `Host=localhost;...`. This is what allows `dotnet-ef migrations add` to work without a live database.

## EnsureCreatedAsync removed
All nine service Program.cs files previously called both `RunMigrationsAsync` AND `EnsureCreatedAsync` in sequence. The duplicate `EnsureCreatedAsync` blocks have been removed. Schema is now managed exclusively through EF Core migrations.