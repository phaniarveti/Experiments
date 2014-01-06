using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;

namespace Umbraco.Core
{
	///<summary>
	/// Extension methods for dictionary & concurrentdictionary
	///</summary>
	internal static class DictionaryExtensions
	{
        /// <summary>
        /// Updates an item with the specified key with the specified value
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="updateFactory"></param>
        /// <returns></returns>
        /// <remarks>
        /// Taken from: http://stackoverflow.com/questions/12240219/is-there-a-way-to-use-concurrentdictionary-tryupdate-with-a-lambda-expression
        /// 
        /// If there is an item in the dictionary with the key, it will keep trying to update it until it can
        /// </remarks>
        public static bool TryUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, Func<TValue, TValue> updateFactory)
        {
            TValue curValue;
            while (dict.TryGetValue(key, out curValue))
            {
                if (dict.TryUpdate(key, updateFactory(curValue), curValue))
                    return true;
                //if we're looping either the key was removed by another thread, or another thread
                //changed the value, so we start again.
            }
            return false;
        }

        /// <summary>
        /// Updates an item with the specified key with the specified value
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="updateFactory"></param>
        /// <returns></returns>
        /// <remarks>
        /// Taken from: http://stackoverflow.com/questions/12240219/is-there-a-way-to-use-concurrentdictionary-tryupdate-with-a-lambda-expression
        /// 
        /// WARNING: If the value changes after we've retreived it, then the item will not be updated
        /// </remarks>
        public static bool TryUpdateOptimisitic<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, Func<TValue, TValue> updateFactory)
        {
            TValue curValue;
            if (!dict.TryGetValue(key, out curValue))
                return false;
            dict.TryUpdate(key, updateFactory(curValue), curValue);
            return true;//note we return true whether we succeed or not, see explanation below.
        }

		/// <summary>
		/// Converts a dictionary to another type by only using direct casting
		/// </summary>
		/// <typeparam name="TKeyOut"></typeparam>
		/// <typeparam name="TValOut"></typeparam>
		/// <param name="d"></param>
		/// <returns></returns>
		public static IDictionary<TKeyOut, TValOut> ConvertTo<TKeyOut, TValOut>(this IDictionary d)
		{
			var result = new Dictionary<TKeyOut, TValOut>();
			foreach (DictionaryEntry v in d)
			{
				result.Add((TKeyOut)v.Key, (TValOut)v.Value);
			}
			return result;
		}

		/// <summary>
		/// Converts a dictionary to another type using the specified converters
		/// </summary>
		/// <typeparam name="TKeyOut"></typeparam>
		/// <typeparam name="TValOut"></typeparam>
		/// <param name="d"></param>
		/// <param name="keyConverter"></param>
		/// <param name="valConverter"></param>
		/// <returns></returns>
		public static IDictionary<TKeyOut, TValOut> ConvertTo<TKeyOut, TValOut>(this IDictionary d, Func<object, TKeyOut> keyConverter, Func<object, TValOut> valConverter)
		{
			var result = new Dictionary<TKeyOut, TValOut>();
			foreach (DictionaryEntry v in d)
			{
				result.Add(keyConverter(v.Key), valConverter(v.Value));
			}
			return result;
		}

		/// <summary>
		/// Converts a dictionary to a NameValueCollection
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		public static NameValueCollection ToNameValueCollection(this IDictionary<string, string> d)
		{
			var n = new NameValueCollection();
			foreach (var i in d)
			{
				n.Add(i.Key, i.Value);
			}
			return n;
		}

		/// <summary>
		/// Converts a dictionary to a FormCollection
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		public static FormCollection ToFormCollection(this IDictionary<string, object> d)
		{
			var n = new FormCollection();
			foreach (var i in d)
			{
				n.Add(i.Key, Convert.ToString(i.Value));
			}
			return n;
		}

		/// <summary>
		/// Returns a new dictionary of this ... others merged leftward.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TK"></typeparam>
		/// <typeparam name="TV"></typeparam>
		/// <param name="me"></param>
		/// <param name="others"></param>
		/// <returns></returns>
		/// <remarks>
		/// Reference: http://stackoverflow.com/questions/294138/merging-dictionaries-in-c
		/// </remarks>
		public static T MergeLeft<T, TK, TV>(this T me, params IDictionary<TK, TV>[] others)
			where T : IDictionary<TK, TV>, new()
		{
			var newMap = new T();
			foreach (var p in (new List<IDictionary<TK, TV>> { me }).Concat(others).SelectMany(src => src))
			{
				newMap[p.Key] = p.Value;
			}
			return newMap;
		}

		/// <summary>
		/// Returns the value of the key value based on the key, if the key is not found, a null value is returned
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TVal">The type of the val.</typeparam>
		/// <param name="d">The d.</param>
		/// <param name="key">The key.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public static TVal GetValue<TKey, TVal>(this IDictionary<TKey, TVal> d, TKey key, TVal defaultValue = default(TVal))
		{
			if (d.ContainsKey(key))
			{
				return d[key];
			}
			return defaultValue;
		}

		/// <summary>
		/// Returns the value of the key value based on the key as it's string value, if the key is not found, then an empty string is returned
		/// </summary>
		/// <param name="d"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static string GetValueAsString<TKey, TVal>(this IDictionary<TKey, TVal> d, TKey key)
		{
			if (d.ContainsKey(key))
			{
				return d[key].ToString();
			}
			return String.Empty;
		}

		/// <summary>contains key ignore case.</summary>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <typeparam name="TValue">Value Type</typeparam>
		/// <returns>The contains key ignore case.</returns>
		public static bool ContainsKeyIgnoreCase<TValue>(this IDictionary<string, TValue> dictionary, string key)
		{
			return dictionary.Keys.Any(i => i.Equals(key, StringComparison.CurrentCultureIgnoreCase));
		}

		/// <summary>
		/// Converts a dictionary object to a query string representation such as:
		/// firstname=shannon&lastname=deminick
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		public static string ToQueryString(this IDictionary<string, object> d)
		{
			if (!d.Any()) return "";

			var builder = new StringBuilder();
			foreach (var i in d)
			{
				builder.Append(String.Format("{0}={1}&", HttpUtility.UrlEncode(i.Key), i.Value == null ? string.Empty : HttpUtility.UrlEncode(i.Value.ToString())));
			}
			return builder.ToString().TrimEnd('&');
		}

		/// <summary>The get entry ignore case.</summary>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <typeparam name="TValue">The type</typeparam>
		/// <returns>The entry</returns>
		public static TValue GetEntryIgnoreCase<TValue>(this IDictionary<string, TValue> dictionary, string key)
		{
			return dictionary.GetEntryIgnoreCase(key, default(TValue));
		}

		/// <summary>The get entry ignore case.</summary>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <typeparam name="TValue">The type</typeparam>
		/// <returns>The entry</returns>
		public static TValue GetEntryIgnoreCase<TValue>(this IDictionary<string, TValue> dictionary, string key, TValue defaultValue)
		{
			key = dictionary.Keys.Where(i => i.Equals(key, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

			return !key.IsNullOrWhiteSpace()
			       	? dictionary[key]
			       	: defaultValue;
		}
	}
}