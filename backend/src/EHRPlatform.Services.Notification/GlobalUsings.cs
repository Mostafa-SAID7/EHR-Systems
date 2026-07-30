// Global using directives
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.Extensions.Logging;
global using MediatR;
global using EHRPlatform.Common.Data;
global using EHRPlatform.Common.Infrastructure.EventDriven;
global using EHRPlatform.Services.Notification.Features.Notifications.Dtos.Responses;
// Domain entity type aliases: resolve 'Notification' ambiguity between namespace and class
global using NotificationEntity = EHRPlatform.Services.Notification.Domain.Entities.Notification;
global using NotificationPreference = EHRPlatform.Services.Notification.Domain.Entities.NotificationPreference;
global using NotificationTemplate = EHRPlatform.Services.Notification.Domain.Entities.NotificationTemplate;
// Domain events
global using EHRPlatform.Services.Notification.Domain.Events;

