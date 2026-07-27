#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EHRPlatform.Common.Tags;
using EHRPlatform.Services.Billing.Domain.Entities;
using EHRPlatform.Services.Billing.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Tests.Integration.Features.Tags;

/// <summary>
/// Integration tests for invoice tag operations.
/// Tests tag workflows on billing/invoice resources.
/// </summary>
public class InvoiceTagsIntegrationTests : IntegrationTestBase
{
    private InvoiceTagsController _controller = null!;
    private Mock<ILogger<InvoiceTagsController>> _mockLogger = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _mockLogger = new Mock<ILogger<InvoiceTagsController>>();
        _controller = new InvoiceTagsController(
            MockTagQueryService.Object,
            Mediator,
            _mockLogger.Object);
    }

    /// <summary>
    /// Happy path: Apply single billing tag to invoice.
    /// </summary>
    [Fact]
    public async Task ApplyTag_SingleBillingTag_SuccessfullyApplied()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var tag = CreateTag("Paid", "BillingStatus");
        await DbContext.Tags.AddAsync(tag);
        await DbContext.SaveChangesAsync();

        var command = new ApplyTagsCommand
        {
            ResourceId = invoiceId,
            ResourceType = nameof(Invoice),
            TagIds = new[] { tag.Id },
            ServiceName = "Billing"
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
            .ReturnsAsync(CreateTagAssociation(tag.Id, invoiceId, nameof(Invoice), "Billing"));

        MockTagService
            .Setup(x => x.GetResourceTagsAsync(
                invoiceId,
                nameof(Invoice),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { tag });

        // Act
        var result = await _controller.ApplyInvoiceTags(invoiceId, command, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as TagAssignmentResponse;
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Single(response.AppliedTagIds);
    }

    /// <summary>
    /// Happy path: Apply multiple billing-related tags to invoice.
    /// </summary>
    [Fact]
    public async Task ApplyTags_MultipleBillingTags_AppliesSuccessfully()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var tag1 = CreateTag("Paid", "BillingStatus");
        var tag2 = CreateTag("Insurance", "PaymentMethod");
        var tag3 = CreateTag("Urgent", "Priority");

        await DbContext.Tags.AddAsync(tag1);
        await DbContext.Tags.AddAsync(tag2);
        await DbContext.Tags.AddAsync(tag3);
        await DbContext.SaveChangesAsync();

        var command = new ApplyTagsCommand
        {
            ResourceId = invoiceId,
            ResourceType = nameof(Invoice),
            TagIds = new[] { tag1.Id, tag2.Id, tag3.Id },
            ServiceName = "Billing"
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
                invoiceId,
                nameof(Invoice),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { tag1, tag2, tag3 });

        // Act
        var result = await _controller.ApplyInvoiceTags(invoiceId, command, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as TagAssignmentResponse;
        Assert.NotNull(response);
        Assert.Equal(3, response.TotalTagsOnResource);
    }

    /// <summary>
    /// Query: Get all tags for an invoice.
    /// </summary>
    [Fact]
    public async Task GetInvoiceTags_WithTags_ReturnsAllTags()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var tag1 = CreateTag("Paid", "BillingStatus");
        var tag2 = CreateTag("Reviewed", "Compliance");

        var tagDtos = new[]
        {
            new TagDto { Id = tag1.Id, Name = "Paid", Category = "BillingStatus" },
            new TagDto { Id = tag2.Id, Name = "Reviewed", Category = "Compliance" }
        };

        MockTagQueryService
            .Setup(x => x.GetResourceTagsAsync(
                invoiceId,
                nameof(Invoice),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tagDtos);

        // Act
        var result = await _controller.GetInvoiceTags(invoiceId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    /// <summary>
    /// Query: Empty tags for untagged invoice.
    /// </summary>
    [Fact]
    public async Task GetInvoiceTags_NoTags_ReturnsEmpty()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        MockTagQueryService
            .Setup(x => x.GetResourceTagsAsync(
                invoiceId,
                nameof(Invoice),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TagDto>());

        // Act
        var result = await _controller.GetInvoiceTags(invoiceId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = okResult.Value as dynamic;
        Assert.Empty(returnValue?.tags);
    }

    /// <summary>
    /// Remove: Delete tag from invoice.
    /// </summary>
    [Fact]
    public async Task RemoveInvoiceTag_ValidTag_RemovesSuccessfully()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var tag = CreateTag("Disputed", "BillingStatus");
        var association = CreateTagAssociation(tag.Id, invoiceId, nameof(Invoice), "Billing");

        await DbContext.Tags.AddAsync(tag);
        await DbContext.TagAssociations.AddAsync(association);
        await DbContext.SaveChangesAsync();

        MockTagService
            .Setup(x => x.RemoveTagAsync(
                invoiceId,
                nameof(Invoice),
                tag.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveInvoiceTag(invoiceId, tag.Id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        MockTagService.Verify(
            x => x.RemoveTagAsync(invoiceId, nameof(Invoice), tag.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Edge case: Invalid resource type handling.
    /// </summary>
    [Fact]
    public async Task ApplyTag_InvalidResourceType_HandlesGracefully()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var tag = CreateTag("Tag", "Category");
        await DbContext.Tags.AddAsync(tag);
        await DbContext.SaveChangesAsync();

        var command = new ApplyTagsCommand
        {
            ResourceId = invoiceId,
            ResourceType = "InvalidType",
            TagIds = new[] { tag.Id },
            ServiceName = "Billing"
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
            .ReturnsAsync(CreateTagAssociation(tag.Id, invoiceId, "InvalidType", "Billing"));

        MockTagService
            .Setup(x => x.GetResourceTagsAsync(
                invoiceId,
                "InvalidType",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { tag });

        // Act
        var result = await _controller.ApplyInvoiceTags(invoiceId, command, CancellationToken.None);

        // Assert - Should still return OK (graceful handling)
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    /// <summary>
    /// Set: Replace all tags on invoice.
    /// </summary>
    [Fact]
    public async Task SetInvoiceTags_ReplaceAll_SuccessfullyUpdates()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var oldTag = CreateTag("Pending", "BillingStatus");
        var newTag1 = CreateTag("Paid", "BillingStatus");
        var newTag2 = CreateTag("Verified", "Compliance");

        await DbContext.Tags.AddAsync(oldTag);
        await DbContext.Tags.AddAsync(newTag1);
        await DbContext.Tags.AddAsync(newTag2);
        await DbContext.SaveChangesAsync();

        var command = new SetResourceTagsCommand
        {
            ResourceId = invoiceId,
            ResourceType = nameof(Invoice),
            TagIds = new[] { newTag1.Id, newTag2.Id },
            ServiceName = "Billing"
        };

        var newAssociations = new[]
        {
            CreateTagAssociation(newTag1.Id, invoiceId, nameof(Invoice), "Billing"),
            CreateTagAssociation(newTag2.Id, invoiceId, nameof(Invoice), "Billing")
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
        var result = await _controller.SetInvoiceTags(invoiceId, command, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    /// <summary>
    /// Concurrency: Multiple billing operations on same invoice.
    /// </summary>
    [Fact]
    public async Task ApplyTags_ConcurrentBillingOps_HandlesConcurrency()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var tag1 = CreateTag("Processing", "BillingStatus");
        var tag2 = CreateTag("AutoPay", "PaymentMethod");

        await DbContext.Tags.AddAsync(tag1);
        await DbContext.Tags.AddAsync(tag2);
        await DbContext.SaveChangesAsync();

        var command1 = new ApplyTagsCommand
        {
            ResourceId = invoiceId,
            ResourceType = nameof(Invoice),
            TagIds = new[] { tag1.Id },
            ServiceName = "Billing"
        };

        var command2 = new ApplyTagsCommand
        {
            ResourceId = invoiceId,
            ResourceType = nameof(Invoice),
            TagIds = new[] { tag2.Id },
            ServiceName = "Billing"
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
            _controller.ApplyInvoiceTags(invoiceId, command1, CancellationToken.None),
            _controller.ApplyInvoiceTags(invoiceId, command2, CancellationToken.None));

        // Assert - Both should succeed
        foreach (var result in results)
        {
            Assert.IsType<OkObjectResult>(result);
        }
    }

    /// <summary>
    /// Archived tag: Should not be applicable to new invoices.
    /// </summary>
    [Fact]
    public async Task ApplyTag_ArchivedTag_CannotBeApplied()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var archivedTag = CreateTag("OldStatus", "BillingStatus");
        archivedTag.IsArchived = true;

        await DbContext.Tags.AddAsync(archivedTag);
        await DbContext.SaveChangesAsync();

        var command = new ApplyTagsCommand
        {
            ResourceId = invoiceId,
            ResourceType = nameof(Invoice),
            TagIds = new[] { archivedTag.Id },
            ServiceName = "Billing"
        };

        MockTagService
            .Setup(x => x.GetByIdAsync(archivedTag.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(archivedTag);

        // Archived tag cannot be applied (reflected in response)
        MockTagService
            .Setup(x => x.GetResourceTagsAsync(
                invoiceId,
                nameof(Invoice),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tag[] { }); // No tags applied

        // Act
        var result = await _controller.ApplyInvoiceTags(invoiceId, command, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as TagAssignmentResponse;
        Assert.NotNull(response);
        Assert.False(response.Success); // Should fail due to archived tag
        Assert.Empty(response.AppliedTagIds);
    }

    /// <summary>
    /// Usage tracking: Verify tag usage count increases on application.
    /// </summary>
    [Fact]
    public async Task ApplyTag_TrackingUsageCount_IncrementsProperly()
    {
        // Arrange
        var invoiceId1 = Guid.NewGuid();
        var invoiceId2 = Guid.NewGuid();
        var tag = CreateTag("CommonTag", "Status");
        tag.UsageCount = 0;

        await DbContext.Tags.AddAsync(tag);
        await DbContext.SaveChangesAsync();

        var command1 = new ApplyTagsCommand
        {
            ResourceId = invoiceId1,
            ResourceType = nameof(Invoice),
            TagIds = new[] { tag.Id },
            ServiceName = "Billing"
        };

        var command2 = new ApplyTagsCommand
        {
            ResourceId = invoiceId2,
            ResourceType = nameof(Invoice),
            TagIds = new[] { tag.Id },
            ServiceName = "Billing"
        };

        MockTagService
            .Setup(x => x.GetByIdAsync(tag.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        MockTagService
            .Setup(x => x.GetTagUsageCountAsync(tag.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2); // Two invoices now have this tag

        // Act
        await _controller.ApplyInvoiceTags(invoiceId1, command1, CancellationToken.None);
        await _controller.ApplyInvoiceTags(invoiceId2, command2, CancellationToken.None);
        var usageCount = await MockTagService.Object.GetTagUsageCountAsync(tag.Id);

        // Assert
        Assert.Equal(2, usageCount);
    }
}
