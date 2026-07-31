#nullable enable

namespace EHRPlatform.Common.Shared.Utilities.Helpers;

/// <summary>
/// Helper methods for collection operations and LINQ patterns.
/// Centralizes filtering, mapping, and common collection patterns.
/// </summary>
public static class CollectionHelper
{
    /// <summary>
    /// Check if collection is null or empty.
    /// </summary>
    public static bool IsEmpty<T>(IEnumerable<T>? collection)
    {
        return collection == null || !collection.Any();
    }

    /// <summary>
    /// Check if collection has items.
    /// </summary>
    public static bool HasItems<T>(IEnumerable<T>? collection)
    {
        return collection != null && collection.Any();
    }

    /// <summary>
    /// Get count of items in collection safely.
    /// </summary>
    public static int Count<T>(IEnumerable<T>? collection)
    {
        return collection?.Count() ?? 0;
    }

    /// <summary>
    /// Get first item or default value.
    /// </summary>
    public static T? FirstOrDefault<T>(IEnumerable<T>? collection, T? defaultValue = default)
    {
        return collection?.FirstOrDefault() ?? defaultValue;
    }

    /// <summary>
    /// Get last item or default value.
    /// </summary>
    public static T? LastOrDefault<T>(IEnumerable<T>? collection, T? defaultValue = default)
    {
        return collection?.LastOrDefault() ?? defaultValue;
    }

    /// <summary>
    /// Chunk collection into groups of specified size.
    /// </summary>
    public static IEnumerable<IEnumerable<T>> Chunk<T>(IEnumerable<T>? collection, int chunkSize)
    {
        if (collection == null || chunkSize <= 0)
            yield break;

        var list = collection.ToList();
        for (int i = 0; i < list.Count; i += chunkSize)
            yield return list.Skip(i).Take(chunkSize);
    }

    /// <summary>
    /// Flatten nested enumerable into single enumerable.
    /// </summary>
    public static IEnumerable<T> Flatten<T>(IEnumerable<IEnumerable<T>>? nestedCollection)
    {
        if (nestedCollection == null)
            yield break;

        foreach (var collection in nestedCollection)
        {
            if (collection == null)
                continue;

            foreach (var item in collection)
                yield return item;
        }
    }

    /// <summary>
    /// Filter collection by predicate and return as list.
    /// </summary>
    public static List<T> Filter<T>(IEnumerable<T>? collection, Func<T, bool> predicate)
    {
        if (collection == null)
            return new List<T>();

        return collection.Where(predicate).ToList();
    }

    /// <summary>
    /// Map collection to another type.
    /// </summary>
    public static List<TResult> Map<T, TResult>(IEnumerable<T>? collection, Func<T, TResult> selector)
    {
        if (collection == null)
            return new List<TResult>();

        return collection.Select(selector).ToList();
    }

    /// <summary>
    /// Group collection by key selector.
    /// </summary>
    public static Dictionary<TKey, List<T>> GroupBy<T, TKey>(
        IEnumerable<T>? collection,
        Func<T, TKey> keySelector) where TKey : notnull
    {
        if (collection == null)
            return new Dictionary<TKey, List<T>>();

        return collection
            .GroupBy(keySelector)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    /// Check if all items in collection match predicate.
    /// </summary>
    public static bool All<T>(IEnumerable<T>? collection, Func<T, bool> predicate)
    {
        if (collection == null)
            return true;

        return collection.All(predicate);
    }

    /// <summary>
    /// Check if any item in collection matches predicate.
    /// </summary>
    public static bool Any<T>(IEnumerable<T>? collection, Func<T, bool>? predicate = null)
    {
        if (collection == null)
            return false;

        return predicate == null ? collection.Any() : collection.Any(predicate);
    }

    /// <summary>
    /// Distinct items in collection using key selector.
    /// </summary>
    public static List<T> Distinct<T, TKey>(IEnumerable<T>? collection, Func<T, TKey> keySelector)
    {
        if (collection == null)
            return new List<T>();

        var seen = new HashSet<TKey>();
        var result = new List<T>();

        foreach (var item in collection)
        {
            var key = keySelector(item);
            if (seen.Add(key))
                result.Add(item);
        }

        return result;
    }

    /// <summary>
    /// Order collection by key selector.
    /// </summary>
    public static List<T> OrderBy<T, TKey>(
        IEnumerable<T>? collection,
        Func<T, TKey> keySelector,
        bool ascending = true)
    {
        if (collection == null)
            return new List<T>();

        return ascending
            ? collection.OrderBy(keySelector).ToList()
            : collection.OrderByDescending(keySelector).ToList();
    }

    /// <summary>
    /// Check if item exists in collection using predicate.
    /// </summary>
    public static bool Contains<T>(IEnumerable<T>? collection, Func<T, bool> predicate)
    {
        if (collection == null)
            return false;

        return collection.Any(predicate);
    }

    /// <summary>
    /// Merge multiple collections into single collection.
    /// </summary>
    public static List<T> Merge<T>(params IEnumerable<T>?[] collections)
    {
        var result = new List<T>();

        foreach (var collection in collections)
        {
            if (collection != null)
                result.AddRange(collection);
        }

        return result;
    }

    /// <summary>
    /// Get duplicates in collection by key selector.
    /// </summary>
    public static List<T> GetDuplicates<T, TKey>(
        IEnumerable<T>? collection,
        Func<T, TKey> keySelector) where TKey : notnull
    {
        if (collection == null)
            return new List<T>();

        return collection
            .GroupBy(keySelector)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g)
            .ToList();
    }
}
