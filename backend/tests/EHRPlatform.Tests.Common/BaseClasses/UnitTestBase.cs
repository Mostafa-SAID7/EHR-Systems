using EHRPlatform.Tests.Common.Helpers;
using Moq;
using System;
using System.Collections.Generic;

namespace EHRPlatform.Tests.Common.BaseClasses;

/// <summary>
/// Base class for unit tests.
/// Provides common setup, mocks, and utilities for isolated unit testing.
/// </summary>
public abstract class UnitTestBase : IDisposable
{
    /// <summary>
    /// Gets the mock repository for managing all mocks.
    /// </summary>
    protected MockRepository MockRepository { get; private set; }

    /// <summary>
    /// Gets common mocks used across tests.
    /// </summary>
    protected Dictionary<string, Mock> CommonMocks { get; private set; }

    protected UnitTestBase()
    {
        MockRepository = new MockRepository(MockBehavior.Strict);
        CommonMocks = new Dictionary<string, Mock>();
        Setup();
    }

    /// <summary>
    /// Override to perform setup before each test.
    /// </summary>
    protected virtual void Setup()
    {
    }

    /// <summary>
    /// Override to perform cleanup after each test.
    /// </summary>
    protected virtual void Teardown()
    {
    }

    /// <summary>
    /// Creates a strict mock (fails if unmocked method is called).
    /// </summary>
    protected Mock<T> CreateStrictMock<T>() where T : class
    {
        return MockRepository.Create<T>(MockBehavior.Strict);
    }

    /// <summary>
    /// Creates a loose mock (returns default values for unmocked methods).
    /// </summary>
    protected Mock<T> CreateLooseMock<T>() where T : class
    {
        return MockRepository.Create<T>(MockBehavior.Loose);
    }

    /// <summary>
    /// Stores a mock in common mocks for reuse.
    /// </summary>
    protected void RegisterMock<T>(string name, Mock<T> mock) where T : class
    {
        CommonMocks[name] = mock;
    }

    /// <summary>
    /// Retrieves a registered mock.
    /// </summary>
    protected Mock<T> GetMock<T>(string name) where T : class
    {
        if (CommonMocks.TryGetValue(name, out var mock))
        {
            return mock as Mock<T>;
        }
        throw new KeyNotFoundException($"Mock '{name}' not found");
    }

    /// <summary>
    /// Verifies all mocks have been satisfied.
    /// </summary>
    protected void VerifyAllMocks()
    {
        MockRepository.VerifyAll();
    }

    /// <summary>
    /// Generates test data using the TestDataGenerator.
    /// </summary>
    protected string GenerateTestId() => TestDataGenerator.GenerateId();

    /// <summary>
    /// Generates a test email.
    /// </summary>
    protected string GenerateTestEmail() => TestDataGenerator.GenerateEmail();

    /// <summary>
    /// Generates a test phone number.
    /// </summary>
    protected string GenerateTestPhone() => TestDataGenerator.GeneratePhoneNumber();

    /// <summary>
    /// Generates a test name.
    /// </summary>
    protected string GenerateTestName() => TestDataGenerator.GenerateName();

    /// <summary>
    /// Generates a random date.
    /// </summary>
    protected DateTime GenerateTestDate(DateTime? minDate = null, DateTime? maxDate = null)
        => TestDataGenerator.GenerateRandomDate(minDate, maxDate);

    /// <summary>
    /// Generates a test password.
    /// </summary>
    protected string GenerateTestPassword() => TestDataGenerator.GeneratePassword();

    /// <summary>
    /// Cleans up resources.
    /// </summary>
    public virtual void Dispose()
    {
        Teardown();
        MockRepository?.Dispose();
        CommonMocks?.Clear();
    }
}
