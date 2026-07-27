using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using EHRPlatform.Common.Tags;
using EHRPlatform.Services.Patient.Controllers;
using EHRPlatform.Services.Patient.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Tests.Unit.Services.Patient.Controllers;

/// <summary>
/// Unit tests for PatientTagsController.
/// Tests tag endpoints for patient entity.
/// </summary>
public class PatientTagsControllerTests
{
    private readonly Mock<ITagQueryService> _mockTagQueryService;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<PatientTagsController>> _mockLogger;
    private readonly PatientTagsController _controller;

    public PatientTagsControllerTests()
    {
        _mockTagQueryService = new Mock<ITagQueryService>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<PatientTagsController>>();
        _controller = new PatientTagsController(_mockTagQueryService.Object, _mockMediator.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Test GET /api/v1/patients/{patientId}/tags returns tags successfully.
    /// </summary>
    [Fact]
    public async Task GetPatientTags_WithExistingTags_ReturnsOkWithTags()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var tags = new List<TagDto>
        {
            new TagDto { Id = Guid.NewGuid(), Name = "VIP", Category = "Priority" },
            new TagDto { Id = Guid.NewGuid(), Name = "High Risk", Category = "Health" }
        };

        _mockTagQueryService
            .Setup(x => x.GetResourceTagsAsync(
                patientId,
                nameof(PatientEntity),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        var result = await _controller.GetPatientTags(patientId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        
        var returnValue = okResult.Value as dynamic;
        Assert.Equal(patientId, returnValue?.patientId);
        Assert.Equal(2, returnValue?.tags.Count);
    }

    /// <summary>
    /// Test GET /api/v1/patients/{patientId}/tags with no tags returns empty list.
    /// </summary>
    [Fact]
    public async Task GetPatientTags_WithNoTags_ReturnsOkWithEmptyList()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var tags = new List<TagDto>();

        _mockTagQueryService
            .Setup(x => x.GetResourceTagsAsync(
                patientId,
                nameof(PatientEntity),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        var result = await _controller.GetPatientTags(patientId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = okResult.Value as dynamic;
        Assert.Empty(returnValue?.tags);
    }

    /// <summary>
    /// Test POST /api/v1/patients/{patientId}/tags applies tags successfully.
    /// </summary>
    [Fact]
    public async Task ApplyPatientTags_WithValidCommand_ReturnsOkWithResponse()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var baseCommand = new ApplyTagsCommand
        {
            TagIds = new[] { Guid.NewGuid() },
            ServiceName = "SomeService"
        };

        var response = new TagAssignmentResponse
        {
            Success = true,
            Message = "Successfully applied 1 tag(s)",
            ResourceId = patientId,
            AppliedTagIds = baseCommand.TagIds,
            TotalTagsOnResource = 1,
            Errors = Enumerable.Empty<string>()
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ApplyTagsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ApplyPatientTags(patientId, baseCommand, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResponse = okResult.Value as TagAssignmentResponse;
        Assert.NotNull(returnedResponse);
        Assert.True(returnedResponse.Success);
        Assert.Equal(1, returnedResponse.TotalTagsOnResource);
    }

    /// <summary>
    /// Test POST /api/v1/patients/{patientId}/tags with invalid command returns BadRequest.
    /// </summary>
    [Fact]
    public async Task ApplyPatientTags_WithInvalidCommand_ReturnsBadRequest()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var command = new ApplyTagsCommand
        {
            TagIds = Array.Empty<Guid>(),
            ServiceName = "Patient"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ApplyTagsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("No tags provided"));

        // Act
        var result = await _controller.ApplyPatientTags(patientId, command, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    /// <summary>
    /// Test DELETE /api/v1/patients/{patientId}/tags/{tagId} removes tag successfully.
    /// </summary>
    [Fact]
    public async Task RemovePatientTag_WithExistingTag_ReturnsNoContent()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        _mockMediator
            .Setup(x => x.Send(It.IsAny<RemoveTagCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TagAssignmentResponse
            {
                Success = true,
                Message = "Tag removed successfully",
                ResourceId = patientId,
                AppliedTagIds = new[] { tagId },
                TotalTagsOnResource = 0
            });

        // Act
        var result = await _controller.RemovePatientTag(patientId, tagId, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    /// <summary>
    /// Test DELETE /api/v1/patients/{patientId}/tags/{tagId} with non-existent tag returns NotFound.
    /// </summary>
    [Fact]
    public async Task RemovePatientTag_WithNonExistentTag_ReturnsNotFound()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        _mockMediator
            .Setup(x => x.Send(It.IsAny<RemoveTagCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Tag not found"));

        // Act
        var result = await _controller.RemovePatientTag(patientId, tagId, CancellationToken.None);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    /// <summary>
    /// Test PUT /api/v1/patients/{patientId}/tags replaces all tags successfully.
    /// </summary>
    [Fact]
    public async Task SetPatientTags_WithValidCommand_ReturnsOkWithResponse()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var tagIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var baseCommand = new SetResourceTagsCommand
        {
            TagIds = tagIds,
            ServiceName = "SomeService"
        };

        var response = new TagAssignmentResponse
        {
            Success = true,
            Message = $"Successfully set {tagIds.Length} tag(s)",
            ResourceId = patientId,
            AppliedTagIds = tagIds,
            TotalTagsOnResource = tagIds.Length
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<SetResourceTagsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.SetPatientTags(patientId, baseCommand, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResponse = okResult.Value as TagAssignmentResponse;
        Assert.NotNull(returnedResponse);
        Assert.True(returnedResponse.Success);
        Assert.Equal(2, returnedResponse.TotalTagsOnResource);
    }
}
