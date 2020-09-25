using System.Collections.Generic;
using System.Collections.Specialized;

namespace NarfoxGameTools.Extensions
{
    public static class DictionaryExtensions
    {
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
