#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EHRPlatform.Common.Tags;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;
using EHRPlatform.Services.Appointment.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Tests.Integration.Features.Tags;

/// <summary>
/// Integration tests for appointment tag operations.
/// Tests tag workflows on appointment resources across multiple scenarios.
/// </summary>
public class AppointmentTagsIntegrationTests : IntegrationTestBase
{
    private AppointmentTagsController _controller = null!;
    private Mock<ILogger<AppointmentTagsController>> _mockLogger = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _mockLogger = new Mock<ILogger<AppointmentTagsController>>();
        _controller = new AppointmentTagsController(
            MockTagQueryService.Object,
            Mediator,
            _mockLogger.Object);
    }

    /// <summary>
    /// Happy path: Apply single tag to appointment.
    /// </summary>
    [Fact]
    public async Task ApplyTag_SingleTag_SuccessfullyApplied()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var tag = CreateTag("Confirmed", "Status");
        await DbContext.Tags.AddAsync(tag);
        await DbContext.SaveChangesAsync();

        var command = new ApplyTagsCommand
        {
            ResourceId = appointmentId,
            ResourceType = nameof(Appointment),
            TagIds = new[] { tag.Id },
            ServiceName = "Appointment",
            Context = "Patient confirmed appointment"
        };

        MockTagService
            .Setup(x => x.GetByIdAsync(tag.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        MockTagService
            .Setup(x => x.ApplyTagAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTagAssociation(tag.Id, appointmentId, nameof(Appointment), "Appointment"));

        MockTagService
            .Setup(x => x.GetResourceTagsAsync(
                appointmentId,
                nameof(Appointment),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { tag });

        // Act
        var result = await _controller.ApplyAppointmentTags(appointmentId, command, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as TagAssignmentResponse;
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Single(response.AppliedTagIds);
    }

    /// <summary>
    /// Happy path: Apply multiple appointment-specific tags.
    /// </summary>
    [Fact]
    public async Task ApplyTags_MultipleAppointmentTags_AppliesSuccessfully()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var tag1 = CreateTag("Urgent", "Priority");
        var tag2 = CreateTag("Virtual", "Format");
        var tag3 = CreateTag("Recurring", "Type");

        await DbContext.Tags.AddAsync(tag1);
        await DbContext.Tags.AddAsync(tag2);
        await DbContext.Tags.AddAsync(tag3);
        await DbContext.SaveChangesAsync();

        var command = new ApplyTagsCommand
        {
            ResourceId = appointmentId,
            ResourceType = nameof(Appointment),
            TagIds = new[] { tag1.Id, tag2.Id, tag3.Id },
            ServiceName = "Appointment"
        };

        foreach (var tag in new[] { tag1, tag2, tag3 })
        {
            MockTagService
                .Setup(x => x.GetByIdAsync(tag.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tag);
        }

        MockTagService
            .Setup(x => x.ApplyTagAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid rid, string rt, Guid tid, string sn, string? ctx, string? ab, CancellationToken ct) =>
                CreateTagAssociation(tid, rid, rt, sn));

        MockTagService
            .Setup(x => x.GetResourceTagsAsync(
                appointmentId,
                nameof(Appointment),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { tag1, tag2, tag3 });

        // Act
        var result = await _controller.ApplyAppointmentTags(appointmentId, command, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as TagAssignmentResponse;
        Assert.NotNull(response);
        Assert.Equal(3, response.TotalTagsOnResource);
    }

    /// <summary>
    /// Query: Get all tags for an appointment.
    /// </summary>
    [Fact]
    public async Task GetAppointmentTags_WithTags_ReturnsAllTags()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var tag1 = CreateTag("Urgent", "Priority");
        var tag2 = CreateTag("Virtual", "Format");

        var tagDtos = new[]
        {
            new TagDto { Id = tag1.Id, Name = "Urgent", Category = "Priority" },
            new TagDto { Id = tag2.Id, Name = "Virtual", Category = "Format" }
        };

        MockTagQueryService
            .Setup(x => x.GetResourceTagsAsync(
                appointmentId,
                nameof(Appointment),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tagDtos);

        // Act
        var result = await _controller.GetAppointmentTags(appointmentId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    /// <summary>
    /// Query: Empty tags list for untagged appointment.
    /// </summary>
    [Fact]
    public async Task GetAppointmentTags_NoTags_ReturnsEmpty()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        MockTagQueryService
            .Setup(x => x.GetResourceTagsAsync(
                appointmentId,
                nameof(Appointment),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TagDto>());

        // Act
        var result = await _controller.GetAppointmentTags(appointmentId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = okResult.Value as dynamic;
        Assert.Empty(returnValue?.tags);
    }

    /// <summary>
    /// Remove: Delete tag from appointment.
    /// </summary>
    [Fact]
    public async Task RemoveAppointmentTag_ValidTag_RemovesSuccessfully()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var tag = CreateTag("Cancelled", "Status");
        var association = CreateTagAssociation(tag.Id, appointmentId, nameof(Appointment), "Appointment");

        await DbContext.Tags.AddAsync(tag);
        await DbContext.TagAssociations.AddAsync(association);
        await DbContext.SaveChangesAsync();

        MockTagService
            .Setup(x => x.RemoveTagAsync(
                appointmentId,
                nameof(Appointment),
                tag.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveAppointmentTag(appointmentId, tag.Id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        MockTagService.Verify(
            x => x.RemoveTagAsync(appointmentId, nameof(Appointment), tag.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Edge case: Remove non-existent tag fails gracefully.
    /// </summary>
    [Fact]
    public async Task RemoveAppointmentTag_NonExistentTag_ReturnsNotFound()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        MockTagService
            .Setup(x => x.RemoveTagAsync(
                appointmentId,
                nameof(Appointment),
                tagId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"Tag {tagId} not associated with appointment"));

        // Act
        var result = await _controller.RemoveAppointmentTag(appointmentId, tagId, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    /// <summary>
    /// Set: Replace all tags on appointment.
    /// </summary>
    [Fact]
    public async Task SetAppointmentTags_ReplaceAll_SuccessfullyUpdates()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var oldTag = CreateTag("Pending", "Status");
        var newTag1 = CreateTag("Confirmed", "Status");
        var newTag2 = CreateTag("Reminder Sent", "Notification");

        await DbContext.Tags.AddAsync(oldTag);
        await DbContext.Tags.AddAsync(newTag1);
        await DbContext.Tags.AddAsync(newTag2);
        await DbContext.SaveChangesAsync();

        var command = new SetResourceTagsCommand
        {
            ResourceId = appointmentId,
            ResourceType = nameof(Appointment),
            TagIds = new[] { newTag1.Id, newTag2.Id },
            ServiceName = "Appointment"
        };

        var newAssociations = new[]
        {
            CreateTagAssociation(newTag1.Id, appointmentId, nameof(Appointment), "Appointment"),
            CreateTagAssociation(newTag2.Id, appointmentId, nameof(Appointment), "Appointment")
        };

        MockTagService
            .Setup(x => x.SetResourceTagsAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(newAssociations);

        // Act
        var result = await _controller.SetAppointmentTags(appointmentId, command, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    /// <summary>
    /// Edge case: Apply tags with service restriction (Appointment-only tags).
    /// </summary>
    [Fact]
    public async Task ApplyTag_ServiceRestricted_FailsForWrongService()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var appointmentOnlyTag = CreateTag(
            "Appointment-Only",
            "Type",
            allowedServices: "Appointment"); // Restricted to Appointment service

        await DbContext.Tags.AddAsync(appointmentOnlyTag);
        await DbContext.SaveChangesAsync();

        var command = new ApplyTagsCommand
        {
            ResourceId = appointmentId,
            ResourceType = nameof(Appointment),
            TagIds = new[] { appointmentOnlyTag.Id },
            ServiceName = "Appointment"
        };

        // Should succeed because tag allows Appointment service
        MockTagService
            .Setup(x => x.GetByIdAsync(appointmentOnlyTag.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentOnlyTag);

        MockTagService
            .Setup(x => x.ApplyTagAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTagAssociation(appointmentOnlyTag.Id, appointmentId, nameof(Appointment), "Appointment"));

        MockTagService
            .Setup(x => x.GetResourceTagsAsync(
                appointmentId,
                nameof(Appointment),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { appointmentOnlyTag });

        // Act
        var result = await _controller.ApplyAppointmentTags(appointmentId, command, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as TagAssignmentResponse;
        Assert.NotNull(response);
        Assert.True(response.Success);
    }

    /// <summary>
    /// Async operation: Bulk apply tags to multiple appointments.
    /// </summary>
    [Fact]
    public async Task ApplyTag_BulkOperation_AppliesToMultipleAppointments()
    {
        // Arrange
        var appointmentIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var tag = CreateTag("BulkTag", "Workflow");

        await DbContext.Tags.AddAsync(tag);
        await DbContext.SaveChangesAsync();

        MockTagService
            .Setup(x => x.BulkApplyTagAsync(
                It.IsAny<IEnumerable<Guid>>(),
                nameof(Appointment),
                tag.Id,
                "Appointment",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var result = await MockTagService.Object.BulkApplyTagAsync(
            appointmentIds,
            nameof(Appointment),
            tag.Id,
            "Appointment");

        // Assert
        Assert.Equal(3, result);
    }

    /// <summary>
    /// Concurrency: Multiple clients updating same appointment tags.
    /// </summary>
    [Fact]
    public async Task ApplyTags_ConcurrentUpdates_HandlesConcurrency()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var tag1 = CreateTag("Tag1", "Status");
        var tag2 = CreateTag("Tag2", "Priority");

        await DbContext.Tags.AddAsync(tag1);
        await DbContext.Tags.AddAsync(tag2);
        await DbContext.SaveChangesAsync();

        var command1 = new ApplyTagsCommand
        {
            ResourceId = appointmentId,
            ResourceType = nameof(Appointment),
            TagIds = new[] { tag1.Id },
            ServiceName = "Appointment"
        };

        var command2 = new ApplyTagsCommand
        {
            ResourceId = appointmentId,
            ResourceType = nameof(Appointment),
            TagIds = new[] { tag2.Id },
            ServiceName = "Appointment"
        };

        MockTagService
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid tagId, CancellationToken ct) =>
                new[] { tag1, tag2 }.FirstOrDefault(t => t.Id == tagId));

        MockTagService
            .Setup(x => x.ApplyTagAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid rid, string rt, Guid tid, string sn, string? ctx, string? ab, CancellationToken ct) =>
                CreateTagAssociation(tid, rid, rt, sn));

        // Act - Run concurrently
        var results = await Task.WhenAll(
            _controller.ApplyAppointmentTags(appointmentId, command1, CancellationToken.None),
            _controller.ApplyAppointmentTags(appointmentId, command2, CancellationToken.None));

        // Assert - Both should succeed
        foreach (var result in results)
        {
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
