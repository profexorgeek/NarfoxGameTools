using FlatRedBall;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Math;
using NarfoxGameTools.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace NarfoxGameTools.Extensions
{
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
            rand = rand ?? RandomService.Random;
            T o;
            var c = enumerable.Count();
            if (c > 0)
            {
                o = enumerable.ElementAt(rand.Next(0, c));
            }
            else
            {
                o = default(T);
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
            rand = rand ?? RandomService.Random;
            T o;
            var c = array.Length;
            if(c > 0)
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
        /// Gets the nearest PositionedObject of type T to the X and Y coordinates from 
        /// </summary>
        /// <typeparam name="T">The Type of object</typeparam>
        /// <param name="list">A list to search</param>
        /// <param name="x">The target X coordinate</param>
        /// <param name="y">The target Y coordinate</param>
        /// <param name="isEntityValid">A function that determines if an object should be filtered out. 
        /// True means the object is valid and should NOT be filtered</param>
        /// <returns></returns>
        public static T GetNearestToPoint<T>(this PositionedObjectList<T> list, float x, float y, Func<T, bool> isEntityValid = null) where T : PositionedObject
        {
            T nearest = null;
            float distToNearest = 0;
            float distToCurrent = 0;
            for (var i = list.Count - 1; i > -1; i--)
            {
                var current = list[i];

                // if we have a filter function and the filter function
                // returns a false value, we should skip this entity
                if(isEntityValid != null && isEntityValid(current) == false)
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
        /// Gets a random note from a TileNodeNetwork. This is usually used
        /// in the context of AI to get a bot to path to a random node.
        /// </summary>
        /// <param name="network">The tile node network</param>
        /// <param name="rand">The random object to be ued, defaults to RandomService</param>
        /// <returns>A random node</returns>
        public static PositionedNode Random(this TileNodeNetwork network, Random rand = null)
        {
            rand = rand ?? RandomService.Random;
            return network.Nodes.Random(rand);
        }

        /// <summary>
        /// Converts any IEnumerable into an ObservableCollection. This is handy for
        /// converting standard lists into an MVVM-friendly bound collection
        /// </summary>
        /// <typeparam name="T">The list Type</typeparam>
        /// <param name="list">The list to convert</param>
        /// <returns>An observable collection of the objects in the provided list</returns>
        public static ObservableCollection<T> AsObservable<T>(this IEnumerable<T> list)
        {
            var collection = new ObservableCollection<T>();
            foreach(var item in list)
            {
                collection.Add(item);
            }
            return collection;
        }

        /// <summary>
        /// Helper extension to convert dictionary to name value collection.
        /// Dictionaries are easier to use, especially when merging and
        /// checking for overriding keys. This allows easy conversion
        /// of a dictionary to the NameValueCollection object
        /// expected by WebClient.UploadValues() in the GoogleAnalyticsService
        /// </summary>
        /// <param name="dict">The dictionary</param>
        /// <returns>NameValueCollection created from dictionary</returns>
        public static NameValueCollection ToNameValueCollection(this Dictionary<string, string> dict)
        {
            var nvc = new NameValueCollection();
            foreach (var kvp in dict)
            {
                nvc.Add(kvp.Key, kvp.Value);
            }
            return nvc;
        }

    }
}
