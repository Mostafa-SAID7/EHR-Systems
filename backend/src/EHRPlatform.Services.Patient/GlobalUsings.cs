global using EHRPlatform.Common.Data;
global using EHRPlatform.Common.Infrastructure.EventDriven;
global using EHRPlatform.Services.Patient.Data;
global using EHRPlatform.Services.Patient.Data.Configuration;
global using EHRPlatform.Services.Patient.Data.Seeds;
global using EHRPlatform.Services.Patient.Domain.Entities;
global using EHRPlatform.Services.Patient.Domain.Events;
global using EHRPlatform.Services.Patient.Domain.Enums;
// Application layer — Patients (primary; PatientManagement is legacy, accessed explicitly where needed)
global using EHRPlatform.Services.Patient.Application.Patients.Mappers;
global using EHRPlatform.Services.Patient.Application.Patients.Responses;
global using EHRPlatform.Services.Patient.Application.Patients.Requests;
global using EHRPlatform.Services.Patient.Features.Patients.Commands;
global using EHRPlatform.Services.Patient.Features.Patients.Queries;
global using EHRPlatform.Services.Patient.Features.Patients.Validation;
// Type alias: resolves 'Patient' ambiguity between namespace 'EHRPlatform.Services.Patient'
// and class 'EHRPlatform.Services.Patient.Domain.Entities.Patient'
global using PatientEntity = EHRPlatform.Services.Patient.Domain.Entities.Patient;

