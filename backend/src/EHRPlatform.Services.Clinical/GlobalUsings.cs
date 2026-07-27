// Global usings for EHRPlatform.Services.Clinical
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Logging;
global using MediatR;

global using EHRPlatform.Common.Data;
global using EHRPlatform.Common.DTOs;
global using EHRPlatform.Common.Events;
global using EHRPlatform.Common.Messaging;
global using EHRPlatform.Services.Clinical.Domain.Entities;
global using EHRPlatform.Services.Clinical.Domain.Events;
global using EHRPlatform.Services.Clinical.Application.ClinicalNoteManagement.Responses;
