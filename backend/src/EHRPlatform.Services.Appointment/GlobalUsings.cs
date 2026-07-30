global using Microsoft.Extensions.Logging;
global using EHRPlatform.Common.Infrastructure.EventDriven;
global using EHRPlatform.Common.Infrastructure.Security;
global using EHRPlatform.Services.Appointment.Data;
global using EHRPlatform.Services.Appointment.Data.Configuration;
global using EHRPlatform.Services.Appointment.Data.Seeds;
global using EHRPlatform.Services.Appointment.Domain.Events;
global using AppointmentReminder = EHRPlatform.Services.Appointment.Features.Appointments.Domain.AppointmentReminder;
global using ProviderAvailability = EHRPlatform.Services.Appointment.Features.Appointments.Domain.ProviderAvailability;
global using EHRPlatform.Services.Appointment.Domain.Enums;

// Application Layer - Updated to match reorganized structure
global using EHRPlatform.Services.Appointment.Application.Appointments;
global using EHRPlatform.Services.Appointment.Application.Appointments.Mappers;
global using EHRPlatform.Services.Appointment.Application.Appointments.Responses;
global using EHRPlatform.Services.Appointment.Application.Appointments.Requests;
global using EHRPlatform.Services.Appointment.Application.ProviderAvailability;
global using EHRPlatform.Services.Appointment.Application.ProviderAvailability.Mappers;
global using EHRPlatform.Services.Appointment.Application.ProviderAvailability.Responses;
global using EHRPlatform.Services.Appointment.Application.ProviderAvailability.Requests;

// Features Layer
global using EHRPlatform.Services.Appointment.Features.Appointments.Commands;
global using EHRPlatform.Services.Appointment.Features.Appointments.Queries;
global using EHRPlatform.Services.Appointment.Features.Appointments.Validation;
global using EHRPlatform.Services.Appointment.Features.ProviderAvailability.Commands;
global using EHRPlatform.Services.Appointment.Features.ProviderAvailability.Queries;
global using EHRPlatform.Services.Appointment.Features.ProviderAvailability.Validation;

// Type alias to resolve 'Appointment' vs namespace 'EHRPlatform.Services.Appointment' ambiguity
global using Appointment = EHRPlatform.Services.Appointment.Features.Appointments.Domain.Appointment;

