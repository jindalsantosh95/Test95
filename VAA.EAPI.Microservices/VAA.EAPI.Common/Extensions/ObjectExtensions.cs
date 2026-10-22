using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Common.Extensions
{
    public static class ObjectExtensions
    {
        private const string CsvValueSafeCheckPattern = @"^(?:[^,""\r\n]*|""(?:[^""]|"""")*"")$";

        public static string EscapeCsvValue(this object source)
        {
            if (source is null) return string.Empty;
            if (source is not string) return source.ToString();

            string value = source.ToString();

            if (RegexCompiled.IsMatch(value, CsvValueSafeCheckPattern)) return value;

            value = value.Replace("\"", "\"\"");

            return $"\"{value}\"";
        }

        public static bool PropertyIs(this object source, string name, string value)
        {
            if (source is null) return false;

            var type = source.GetType();

            var property = type.GetProperty(name);

            if (property == null) return false;

            return property.GetValue(source, null)?.ToString().Is(value) ?? false;
        }
    }
}
