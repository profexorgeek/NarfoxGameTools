using NarfoxGameTools.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

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
        public static T Random<T>(this Array array, Random rand = null)
        {
            rand = rand ?? RandomService.Random;
            T o;
            var c = array.Length;
            if (c > 0)
            {
                try
                {
                    o = (T)array.GetValue(rand.Next(0, c));
                }
                catch
                {
                    o = default(T);
                }
            }
            else
            {
                o = default(T);
            }
            return o;
        }

        /// <summary>
        /// Helper extension to convert dictionary to name value collection.
        /// Dictionaries are easier to use, especially when merging and
        /// checking for overriding keys. This allows easy conversion
        /// of a dictionary to the NameValueCollection object
        /// expected by WebClient.UploadValues()
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
