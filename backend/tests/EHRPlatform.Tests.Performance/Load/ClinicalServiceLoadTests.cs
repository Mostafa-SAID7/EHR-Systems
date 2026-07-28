using BenchmarkDotNet.Attributes;
using FluentAssertions;
using EHRPlatform.Services.Clinical.Domain.Entities;
using EHRPlatform.Tests.Common.Builders;
using EHRPlatform.Tests.Common.Helpers;
using Xunit;

namespace EHRPlatform.Tests.Performance.Load;

/// <summary>
/// Performance tests for Clinical Service.
/// Tests throughput, latency, and resource usage under load.
/// HIPAA: Performance impacts availability; must meet SLA targets.
/// </summary>
[MemoryDiagnoser]
public class ClinicalServiceLoadTests
{
    [Fact]
    public void ClinicalNote_Creation_ShouldCompleteFast()
    {
        // Arrange
        var iterations = 1000;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var note = new ClinicalNote
            {
                Id = Guid.NewGuid(),
                PatientId = Guid.NewGuid(),
                ProviderId = Guid.NewGuid(),
                EncounterDate = DateTime.UtcNow.AddDays(-1),
                EncounterType = "Office",
                Status = "Draft"
            };
        }

        stopwatch.Stop();

        // Assert
        var averageMs = (double)stopwatch.ElapsedMilliseconds / iterations;
        averageMs.Should().BeLessThan(1.0, $"Note creation should average < 1ms, got {averageMs}ms");
    }

    [Fact]
    public void ClinicalNote_DiagnosisAddition_ShouldScaleLinarly()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };

        var diagnoses = 100;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < diagnoses; i++)
        {
            var code = $"[{i:D3}]"; // Synthetic ICD-10-like code
            note.AddDiagnosis("I10", $"Diagnosis {i}", i % 2 == 0 ? "Principal" : "Secondary");
        }

        stopwatch.Stop();

        // Assert
        note.Diagnoses.Should().HaveCount(diagnoses);
        var averageMs = (double)stopwatch.ElapsedMilliseconds / diagnoses;
        averageMs.Should().BeLessThan(10.0, $"Adding diagnosis should average < 10ms, got {averageMs}ms");
    }

    [Fact]
    public void ClinicalNote_VitalRecording_ShouldBeFast()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };

        var iterations = 100;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            note.RecordVitals(
                temperature: 98.6m + (i * 0.01m),
                systolic: 120 + i,
                diastolic: 80 + (i / 2),
                heartRate: 70 + i,
                respiratoryRate: 16 + (i / 10)
            );
        }

        stopwatch.Stop();

        // Assert
        note.VitalSigns.Should().HaveCount(iterations);
        var averageMs = (double)stopwatch.ElapsedMilliseconds / iterations;
        averageMs.Should().BeLessThan(5.0, $"Vital recording should average < 5ms, got {averageMs}ms");
    }

    [Fact]
    public void ClinicalNote_EventGeneration_ShouldNotBlockCreation()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };

        var operations = 50;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < operations; i++)
        {
            note.AddDiagnosis("I10", "Test", "Principal");
            note.RecordVitals(98.6m, 120, 80, 72, 16);
            note.AddProcedure("Test", "TEST", "Result");
        }

        stopwatch.Stop();

        // Assert
        note.GetDomainEvents().Should().HaveCount(operations * 3); // 3 events per iteration
        var averageMs = (double)stopwatch.ElapsedMilliseconds / (operations * 3);
        averageMs.Should().BeLessThan(2.0, $"Event generation should average < 2ms, got {averageMs}ms");
    }

    [Fact]
    public void ClinicalNote_Finalization_ShouldNotBlockOnLargeNotes()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            Status = "Draft",
            Subjective = new string('X', 5000),
            Objective = new string('Y', 5000),
            Assessment = new string('Z', 5000),
            Plan = new string('W', 5000)
        };

        // Add lots of data
        for (int i = 0; i < 50; i++)
        {
            note.AddDiagnosis("I10", "Diagnosis", "Principal");
            note.RecordVitals(98.6m, 120, 80, 72, 16);
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        note.Finalize();

        stopwatch.Stop();

        // Assert
        note.Status.Should().Be("Finalized");
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100, "Finalization should complete < 100ms");
    }

    [Fact]
    public void ClinicalNote_ConcurrentCreation_ShouldHandleMultipleThreads()
    {
        // Arrange
        var threadCount = 10;
        var notesPerThread = 100;
        var createdNotes = new System.Collections.Concurrent.ConcurrentBag<ClinicalNote>();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        Parallel.For(0, threadCount, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, _ =>
        {
            for (int i = 0; i < notesPerThread; i++)
            {
                var note = new ClinicalNote
                {
                    Id = Guid.NewGuid(),
                    PatientId = Guid.NewGuid(),
                    ProviderId = Guid.NewGuid(),
                    EncounterDate = DateTime.UtcNow.AddDays(-1),
                    Status = "Draft"
                };
                createdNotes.Add(note);
            }
        });

        stopwatch.Stop();

        // Assert
        createdNotes.Should().HaveCount(threadCount * notesPerThread);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "Concurrent creation of 1000 notes should take < 5s");
    }

    [Fact]
    public void ClinicalNote_MemoryUsage_ShouldBeEfficient()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);

        // Act - Create and manipulate many notes
        var notes = new List<ClinicalNote>();
        for (int i = 0; i < 1000; i++)
        {
            var note = new ClinicalNote
            {
                Id = Guid.NewGuid(),
                PatientId = Guid.NewGuid(),
                ProviderId = Guid.NewGuid(),
                Subjective = new string('A', 100),
                Objective = new string('B', 100)
            };

            for (int j = 0; j < 10; j++)
            {
                note.AddDiagnosis("I10", "Test", "Principal");
            }

            notes.Add(note);
        }

        var afterCreation = GC.GetTotalMemory(false);
        notes.Clear();
        notes = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        var afterCleanup = GC.GetTotalMemory(true);

        // Assert
        var memoryUsed = afterCreation - initialMemory;
        var memoryRetained = afterCleanup - initialMemory;

        memoryUsed.Should().BeLessThan(500_000_000, "1000 notes with data should use < 500MB");
        memoryRetained.Should().BeLessThan(100_000_000, "After cleanup, retained memory should be < 100MB");
    }

    [Fact]
    public void ClinicalNote_BatchOperations_ShouldMaintainConsistency()
    {
        // Arrange
        var notes = new List<ClinicalNote>();
        var batchSize = 500;

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < batchSize; i++)
        {
            var note = new ClinicalNote
            {
                Id = Guid.NewGuid(),
                PatientId = Guid.NewGuid(),
                ProviderId = Guid.NewGuid(),
                EncounterDate = DateTime.UtcNow.AddDays(-i),
                Status = "Draft"
            };

            note.AddDiagnosis("I10", "Hypertension", "Principal");
            note.RecordVitals(98.6m, 120, 80, 72, 16);
            note.Finalize();

            notes.Add(note);
        }

        stopwatch.Stop();

        // Assert
        notes.Should().HaveCount(batchSize);
        notes.All(n => n.Status == "Finalized").Should().BeTrue();
        var averageMs = (double)stopwatch.ElapsedMilliseconds / batchSize;
        averageMs.Should().BeLessThan(10.0, $"Batch processing should average < 10ms per note, got {averageMs}ms");
    }
}
