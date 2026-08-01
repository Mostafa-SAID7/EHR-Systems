using System;
using System.Collections.Generic;

namespace EHRPlatform.Common.Mapping;

/// <summary>
/// Interface for object mapping/transformation.
/// Single responsibility: Object mapping contract.
/// </summary>
public interface IMapper
{
    /// <summary>
    /// Map source object to destination type.
    /// </summary>
    TDestination Map<TSource, TDestination>(TSource source) where TDestination : new();

    /// <summary>
    /// Map source object to existing destination.
    /// </summary>
    void Map<TSource, TDestination>(TSource source, TDestination destination);

    /// <summary>
    /// Map collection of source objects.
    /// </summary>
    List<TDestination> MapCollection<TSource, TDestination>(IEnumerable<TSource> source) where TDestination : new();

    /// <summary>
    /// Map dynamic object type.
    /// </summary>
    object Map(object source, Type sourceType, Type destinationType);
}
