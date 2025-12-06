using System;
using System.Collections.Generic;
using System.Linq;

namespace DZModForger.Utilities
{
    /// <summary>
    /// Collection utility extension methods
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Checks if collection is null or empty
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }

        /// <summary>
        /// Adds range of items to collection
        /// </summary>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection == null || items == null)
                return;

            foreach (var item in items)
                collection.Add(item);
        }

        /// <summary>
        /// Removes range of items from collection
        /// </summary>
        public static void RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection == null || items == null)
                return;

            foreach (var item in items)
                collection.Remove(item);
        }

        /// <summary>
        /// Finds first item matching predicate
        /// </summary>
        public static T FirstOrDefault<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            if (collection == null)
                return default;

            foreach (var item in collection)
            {
                if (predicate(item))
                    return item;
            }

            return default;
        }

        /// <summary>
        /// Finds last item matching predicate
        /// </summary>
        public static T LastOrDefault<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            if (collection == null)
                return default;

            T last = default;
            foreach (var item in collection)
            {
                if (predicate(item))
                    last = item;
            }

            return last;
        }

        /// <summary>
        /// Chunks collection into groups of specified size
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> collection, int chunkSize)
        {
            if (collection == null || chunkSize <= 0)
                yield break;

            var chunk = new List<T>(chunkSize);
            foreach (var item in collection)
            {
                chunk.Add(item);
                if (chunk.Count == chunkSize)
                {
                    yield return chunk;
                    chunk = new List<T>(chunkSize);
                }
            }

            if (chunk.Count > 0)
                yield return chunk;
        }

        /// <summary>
        /// Distinct items by specified key
        /// </summary>
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> collection, Func<T, TKey> keySelector)
        {
            if (collection == null)
                return Enumerable.Empty<T>();

            var seen = new HashSet<TKey>();
            foreach (var item in collection)
            {
                var key = keySelector(item);
                if (seen.Add(key))
                    yield return item;
            }
        }

        /// <summary>
        /// Flattens nested collections
        /// </summary>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> collection)
        {
            if (collection == null)
                return Enumerable.Empty<T>();

            return collection.SelectMany(x => x);
        }

        /// <summary>
        /// Performs action on each item
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if (collection == null || action == null)
                return;

            foreach (var item in collection)
                action(item);
        }

        /// <summary>
        /// Converts collection to observable collection
        /// </summary>
        public static System.Collections.ObjectModel.ObservableCollection<T> ToObservableCollection<T>(
            this IEnumerable<T> collection)
        {
            var observableCollection = new System.Collections.ObjectModel.ObservableCollection<T>();
            if (collection != null)
            {
                collection.ForEach(observableCollection.Add);
            }

            return observableCollection;
        }

        /// <summary>
        /// Shuffles collection using Fisher-Yates algorithm
        /// </summary>
        public static void Shuffle<T>(this IList<T> collection)
        {
            if (collection == null || collection.Count <= 1)
                return;

            var random = new Random();
            for (int i = collection.Count - 1; i > 0; i--)
            {
                int randomIndex = random.Next(i + 1);
                (collection[i], collection[randomIndex]) = (collection[randomIndex], collection[i]);
            }
        }

        /// <summary>
        /// Groups items by key and counts them
        /// </summary>
        public static Dictionary<TKey, int> CountBy<T, TKey>(this IEnumerable<T> collection, Func<T, TKey> keySelector)
        {
            var result = new Dictionary<TKey, int>();
            if (collection == null)
                return result;

            foreach (var item in collection)
            {
                var key = keySelector(item);
                if (result.ContainsKey(key))
                    result[key]++;
                else
                    result[key] = 1;
            }

            return result;
        }
    }
}
