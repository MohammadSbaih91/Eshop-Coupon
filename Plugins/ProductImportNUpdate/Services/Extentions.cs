using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProductImportNUpdate.Services
{
    public class NullableDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TValue : class
    {
        public NullableDictionary()
        { }
        public NullableDictionary(IEqualityComparer<TKey> e) : base(e) { }
        public new TValue this[TKey key]
        {
            get => !ContainsKey(key) ? null : base[key];
            set
            {
                if (!ContainsKey(key))
                    Add(key, value);
                else
                    base[key] = value;
            }
        }
    }

    public static class MiscExtensions
    {
        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source, int n)
        {
            var enumerable = source as T[] ?? source.ToArray();
            return enumerable.Take(Math.Max(0, enumerable.Count() - n));
        }

        public static IEnumerable<string> UnEscape(this string source, char separator, string escape = "\"")
        {
            var fields = new List<string>();
            try
            {
                while (!string.IsNullOrEmpty(source))
                {
                    var firstIndexOfComma = source.IndexOf(separator);
                    var firstIndexOfQuot = source?.IndexOf(escape)??0;
                    if ((firstIndexOfComma < firstIndexOfQuot && !source.StartsWith(escape)) || (firstIndexOfComma == -1))
                    {
                        var value = source.Split(separator)?.FirstOrDefault()?
                                          .Replace(escape, "").Replace("\r", "");
                        fields.Add(value);
                        source = source.Substring(firstIndexOfComma + 1);
                        if (firstIndexOfComma == -1)
                        {
                            source = string.Empty;
                        }
                    }
                    else
                    {
                        var endIndexOfQuot = source?.IndexOf(escape + separator)??0;
                        var value = source.Substring(1, endIndexOfQuot - 1)
                                          .Replace(escape, "").Replace("\r", "");
                        fields.Add(value);
                        source = source.Substring(endIndexOfQuot + 2);
                    }

                }
            }
            catch (Exception e)
            {
                source = source?.Length > 50 ? $"{source.Substring(0, 49)}..." : source;
                throw new Exception($"{nameof(ProductImportNUpdate)} - unable to un-escape line from :- {source}", e);
            }
            return fields;
        }

        public static T ToValue<T>(this string value)
        {
            try
            {
                var numOnly = Regex.Replace(value, "[^0-9.]", "");
                return string.IsNullOrWhiteSpace(numOnly) ? default(T) : (T)Convert.ChangeType(numOnly, typeof(T));
            }
            catch (Exception) { return default(T); }
        }

        public static T ToTypeOrDefault<T>(this object value)
        {
            if (value == null) return default(T);
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception) { return default(T); }
        }
    }
    
}
