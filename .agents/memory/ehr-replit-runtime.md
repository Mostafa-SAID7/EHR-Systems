---
name: EHR Replit runtime setup
description: Replit-specific runtime and optional messaging requirements for the imported EHR platform.
---

The imported solution targets .NET 8, so Replit must declare the dotnet-8.0 module rather than relying on the older generated runtime. Angular's dev-server `allowedHosts` value is an array for the installed CLI schema.

When Kafka and RabbitMQ are not provisioned, backend services should start with messaging disabled or in-memory. Broker helpers that default blank configuration to localhost can fail during MassTransit bus validation even when the rest of the service is healthy.

**Why:** Replit provides PostgreSQL locally but does not automatically provide the platform's optional broker and search infrastructure; treating those services as mandatory makes an otherwise runnable local platform fail at startup.

**How to apply:** Keep PostgreSQL as the required startup dependency, gate external broker registration on explicit configuration, and preserve the external transport path when broker settings are supplied.