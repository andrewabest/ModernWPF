using System;
using System.Collections.Generic;
using System.Linq;

namespace ModernWPF.Client.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool None<T>(this IEnumerable<T> collection)
        {
            return !collection.Any();
        }

        public static bool None<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            return !collection.Any(predicate);
        }
    }
}