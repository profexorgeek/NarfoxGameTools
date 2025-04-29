using Narfox.Entities;
using Narfox.Services;
using System.Collections.ObjectModel;

namespace Narfox.Extensions;

public static class CollectionExtensions
{
    /// <summary>
    /// A Linq-like method for getting a random element from an IEnumerable
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="array">The array to fetch an element from</param>
    /// <param name="rand">Optional Random instance, if not provided 
    /// the method will use the RandomService.</param>
    /// <returns>An element of type T or default(T)</returns>
    public static T Random<T>(this IEnumerable<T> enumerable, Random rand = null)
    {
        rand = rand ?? RandomService.I.UnseededRandom;
        T o;
        var c = enumerable.Count();
        if (c > 0)
        {
            o = enumerable.ElementAt(rand.Next(0, c));
        }
        else
        {
            o = default;
        }
        return o;
    }

    /// <summary>
    /// A Linq-like method for getting a random element from an array
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="array">The array to fetch an element from</param>
    /// <param name="rand">Optional Random instance, if not provided 
    /// the method will use a static default instance.</param>
    /// <returns>An element of type T or default(T)</returns>
    public static T Random<T>(this T[] array, Random rand = null)
    {
        rand = rand ?? RandomService.I.UnseededRandom;
        T o;
        var c = array.Length;
        if (c > 0)
        {
            o = array[rand.Next(0, c)];
        }
        else
        {
            o = default(T);
        }
        return o;
    }

    /// <summary>
    /// Gets the nearest entity to the provied coordinates
    /// </summary>
    /// <typeparam name="T">The Type of object</typeparam>
    /// <param name="list">A list to search</param>
    /// <param name="x">The target X coordinate</param>
    /// <param name="y">The target Y coordinate</param>
    /// <param name="isEntityValid">An optional function that determines if an object should be filtered out. 
    /// True means the object is valid and should NOT be filtered. This allows the caller to filter invalid
    /// entities such as entities that are on a friendly team.</param>
    /// <returns>The nearest entity in the list</returns>
    public static IPositionedEntity? GetNearestEntity(this List<IPositionedEntity> list,
        float x, float y,
        Func<IPositionedEntity, bool>? isEntityValid = null)
    {
        IPositionedEntity? nearest = null;
        float distToNearest = 0;
        float distToCurrent = 0;
        for (var i = list.Count - 1; i > -1; i--)
        {
            var current = list[i];

            // if we have a filter function and the filter function
            // returns a false value, we should skip this entity
            if (isEntityValid != null && isEntityValid(current) == false)
            {
                continue;
            }

            distToCurrent = current.DistanceTo(x, y);

            if (nearest == null)
            {
                nearest = current;
                distToNearest = distToCurrent;
            }
            else
            {
                if (distToCurrent < distToNearest)
                {
                    nearest = current;
                    distToNearest = distToCurrent;
                }
            }
        }
        return nearest;
    }

    /// <summary>
    /// Orders an observable collection and returns a new observable collection
    /// with the ordered items
    /// </summary>
    /// <typeparam name="T">The type of item in the collection</typeparam>
    /// <typeparam name="TKey">The key to order by, used in the provided lambda function</typeparam>
    /// <param name="collection">The collection to order</param>
    /// <param name="keySelector">The sorting function to use</param>
    /// <returns></returns>
    public static ObservableCollection<T> OrderObservableCollectionBy<T, TKey>(
    this ObservableCollection<T> collection,
    Func<T, TKey> keySelector)
    {
        var sorted = collection.OrderBy(keySelector).ToList();
        return new ObservableCollection<T>(sorted);
    }
}
