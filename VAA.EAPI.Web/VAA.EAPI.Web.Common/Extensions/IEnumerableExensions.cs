using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Web.Common.Extensions
{
    public static class IEnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source) => !(source?.Any() ?? false);
        public static IEnumerable<T> IfIsNullOrEmpty<T>(this IEnumerable<T> source, params T[] @default) => source.IsNullOrEmpty() ? @default : source;
    }
}
