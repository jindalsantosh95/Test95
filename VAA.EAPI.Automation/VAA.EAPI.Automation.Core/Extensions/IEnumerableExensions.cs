using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class IEnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source) => !(source?.Any() ?? false);
        public static IEnumerable<T> IfIsNullOrEmpty<T>(this IEnumerable<T> source, params T[] @default) => source.IsNullOrEmpty() ? @default : source;

        public static string ConvertToCsv<T>(this IEnumerable<T> source) => ConvertToCsv(source, null);
        public static string ConvertToCsv<T>(this IEnumerable<T> source, string headerLine)
        {
            StringBuilder stringBuilder = new();

            if (!headerLine.IsNullOrWhiteSpace())
                stringBuilder.AppendLine(headerLine);

            foreach (string line in source.Select(x => x.ToString()))
                stringBuilder.AppendLine(line);

            return stringBuilder.ToString();
        }
    }
}
