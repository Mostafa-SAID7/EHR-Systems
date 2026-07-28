using FluentAssertions;
using Moq;
using EHRPlatform.Services.Clinical.Domain.Entities;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Queries;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Handlers;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Responses;
using EHRPlatform.Common.Data;
using Microsoft.Extensions.Logging;
using Xunit;

namespace EHRPlatform.Tests.Unit.Application;

/// <summary>
/// Unit tests for Clinical Service query handlers.
/// Tests query patterns, filtering, and response mapping.
/// HIPAA: Queries must respect access control and authorization.
/// </summary>
public class ClinicalQueryTests
{
    private readonly Mock<IRepository<ClinicalNote>> _mockRepository;
    private readonly Mock<ILogger<GetPatientClinicalTimelineQueryHandler>> _mockLogger;

    public ClinicalQueryTests()
    {
        _mockRepository = new Mock<IRepository<ClinicalNote>>();
        _mockLogger = new Mock<ILogger<GetPatientClinicalTimelineQueryHandler>>();
    }

    [Fact]
    public async Task GetPatientClinicalTimelineQuery_WithValidPatientId_ShouldReturnNotes()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var notes = new List<ClinicalNote>
        {
            new ClinicalNote
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                ProviderId = Guid.NewGuid(),
                EncounterDate = DateTime.UtcNow.AddDays(-5),
                EncounterType = "Office",
                Status = "Finalized"
            },
            new ClinicalNote
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                ProviderId = Guid.NewGuid(),
                EncounterDate = DateTime.UtcNow.AddDays(-2),
                EncounterType = "Telehealth",
                Status = "Draft"
            }
        };

        var query = new GetPatientClinicalTimelineQuery { PatientId = patientId };

        _mockRepository.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ClinicalNote, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(notes.AsQueryable().Where(n => n.PatientId == patientId).ToList());

        var handler = new GetPatientClinicalTimelineQueryHandler(_mockRepository, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPatientClinicalTimelineQuery_ShouldReturnNotesInChronologicalOrder()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var oldDate = DateTime.UtcNow.AddDays(-30);
        var middleDate = DateTime.UtcNow.AddDays(-15);
        var recentDate = DateTime.UtcNow.AddDays(-2);

        var notes = new List<ClinicalNote>
        {
            new ClinicalNote
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                ProviderId = Guid.NewGuid(),
                EncounterDate = oldDate
            },
            new ClinicalNote
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                ProviderId = Guid.NewGuid(),
                EncounterDate = recentDate
            },
            new ClinicalNote
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                ProviderId = Guid.NewGuid(),
                EncounterDate = middleDate
            }
        };

        var query = new GetPatientClinicalTimelineQuery { PatientId = patientId };

        _mockRepository.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ClinicalNote, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(notes.OrderByDescending(n => n.EncounterDate).ToList());

        var handler = new GetPatientClinicalTimelineQueryHandler(_mockRepository, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result[0].EncounterDate.Should().BeGreaterThan(result[1].EncounterDate);
        result[1].EncounterDate.Should().BeGreaterThan(result[2].EncounterDate);
    }

    [Fact]
    public async Task GetPatientDiagnosisHistoryQuery_ShouldReturnDiagnosesOrderedByDate()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var noteId = Guid.NewGuid();

        var note = new ClinicalNote
        {
            Id = noteId,
            PatientId = patientId,
            ProviderId = Guid.NewGuid(),
            EncounterDate = DateTime.UtcNow.AddDays(-10)
        };

        note.AddDiagnosis("I10", "Essential hypertension", "Principal");
        note.AddDiagnosis("E11", "Type 2 diabetes", "Secondary");

        var query = new GetPatientDiagnosisHistoryQuery { PatientId = patientId };

        _mockRepository.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ClinicalNote, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClinicalNote> { note });

        var handler = new GetPatientDiagnosisHistoryQueryHandler(_mockRepository, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Count().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetClinicalNoteByIdQuery_WithValidId_ShouldReturnNote()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var note = new ClinicalNote
        {
            Id = noteId,
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            EncounterType = "Office",
            Status = "Finalized"
        };

        var query = new GetClinicalNoteByIdQuery { ClinicalNoteId = noteId };

        _mockRepository.Setup(r => r.GetByIdAsync(noteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        var handler = new GetClinicalNoteByIdQueryHandler(_mockRepository, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Equal(noteId);
    }

    [Fact]
    public async Task GetClinicalNoteByIdQuery_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var query = new GetClinicalNoteByIdQuery { ClinicalNoteId = noteId };

        _mockRepository.Setup(r => r.GetByIdAsync(noteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClinicalNote?)null);

        var handler = new GetClinicalNoteByIdQueryHandler(_mockRepository, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProviderPatientsQuery_ShouldReturnPatientsForProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var patientId1 = Guid.NewGuid();
        var patientId2 = Guid.NewGuid();

        var notes = new List<ClinicalNote>
        {
            new ClinicalNote { Id = Guid.NewGuid(), ProviderId = providerId, PatientId = patientId1 },
            new ClinicalNote { Id = Guid.NewGuid(), ProviderId = providerId, PatientId = patientId2 }
        };

        var query = new GetProviderPatientsQuery { ProviderId = providerId };

        _mockRepository.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ClinicalNote, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(notes);

        var handler = new GetProviderPatientsQueryHandler(_mockRepository, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var uniquePatients = result.Select(n => n.PatientId).Distinct();
        uniquePatients.Should().Contain(patientId1);
        uniquePatients.Should().Contain(patientId2);
    }

    [Fact]
    public async Task GetClinicalNotesInDateRangeQuery_ShouldFilterByDate()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow.AddDays(-1);

        var notes = new List<ClinicalNote>
        {
            new ClinicalNote
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                ProviderId = Guid.NewGuid(),
                EncounterDate = startDate.AddDays(1)
            },
            new ClinicalNote
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                ProviderId = Guid.NewGuid(),
                EncounterDate = startDate.AddDays(15)
            },
            new ClinicalNote
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                ProviderId = Guid.NewGuid(),
                EncounterDate = endDate
            }
        };

        var query = new GetClinicalNotesInDateRangeQuery
        {
            PatientId = patientId,
            StartDate = startDate,
            EndDate = endDate
        };

        _mockRepository.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ClinicalNote, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(notes.Where(n => n.EncounterDate >= startDate && n.EncounterDate <= endDate).ToList());

        var handler = new GetClinicalNotesInDateRangeQueryHandler(_mockRepository, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.All(n => n.EncounterDate >= startDate && n.EncounterDate <= endDate).Should().BeTrue();
    }
}
