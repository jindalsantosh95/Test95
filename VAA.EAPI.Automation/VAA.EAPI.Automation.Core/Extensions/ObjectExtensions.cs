namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static string EscapeCsvValue(this object source)
        {
            if (source is null)
                return string.Empty;

            string value = source.ToString() ?? string.Empty;

            // Escape internal quotes
            value = value.Replace("\"", "\"\"");

            // Wrap in quotes if the value contains a comma, semicolon, or newline
            if (value.Contains(',') || value.Contains(';') || value.Contains('\n'))
            {
                return $"\"{value}\"";
            }

            return value;
        }
    }
}
