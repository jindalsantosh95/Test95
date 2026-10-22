using System.Data;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class DataRowExtensions
    {
        public static T Get<T>(this DataRow source, string column, T @default = default)
        {
            @default ??= default;

            if (source.Table.Columns.Contains(column))
                if ((source[column]?.ToString() ?? string.Empty).TryParse(out T result))
                    return result;

            return @default;
        }
    }
}