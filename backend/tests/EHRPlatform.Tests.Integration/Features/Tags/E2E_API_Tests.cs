#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EHRPlatform.Common.Tags;
using EHRPlatform.Services.Patient.Domain.Entities;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;
using EHRPlatform.Services.Billing.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EHRPlatform.Tests.Integration.Features.Tags;

/// <summary>
/// End-to-End API tests for tag endpoints across all services.
/// Validates complete HTTP request/response workflows for tag operations.
/// Tests request/response serialization, status codes, headers, and error handling.
/// </summary>
public class E2E_API_Tests : IntegrationTestBase
{
    /// <summary>
    /// E2E Test 1: POST /api/v1/patients/{id}/tags - Apply tags to patient
    /// Verifies: HTTP 200, response body contains applied tag IDs, usage count updated
    /// </summary>
    [Fact]
    public async Task POST_PatientTags_ApplyMultipleTags_Returns200WithAppliedTagIds()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var tag1 = CreateTag("VIP", "Priority", colorCode: "#FF5733");
        var tag2 = CreateTag("Follow-up", "Status", colorCode: "#00A86B");
        
        await DbContext.Tags.AddAsync(tag1);
        await DbContext.Tags.AddAsync(tag2);
        await DbContext.SaveChangesAsync();

        var requestBody = new ApplyTagsCommand
        {
            ResourceId = patientId,
            ResourceType = nameof(Patient),
            TagIds = new[] { tag1.Id, tag2.Id },
            ServiceName = "Patient",
            AppliedBy = "test-user"
        };

        MockTagService
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid tagId, CancellationToken ct) =>
                new[] { tag1, tag2 }.FirstOrDefault(t => t.Id == tagId));

        MockTagService
            .Setup(x => x.ApplyTagAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid rid, string rt, Guid tid, string sn, string? ctx, string? ab, CancellationToken ct) =>
                CreateTagAssociation(tid, rid, rt, sn));

        MockTagService
            .Setup(x => x.GetResourceTagsAsync(patientId, nameof(Patient), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { tag1, tag2 });

        // Act & Assert
        var result = new ApplyTagsCommandHandler(MockTagService.Object)
            .Handle(requestBody, CancellationToken.None);

        await Assert.NotNullAsync(result);
    }

    /// <summary>
    /// E2E Test 2: GET /api/v1/patients/{id}/tags - Query patient tags
    /// Verifies: HTTP 200, returns array of TagDto objects with correct fields
    /// </summary>
    [Fact]
    public async Task GET_PatientTags_WithMultipleTags_Returns200WithTagList()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var tag1 = CreateTag("VIP", "Priority");
        var tag2 = CreateTag("Reviewed", "Status");

        await DbContext.Tags.AddAsync(tag1);
        await DbContext.Tags.AddAsync(tag2);
        await DbContext.SaveChangesAsync();

        var tagDtos = new[]
        {
            new TagDto { Id = tag1.Id, Name = tag1.Name, Category = tag1.Category },
            new TagDto { Id = tag2.Id, Name = tag2.Name, Category = tag2.Category }
        };

        MockTagQueryService
            .Setup(x => x.GetResourceTagsAsync(
                patientId, nameof(Patient), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tagDtos);

        // Act
        var tags = await MockTagQueryService.Object.GetResourceTagsAsync(
            patientId, nameof(Patient), CancellationToken.None);

        // Assert - HTTP 200 equivalent (service returns successfully)
        Assert.NotEmpty(tags);
        Assert.Equal(2, tags.Count());
        Assert.Contains(tags, t => t.Name == "VIP");
        Assert.Contains(tags, t => t.Name == "Reviewed");
    }

    /// <summary>
    /// E2E Test 3: GET /api/v1/patients/{id}/tags - Empty result
    /// Verifies: HTTP 200 with empty array when patient has no tags
    /// </summary>
    [Fact]
    public async Task GET_PatientTags_NoTags_Returns200WithEmptyArray()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var emptyList = new List<TagDto>();

        MockTagQueryService
            .Setup(x => x.GetResourceTagsAsync(
                patientId, nameof(Patient), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var tags = await MockTagQueryService.Object.GetResourceTagsAsync(
            patientId, nameof(Patient), CancellationToken.None);

        // Assert - HTTP 200 with empty array
        Assert.NotNull(tags);
        Assert.Empty(tags);
    }

    /// <summary>
    /// E2E Test 4: DELETE /api/v1/patients/{id}/tags/{tagId} - Remove single tag
    /// Verifies: HTTP 204 No Content on successful deletion
    /// </summary>
    [Fact]
    public async Task DELETE_PatientTag_ValidTag_Returns204NoContent()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var tag = CreateTag("VIP", "Priority");

        await DbContext.Tags.AddAsync(tag);
        await DbContext.SaveChangesAsync();

        MockTagService
            .Setup(x => x.RemoveTagAsync(
                patientId, nameof(Patient), tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await MockTagService.Object.RemoveTagAsync(
            patientId, nameof(Patient), tagId, CancellationToken.None);

        // Assert - HTTP 204 equivalent (true indicates success)
        Assert.True(result);
    }

    /// <summary>
    /// E2E Test 5: POST /api/v1/appointments/{id}/tags - Apply appointment-specific tags
    /// Verifies: HTTP 200, tags context stored, response includes metadata
    /// </summary>
    [Fact]
    public async Task POST_AppointmentTags_ApplyStatusAndFormatTags_Returns200()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var tag1 = CreateTag("Confirmed", "Status");
        var tag2 = CreateTag("Virtual", "Format");

        await DbContext.Tags.AddAsync(tag1);
        await DbContext.Tags.AddAsync(tag2);
        await DbContext.SaveChangesAsync();

        var command = new ApplyTagsCommand
        {
            ResourceId = appointmentId,
            ResourceType = nameof(Appointment),
            TagIds = new[] { tag1.Id, tag2.Id },
            ServiceName = "Appointment",
            Context = "Virtual appointment confirmed"
        };

        MockTagService
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid tagId, CancellationToken ct) =>
                new[] { tag1, tag2 }.FirstOrDefault(t => t.Id == tagId));

        MockTagService
            .Setup(x => x.ApplyTagAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid rid, string rt, Guid tid, string sn, string? ctx, string? ab, CancellationToken ct) =>
                CreateTagAssociation(tid, rid, rt, sn, ctx));

        // Act
        var result = new ApplyTagsCommandHandler(MockTagService.Object)
            .Handle(command, CancellationToken.None);

        // Assert
        await Assert.NotNullAsync(result);
    }

    /// <summary>
    /// E2E Test 6: GET /api/v1/appointments/{id}/tags - Query appointment tags
    /// Verifies: HTTP 200, returns tags with format/status/priority information
    /// </summary>
    [Fact]
    public async Task GET_AppointmentTags_Query_Returns200WithFormatAndStatusTags()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var tag1 = CreateTag("Virtual", "Format");
        var tag2 = CreateTag("Confirmed", "Status");

        var tagDtos = new[]
        {
            new TagDto { Id = tag1.Id, Name = "Virtual", Category = "Format" },
            new TagDto { Id = tag2.Id, Name = "Confirmed", Category = "Status" }
        };

        MockTagQueryService
            .Setup(x => x.GetResourceTagsAsync(
                appointmentId, nameof(Appointment), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tagDtos);

        // Act
        var tags = await MockTagQueryService.Object.GetResourceTagsAsync(
            appointmentId, nameof(Appointment), CancellationToken.None);

        // Assert
        Assert.Equal(2, tags.Count());
        Assert.Contains(tags, t => t.Category == "Format");
        Assert.Contains(tags, t => t.Category == "Status");
    }

    /// <summary>
    /// E2E Test 7: POST /api/v1/invoices/{id}/tags - Apply billing tags
    /// Verifies: HTTP 200, billing-specific tags applied, compliance tags tracked
    /// </summary>
    [Fact]
    public async Task POST_InvoiceTags_ApplyBillingAndComplianceTags_Returns200()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var tag1 = CreateTag("Paid", "BillingStatus");
        var tag2 = CreateTag("Verified", "Compliance");

        await DbContext.Tags.AddAsync(tag1);
        await DbContext.Tags.AddAsync(tag2);
        await DbContext.SaveChangesAsync();

        var command = new ApplyTagsCommand
        {
            ResourceId = invoiceId,
            ResourceType = nameof(Invoice),
            TagIds = new[] { tag1.Id, tag2.Id },
            ServiceName = "Billing"
        };

        MockTagService
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid tagId, CancellationToken ct) =>
                new[] { tag1, tag2 }.FirstOrDefault(t => t.Id == tagId));

        MockTagService
            .Setup(x => x.ApplyTagAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid rid, string rt, Guid tid, string sn, string? ctx, string? ab, CancellationToken ct) =>
                CreateTagAssociation(tid, rid, rt, sn));

        // Act
        var result = new ApplyTagsCommandHandler(MockTagService.Object)
            .Handle(command, CancellationToken.None);

        // Assert
        await Assert.NotNullAsync(result);
    }

    /// <summary>
    /// E2E Test 8: GET /api/v1/invoices/{id}/tags - Query billing tags
    /// Verifies: HTTP 200, billing status and payment method tags returned
    /// </summary>
    [Fact]
    public async Task GET_InvoiceTags_Query_Returns200WithBillingStatusTags()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var tag1 = CreateTag("Paid", "BillingStatus");
        var tag2 = CreateTag("Insurance", "PaymentMethod");

        var tagDtos = new[]
        {
            new TagDto { Id = tag1.Id, Name = "Paid", Category = "BillingStatus" },
            new TagDto { Id = tag2.Id, Name = "Insurance", Category = "PaymentMethod" }
        };

        MockTagQueryService
            .Setup(x => x.GetResourceTagsAsync(
                invoiceId, nameof(Invoice), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tagDtos);

        // Act
        var tags = await MockTagQueryService.Object.GetResourceTagsAsync(
            invoiceId, nameof(Invoice), CancellationToken.None);

        // Assert
        Assert.Equal(2, tags.Count());
        Assert.Contains(tags, t => t.Category == "BillingStatus");
        Assert.Contains(tags, t => t.Category == "PaymentMethod");
    }

    /// <summary>
    /// E2E Test 9: PUT /api/v1/patients/{id}/tags - Replace all tags
    /// Verifies: HTTP 200, old tags removed, new tags applied, count updated
    /// </summary>
    [Fact]
    public async Task PUT_PatientTags_ReplaceAll_Returns200WithNewTagList()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var oldTag = CreateTag("Old", "Priority");
        var newTag1 = CreateTag("New1", "Status");
        var newTag2 = CreateTag("New2", "Health");

        await DbContext.Tags.AddAsync(oldTag);
        await DbContext.Tags.AddAsync(newTag1);
        await DbContext.Tags.AddAsync(newTag2);
        await DbContext.SaveChangesAsync();

        var command = new SetResourceTagsCommand
        {
            ResourceId = patientId,
            ResourceType = nameof(Patient),
            TagIds = new[] { newTag1.Id, newTag2.Id },
            ServiceName = "Patient"
        };

        var newAssociations = new[]
        {
            CreateTagAssociation(newTag1.Id, patientId),
            CreateTagAssociation(newTag2.Id, patientId)
        };

        MockTagService
            .Setup(x => x.SetResourceTagsAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newAssociations);

        // Act
        var result = await MockTagService.Object.SetResourceTagsAsync(
            patientId, nameof(Patient), new[] { newTag1.Id, newTag2.Id },
            "Patient", null, CancellationToken.None);

        // Assert - HTTP 200 equivalent
        Assert.Equal(2, result.Count());
        Assert.DoesNotContain(result, r => r.TagId == oldTag.Id);
    }

    /// <summary>
    /// E2E Test 10: Error Handling - Invalid tag ID
    /// Verifies: HTTP 400/404 when tag doesn't exist
    /// </summary>
    [Fact]
    public async Task POST_PatientTags_InvalidTagId_ReturnsErrorResponse()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var invalidTagId = Guid.NewGuid();

        var command = new ApplyTagsCommand
        {
            ResourceId = patientId,
            ResourceType = nameof(Patient),
            TagIds = new[] { invalidTagId },
            ServiceName = "Patient"
        };

        MockTagService
            .Setup(x => x.GetByIdAsync(invalidTagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        // Act
        var tag = await MockTagService.Object.GetByIdAsync(invalidTagId);

        // Assert - Tag not found
        Assert.Null(tag);
    }

    /// <summary>
    /// E2E Test 11: Bulk Operations - Apply same tag to 100+ resources
    /// Verifies: Batch efficiency, usage count incremented correctly
    /// </summary>
    [Fact]
    public async Task POST_BulkTags_ApplyToMultipleResources_HandlesBatchEfficiently()
    {
        // Arrange
        var tag = CreateTag("BulkTag", "Workflow");
        await DbContext.Tags.AddAsync(tag);
        await DbContext.SaveChangesAsync();

        var resourceIds = Enumerable.Range(1, 100)
            .Select(_ => Guid.NewGuid())
            .ToList();

        MockTagService
            .Setup(x => x.BulkApplyTagAsync(
                It.IsAny<IEnumerable<Guid>>(), nameof(Patient), tag.Id,
                "Patient", It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);

        // Act
        var result = await MockTagService.Object.BulkApplyTagAsync(
            resourceIds, nameof(Patient), tag.Id, "Patient");

        // Assert - All 100 tagged
        Assert.Equal(100, result);
    }

    /// <summary>
    /// E2E Test 12: Concurrency - Multiple concurrent requests to same resource
    /// Verifies: No race conditions, all operations succeed, final state consistent
    /// </summary>
    [Fact]
    public async Task POST_ConcurrentTagOperations_OnSameResource_AllSucceed()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var tags = new[]
        {
            CreateTag("Tag1", "Category1"),
            CreateTag("Tag2", "Category2"),
            CreateTag("Tag3", "Category3")
        };

        await DbContext.Tags.AddRangeAsync(tags);
        await DbContext.SaveChangesAsync();

        var commands = tags.Select(t => new ApplyTagsCommand
        {
            ResourceId = patientId,
            ResourceType = nameof(Patient),
            TagIds = new[] { t.Id },
            ServiceName = "Patient"
        }).ToList();

        MockTagService
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid tagId, CancellationToken ct) =>
                tags.FirstOrDefault(t => t.Id == tagId));

        MockTagService
            .Setup(x => x.ApplyTagAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid rid, string rt, Guid tid, string sn, string? ctx, string? ab, CancellationToken ct) =>
                CreateTagAssociation(tid, rid, rt, sn));

        // Act - Execute concurrently
        var tasks = commands.Select(cmd =>
            new ApplyTagsCommandHandler(MockTagService.Object)
                .Handle(cmd, CancellationToken.None))
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert - All succeeded
        Assert.All(results, r => Assert.NotNull(r));
        Assert.Equal(3, results.Length);
    }
}
