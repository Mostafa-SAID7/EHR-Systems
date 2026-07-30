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
global using EHRPlatform.Common.Application.CQRS;
global using EHRPlatform.Common.Data;
global using EHRPlatform.Common.Infrastructure.EventDriven;
global using EHRPlatform.Common.Application.Mapping;

// Domain entities and enums
global using EHRPlatform.Services.Prescription.Domain.Entities;
global using EHRPlatform.Services.Prescription.Domain.Enums;
// Type alias: resolves 'Prescription' ambiguity between namespace 'EHRPlatform.Services.Prescription'
// and class 'EHRPlatform.Services.Prescription.Domain.Entities.Prescription'
global using PrescriptionEntity = EHRPlatform.Services.Prescription.Domain.Entities.Prescription;
global using EHRPlatform.Services.Prescription.Domain.Events;

// Application layer
global using EHRPlatform.Services.Prescription.Application.PrescriptionManagement.Responses;
global using EHRPlatform.Services.Prescription.Application.PrescriptionManagement.Requests;
global using EHRPlatform.Services.Prescription.Application.PrescriptionManagement.Mappers;

// Data layer
global using EHRPlatform.Services.Prescription.Data;

