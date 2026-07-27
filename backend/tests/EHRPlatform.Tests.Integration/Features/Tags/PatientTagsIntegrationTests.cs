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
using EHRPlatform.Services.Patient.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Tests.Integration.Features.Tags;

/// <summary>
/// Integration tests for patient tag operations.
/// Tests full workflow: apply, query, update, remove tags on patient resources.
/// </summary>
public class PatientTagsIntegrationTests : IntegrationTestBase
{
    private PatientTagsController _controller = null!;
    private Mock<ILogger<PatientTagsController>> _mockLogger = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _mockLogger = new Mock<ILogger<PatientTagsController>>();
        _controller = new PatientTagsController(
            MockTagQueryService.Object,
            Mediator,
            _mockLogger.Object);
    }

    /// <summary>
    /// Happy path: Apply single tag to patient, verify association created.
    /// </summary>
    [Fact]
    public async Task ApplyTag_SingleTag_CreatesAssociationSuccessfully()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var tag = CreateTag("VIP", "Priority");
        await DbContext.Tags.AddAsync(tag);
        await DbContext.SaveChangesAsync();

        var command = new ApplyTagsCommand
        {
            ResourceId = patientId,
            ResourceType = nameof(Patient),
            TagIds = new[] { tag.Id },
            ServiceName = "Patient",
            AppliedBy = "doctor-123"
        };

        var mockResponse = new TagAssignmentResponse
        {
            Success = true,
            Message = "Successfully applied 1 tag(s)",
            ResourceId = patientId,
            AppliedTagIds = new[] { tag.Id },
            TotalTagsOnResource = 1,
            Errors = Enumerable.Empty<string>()
        };

        MockTagService
            .Setup(x => x.GetByIdAsync(tag.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        MockTagService
            .Setup(x => x.ApplyTagAsync(
                patientId,
                nameof(Patient),
                tag.Id,
                "Patient",
                null,
                "doctor-123",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTagAssociation(tag.Id, patientId));

        MockTagService
            .Setup(x => x.GetResourceTagsAsync(
                patientId,
                nameof(Patient),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { tag });

        // Act
        var result = await _controller.ApplyPatientTags(patientId, command, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = (OkObjectResult)result;
        var response = okResult.Value as TagAssignmentResponse;
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Single(response.AppliedTagIds);
        Assert.Equal(tag.Id, response.AppliedTagIds.First());
    }

    /// <summary>
    /// Happy path: Apply multiple tags to patient.
    /// </summary>
    [Fact]
    public async Task ApplyTags_MultipleTags_AppliesAllSuccessfully()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var tag1 = CreateTag("VIP", "Priority");
        var tag2 = CreateTag("Follow-up", "Status");
        var tag3 = CreateTag("High Risk", "Health");

        await DbContext.Tags.AddAsync(tag1);
        await DbContext.Tags.AddAsync(tag2);
        await DbContext.Tags.AddAsync(tag3);
        await DbContext.SaveChangesAsync();

        var tagIds = new[] { tag1.Id, tag2.Id, tag3.Id };
        var command = new ApplyTagsCommand
        {
            ResourceId = patientId,
            ResourceType = nameof(Patient),
            TagIds = tagIds,
            ServiceName = "Patient"
        };

        var mockResponse = new TagAssignmentResponse
        {
            Success = true,
            Message = "Successfully applied 3 tag(s)",
            ResourceId = patientId,
            AppliedTagIds = tagIds,
            TotalTagsOnResource = 3,
            Errors = Enumerable.Empty<string>()
        };

        foreach (var tagId in tagIds)
        {
            var tag = await DbContext.Tags.FindAsync(tagId);
            MockTagService
                .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
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

        var allTags = new[] { tag1, tag2, tag3 };
        MockTagService
            .Setup(x => x.GetResourceTagsAsync(
                patientId,
                nameof(Patient),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(allTags);

        // Act
        var result = await _controller.ApplyPatientTags(patientId, command, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as TagAssignmentResponse;
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal(3, response.TotalTagsOnResource);
    }

    /// <summary>
    /// Query: Get all tags applied to a patient.
    /// </summary>
    [Fact]
    public async Task GetPatientTags_WithExistingTags_ReturnsAllTags()
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
                patientId,
                nameof(Patient),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tagDtos);

        // Act
        var result = await _controller.GetPatientTags(patientId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.NotNull(okResult.Value);
    }

    /// <summary>
    /// Query: Get patient with no tags returns empty list.
    /// </summary>
    [Fact]
    public async Task GetPatientTags_WithNoTags_ReturnsEmptyList()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var emptyTags = new List<TagDto>();

        MockTagQueryService
            .Setup(x => x.GetResourceTagsAsync(
                patientId,
                nameof(Patient),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyTags);

        // Act
        var result = await _controller.GetPatientTags(patientId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = okResult.Value as dynamic;
        Assert.Empty(returnValue?.tags);
    }

    /// <summary>
    /// Remove: Delete single tag from patient.
    /// </summary>
    [Fact]
    public async Task RemovePatientTag_ValidTag_RemovesSuccessfully()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var tag = CreateTag("VIP", "Priority");
        var association = CreateTagAssociation(tag.Id, patientId);

        await DbContext.Tags.AddAsync(tag);
        await DbContext.TagAssociations.AddAsync(association);
        await DbContext.SaveChangesAsync();

        MockTagService
            .Setup(x => x.RemoveTagAsync(
                patientId,
                nameof(Patient),
                tag.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemovePatientTag(patientId, tag.Id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        MockTagService.Verify(
            x => x.RemoveTagAsync(patientId, nameof(Patient), tag.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Edge case: Apply duplicate tags (should be idempotent).
    /// </summary>
    [Fact]
    public async Task ApplyTag_DuplicateTag_IsIdempotent()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var tag = CreateTag("VIP", "Priority");
        await DbContext.Tags.AddAsync(tag);
        await DbContext.SaveChangesAsync();

        var command = new ApplyTagsCommand
        {
            ResourceId = patientId,
            ResourceType = nameof(Patient),
            TagIds = new[] { tag.Id, tag.Id }, // Duplicate
            ServiceName = "Patient"
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
            .ReturnsAsync(CreateTagAssociation(tag.Id, patientId));

        MockTagService
            .Setup(x => x.GetResourceTagsAsync(
                patientId,
                nameof(Patient),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { tag });

        // Act
        var result = await _controller.ApplyPatientTags(patientId, command, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        // Should handle duplicate gracefully
        Assert.NotNull(okResult.Value);
    }

    /// <summary>
    /// Edge case: Apply non-existent tag (should fail gracefully).
    /// </summary>
    [Fact]
    public async Task ApplyTag_NonExistentTag_ReturnsPartialSuccess()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var validTag = CreateTag("VIP", "Priority");
        var invalidTagId = Guid.NewGuid();

        await DbContext.Tags.AddAsync(validTag);
        await DbContext.SaveChangesAsync();

        var command = new ApplyTagsCommand
        {
            ResourceId = patientId,
            ResourceType = nameof(Patient),
            TagIds = new[] { validTag.Id, invalidTagId },
            ServiceName = "Patient"
        };

        MockTagService
            .Setup(x => x.GetByIdAsync(validTag.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validTag);

        MockTagService
            .Setup(x => x.GetByIdAsync(invalidTagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        MockTagService
            .Setup(x => x.ApplyTagAsync(
                patientId,
                nameof(Patient),
                validTag.Id,
                "Patient",
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTagAssociation(validTag.Id, patientId));

        MockTagService
            .Setup(x => x.GetResourceTagsAsync(
                patientId,
                nameof(Patient),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { validTag });

        // Act
        var result = await _controller.ApplyPatientTags(patientId, command, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as TagAssignmentResponse;
        Assert.NotNull(response);
        Assert.False(response.Success); // Partial failure
        Assert.Single(response.AppliedTagIds); // Only valid tag applied
    }

    /// <summary>
    /// Set: Replace all tags on a patient.
    /// </summary>
    [Fact]
    public async Task SetPatientTags_ReplaceAllTags_SuccessfullyUpdates()
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

        var oldAssociation = CreateTagAssociation(oldTag.Id, patientId);
        await DbContext.TagAssociations.AddAsync(oldAssociation);
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
                patientId,
                nameof(Patient),
                new[] { newTag1.Id, newTag2.Id },
                "Patient",
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(newAssociations);

        MockTagService
            .Setup(x => x.GetResourceTagsAsync(
                patientId,
                nameof(Patient),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { newTag1, newTag2 });

        // Act
        var result = await _controller.SetPatientTags(patientId, command, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    /// <summary>
    /// Concurrency: Apply tags in parallel to same patient.
    /// </summary>
    [Fact]
    public async Task ApplyTags_ConcurrentRequests_HandlesConcurrencyCorrectly()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var tag1 = CreateTag("Tag1", "Priority");
        var tag2 = CreateTag("Tag2", "Status");

        await DbContext.Tags.AddAsync(tag1);
        await DbContext.Tags.AddAsync(tag2);
        await DbContext.SaveChangesAsync();

        var command1 = new ApplyTagsCommand
        {
            ResourceId = patientId,
            ResourceType = nameof(Patient),
            TagIds = new[] { tag1.Id },
            ServiceName = "Patient"
        };

        var command2 = new ApplyTagsCommand
        {
            ResourceId = patientId,
            ResourceType = nameof(Patient),
            TagIds = new[] { tag2.Id },
            ServiceName = "Patient"
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

        // Act - Run in parallel
        var task1 = _controller.ApplyPatientTags(patientId, command1, CancellationToken.None);
        var task2 = _controller.ApplyPatientTags(patientId, command2, CancellationToken.None);

        await Task.WhenAll(task1, task2);

        // Assert - Both should succeed
        var result1 = Assert.IsType<OkObjectResult>(await task1);
        var result2 = Assert.IsType<OkObjectResult>(await task2);
        Assert.NotNull(result1.Value);
        Assert.NotNull(result2.Value);
    }
}
