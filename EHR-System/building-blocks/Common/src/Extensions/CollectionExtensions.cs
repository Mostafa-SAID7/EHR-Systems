using System;
using System.Collections.Generic;
using System.Linq;

namespace EHRPlatform.Common.Extensions;

/// <summary>
/// Collection manipulation and query extensions.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Check if collection is null or empty.
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }

    /// <summary>
    /// Check if collection has any items.
    /// </summary>
    public static bool HasItems<T>(this IEnumerable<T>? source)
    {
        return source?.Any() == true;
    }

    /// <summary>
    /// Batch collection into chunks of specified size.
    /// </summary>
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        if (batchSize <= 0)
            throw new ArgumentException("Batch size must be greater than 0", nameof(batchSize));

        using var enumerator = source.GetEnumerator();

        while (enumerator.MoveNext())
        {
            yield return InnerBatch(enumerator, batchSize - 1);
        }
    }

    private static IEnumerable<T> InnerBatch<T>(IEnumerator<T> enumerator, int batchSize)
    {
        yield return enumerator.Current;

        int count = 0;
        while (count < batchSize && enumerator.MoveNext())
        {
            yield return enumerator.Current;
            count++;
        }
    }

    /// <summary>
    /// Safely get first item or default from collection.
    /// </summary>
    public static T? GetFirstOrDefault<T>(this IEnumerable<T>? source, T? defaultValue = default)
    {
        return source?.FirstOrDefault() ?? defaultValue;
    }

    /// <summary>
    /// Safely get item at index or default.
    /// </summary>
    public static T? GetAtIndexOrDefault<T>(this IEnumerable<T>? source, int index, T? defaultValue = default)
    {
        if (source == null)
            return defaultValue;

        if (source is IList<T> list)
            return index >= 0 && index < list.Count ? list[index] : defaultValue;

        return source.ElementAtOrDefault(index) ?? defaultValue;
    }

    /// <summary>
    /// Flatten nested collections into single enumerable.
    /// </summary>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
    {
        foreach (var collection in source)
        {
            foreach (var item in collection)
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Distinct by key selector (removes duplicates based on key).
    /// </summary>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        var seen = new HashSet<TKey>();
        foreach (var item in source)
        {
            var key = keySelector(item);
            if (seen.Add(key))
                yield return item;
        }
    }

    /// <summary>
    /// Chunk collection by size (alternative to Batch).
    /// </summary>
    public static List<List<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
    {
        var chunks = new List<List<T>>();
        var chunk = new List<T>();

        foreach (var item in source)
        {
            chunk.Add(item);
            if (chunk.Count == chunkSize)
            {
                chunks.Add(chunk);
                chunk = new List<T>();
            }
        }

        if (chunk.Count > 0)
            chunks.Add(chunk);

        return chunks;
    }

    /// <summary>
    /// Order by if condition is true, otherwise return as-is.
    /// </summary>
    public static IOrderedEnumerable<T> OrderByIf<T, TKey>(
        this IEnumerable<T> source,
        bool condition,
        Func<T, TKey> keySelector)
    {
        return condition ? source.OrderBy(keySelector) : source.OrderBy(x => default);
    }

    /// <summary>
    /// Return distinct items based on multiple key selectors.
    /// </summary>
    public static IEnumerable<T> DistinctByMultiple<T>(
        this IEnumerable<T> source,
        params Func<T, object>[] keySelectors)
    {
        var seen = new HashSet<string>();
        foreach (var item in source)
        {
            var key = string.Join("_", keySelectors.Select(ks => ks(item)));
            if (seen.Add(key))
                yield return item;
        }
    }

    /// <summary>
    /// Check if any items match all conditions (AND logic).
    /// </summary>
    public static bool AllMatch<T>(this IEnumerable<T> source, params Func<T, bool>[] predicates)
    {
        return source.Any(item => predicates.All(p => p(item)));
    }

    /// <summary>
    /// Check if any items match any condition (OR logic).
    /// </summary>
    public static bool AnyMatch<T>(this IEnumerable<T> source, params Func<T, bool>[] predicates)
    {
        return source.Any(item => predicates.Any(p => p(item)));
    }

    /// <summary>
    /// Group items by value with ordering.
    /// </summary>
    public static Dictionary<TKey, List<T>> GroupByWithCount<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector) where TKey : notnull
    {
        return source
            .GroupBy(keySelector)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    /// Paginate collection (skip + take).
    /// </summary>
    public static (IEnumerable<T> Items, int TotalCount, int PageNumber, int PageSize) Paginate<T>(
        this IEnumerable<T> source,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var total = source.Count();
        var items = source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (items, total, pageNumber, pageSize);
    }

    /// <summary>
    /// Ensure collection has minimum count or throw.
    /// </summary>
    public static IEnumerable<T> RequireCount<T>(this IEnumerable<T> source, int minCount, string? message = null)
    {
        var list = source.ToList();
        if (list.Count < minCount)
            throw new InvalidOperationException(message ?? $"Expected at least {minCount} items, got {list.Count}");

        return list;
    }
}
