// Global using statements for Prescription service
// Common namespaces
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

// EHR Platform common
global using EHRPlatform.Common.CQRS;
global using EHRPlatform.Common.Data;
global using EHRPlatform.Common.Messaging;
global using EHRPlatform.Common.Mapping;

// Domain entities and enums
global using EHRPlatform.Services.Prescription.Domain.Entities;
global using EHRPlatform.Services.Prescription.Domain.Enums;

// Application layer
global using EHRPlatform.Services.Prescription.Application.PrescriptionManagement.Responses;
global using EHRPlatform.Services.Prescription.Application.PrescriptionManagement.Requests;
global using EHRPlatform.Services.Prescription.Application.PrescriptionManagement.Mappers;

// Data layer
global using EHRPlatform.Services.Prescription.Data;
