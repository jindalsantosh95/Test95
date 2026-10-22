using System.Text;
using Microsoft.Azure.Documents;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder MakeInterpolationSafe(this StringBuilder source) => source.Replace("{", "{{").Replace("}", "}}");

        public static void AppendLineIf(this StringBuilder source, Func<StringBuilder, bool> predicate, string value)
        {
            if (predicate(source))
                source.AppendLine(value);
        }
    }
}
