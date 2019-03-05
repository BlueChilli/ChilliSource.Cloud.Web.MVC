using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliSource.Cloud.Web.MVC
{
    internal static class DictionaryExtensions
    {
        public static bool AddOrSkipIfExists<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                return false;

            dictionary.Add(key, value);
            return true;
        }

        public static IDictionary<TKey, TValue> Merge<TKey, TValue, TValue2>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue2> dictionary2, bool overwrite = false)
            where TValue2 : TValue
        {
            foreach (var key in dictionary2.Keys)
            {
                if (!dictionary.ContainsKey(key))
                    dictionary.Add(key, dictionary2[key]);
                else if (overwrite)
                    dictionary[key] = dictionary2[key];
            }

            return dictionary;
        }
    }
}
