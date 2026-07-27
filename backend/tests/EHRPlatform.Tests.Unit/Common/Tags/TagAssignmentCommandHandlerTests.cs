using Xunit;
using Moq;
using EHRPlatform.Common.Tags;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Tests.Unit.Common.Tags;

/// <summary>
/// Unit tests for tag assignment commands.
/// Tests ApplyTagsCommand, RemoveTagCommand, and SetResourceTagsCommand handlers.
/// </summary>
public class TagAssignmentCommandHandlerTests
{
    private readonly Mock<ITagService> _mockTagService;
    private readonly Mock<ILogger<ApplyTagsCommandHandler>> _mockLogger;
    private readonly ApplyTagsCommandHandler _handler;

    public TagAssignmentCommandHandlerTests()
    {
        _mockTagService = new Mock<ITagService>();
        _mockLogger = new Mock<ILogger<ApplyTagsCommandHandler>>();
        _handler = new ApplyTagsCommandHandler(_mockTagService.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Test applying tags successfully.
    /// </summary>
    [Fact]
    public async Task ApplyTagsCommandHandler_WithValidCommand_AppliesTagsSuccessfully()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var tagIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var command = new ApplyTagsCommand
        {
            ResourceId = resourceId,
            ResourceType = "Patient",
            TagIds = tagIds,
            ServiceName = "Patient",
            Context = "Test context",
            AppliedBy = "user123"
        };

        var tag = new Tag
        {
            Id = tagIds[0],
            Name = "Test Tag",
            Category = "Test",
            ServiceName = "Patient"
        };

        _mockTagService
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        _mockTagService
            .Setup(x => x.ApplyTagAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TagAssociation
            {
                Id = Guid.NewGuid(),
                ResourceId = resourceId,
                ResourceType = "Patient",
                TagId = tagIds[0]
            });

        _mockTagService
            .Setup(x => x.GetResourceTagsAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TagAssociation>
            {
                new TagAssociation { TagId = tagIds[0] },
                new TagAssociation { TagId = tagIds[1] }
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Contains("Successfully applied", result.Message);
        Assert.Equal(resourceId, result.ResourceId);
    }

    /// <summary>
    /// Test applying tags with non-existent tag fails.
    /// </summary>
    [Fact]
    public async Task ApplyTagsCommandHandler_WithNonExistentTag_ReturnsPartialSuccess()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var validTagId = Guid.NewGuid();
        var invalidTagId = Guid.NewGuid();
        
        var command = new ApplyTagsCommand
        {
            ResourceId = resourceId,
            ResourceType = "Patient",
            TagIds = new[] { validTagId, invalidTagId },
            ServiceName = "Patient"
        };

        var validTag = new Tag
        {
            Id = validTagId,
            Name = "Valid Tag",
            Category = "Test",
            ServiceName = "Patient"
        };

        _mockTagService
            .Setup(x => x.GetByIdAsync(validTagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validTag);

        _mockTagService
            .Setup(x => x.GetByIdAsync(invalidTagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        _mockTagService
            .Setup(x => x.ApplyTagAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TagAssociation { TagId = validTagId });

        _mockTagService
            .Setup(x => x.GetResourceTagsAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TagAssociation>
            {
                new TagAssociation { TagId = validTagId }
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success); // Partial failure due to invalid tag
        Assert.Contains("not found", result.Message);
        Assert.Single(result.AppliedTagIds);
    }

    /// <summary>
    /// Test applying tags with unauthorized service fails.
    /// </summary>
    [Fact]
    public async Task ApplyTagsCommandHandler_WithUnauthorizedService_ReturnsError()
    {
        // Arrange
        var command = new ApplyTagsCommand
        {
            ResourceId = Guid.NewGuid(),
            ResourceType = "Patient",
            TagIds = new[] { Guid.NewGuid() },
            ServiceName = "Patient"
        };

        var tag = new Tag
        {
            Id = command.TagIds.First(),
            Name = "Restricted Tag",
            Category = "Billing",
            ServiceName = "Billing" // Different service
        };

        _mockTagService
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("cannot be used by service", result.Message);
    }
}

/// <summary>
/// Tests for RemoveTagCommand handler.
/// </summary>
public class RemoveTagCommandHandlerTests
{
    private readonly Mock<ITagService> _mockTagService;
    private readonly Mock<ILogger<RemoveTagCommandHandler>> _mockLogger;
    private readonly RemoveTagCommandHandler _handler;

    public RemoveTagCommandHandlerTests()
    {
        _mockTagService = new Mock<ITagService>();
        _mockLogger = new Mock<ILogger<RemoveTagCommandHandler>>();
        _handler = new RemoveTagCommandHandler(_mockTagService.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Test removing tag successfully.
    /// </summary>
    [Fact]
    public async Task RemoveTagCommandHandler_WithExistingTag_RemovesSuccessfully()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var command = new RemoveTagCommand
        {
            ResourceId = resourceId,
            ResourceType = "Patient",
            TagId = tagId,
            ServiceName = "Patient"
        };

        _mockTagService
            .Setup(x => x.RemoveTagAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockTagService
            .Setup(x => x.GetResourceTagsAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TagAssociation>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Contains("removed successfully", result.Message);
    }

    /// <summary>
    /// Test removing non-existent tag fails.
    /// </summary>
    [Fact]
    public async Task RemoveTagCommandHandler_WithNonExistentTag_ReturnsFalse()
    {
        // Arrange
        var command = new RemoveTagCommand
        {
            ResourceId = Guid.NewGuid(),
            ResourceType = "Patient",
            TagId = Guid.NewGuid(),
            ServiceName = "Patient"
        };

        _mockTagService
            .Setup(x => x.RemoveTagAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockTagService
            .Setup(x => x.GetResourceTagsAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TagAssociation>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not found on resource", result.Message);
    }
}
