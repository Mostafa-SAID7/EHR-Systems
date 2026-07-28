#nullable enable

using System;
using Moq;
using EHRPlatform.Common.Data;

namespace EHRPlatform.Tests.Common.Base;

/// <summary>
/// Base class for unit tests providing common mocks and utilities.
/// </summary>
public abstract class UnitTestBase
{
    protected readonly Mock<IRepository<object>> MockRepository;
    protected readonly Mock<IUnitOfWork> MockUnitOfWork;

    public UnitTestBase()
    {
        MockRepository = new Mock<IRepository<object>>();
        MockUnitOfWork = new Mock<IUnitOfWork>();
    }

    /// <summary>
    /// Setup unit of work with default behavior.
    /// </summary>
    protected void SetupUnitOfWork()
    {
        MockUnitOfWork
            .Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);
    }

    /// <summary>
    /// Create a strict mock (no default values).
    /// </summary>
    protected Mock<T> CreateStrictMock<T>() where T : class
    {
        return new Mock<T>(MockBehavior.Strict);
    }

    /// <summary>
    /// Create a loose mock (default values).
    /// </summary>
    protected Mock<T> CreateLooseMock<T>() where T : class
    {
        return new Mock<T>(MockBehavior.Loose);
    }
}
