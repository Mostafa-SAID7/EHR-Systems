#nullable enable

using EHRPlatform.Common.Domain.Constants;

namespace EHRPlatform.Common.Domain.Exceptions;

/// <summary>
/// Exception thrown when a search operation fails.
/// Single responsibility: Represent search failures only.
/// </summary>
public class SearchException : DomainException
{
    /// <summary>
    /// Error code for search errors.
    /// </summary>
    public override string ErrorCode => ErrorCode.SearchError;

    /// <summary>
    /// Initialize with search error message.
    /// </summary>
    public SearchException(string message) 
        : base(message, ErrorCode.SearchError) 
    { 
    }

    /// <summary>
    /// Initialize with search error message and inner exception.
    /// </summary>
    public SearchException(string message, Exception inner) 
        : base(message, ErrorCode.SearchError, inner) 
    { 
    }
}
