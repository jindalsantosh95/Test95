using System.Text.RegularExpressions;

namespace VAA.EAPI.Microservices.Retail.Holidays.Helpers
{

    public static class StringExtensions
    {
        public static string NormalizeName(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            // Trim and replace multiple whitespace characters with a single space
            return Regex.Replace(value.Trim(), @"\s+", " ");
        }
    }
}
