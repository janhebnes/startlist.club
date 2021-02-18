using System.Collections.Generic;
using System.Linq;

namespace FlightJournal.Web.Extensions
{
    public static class EnumerableExtensions
    {

        public static T MaxOrDefault<T>(this IEnumerable<T> x, T defaultValue = default )
        {
            return x.IsNullOrEmpty() ? defaultValue : x.Max();
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> x)
        {
            return x == null || !x.Any();
        }
        public static bool SafeContains<T>(this IEnumerable<T> x, T target)
        {
            return !x.IsNullOrEmpty() && x.Contains(target);
        }
    }
}