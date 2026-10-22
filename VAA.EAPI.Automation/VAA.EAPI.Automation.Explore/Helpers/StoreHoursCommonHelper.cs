using System.Globalization;
using System.Text.RegularExpressions;

namespace VAA.EAPI.Automation.Explore.Helpers
{
    public static class StoreHoursCommonHelper
    {
        public static string ConvertApiSpecialDateToDayMonth(string apiDate)
        {
            if (string.IsNullOrWhiteSpace(apiDate))
                return string.Empty;

            // Remove ordinal suffixes (st, nd, rd, th)
            string cleaned = Regex.Replace(apiDate, "(st|nd|rd|th)", "");

            string[] parts = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                return string.Empty;

            int day = int.Parse(parts[0]);

            string monthName = parts[1];

            if (!DateTime.TryParseExact(monthName, "MMMM", CultureInfo.InvariantCulture,DateTimeStyles.None, out DateTime parsedMonth))
            {
                return string.Empty;
            }

            int month = parsedMonth.Month;

            return $"{day:D2}-{month:D2}";
        }

        public static string ConvertDbDateToDayMonth(string dbDate)
        {
            if (string.IsNullOrWhiteSpace(dbDate))
                return string.Empty;

            if (!DateTime.TryParse(dbDate, CultureInfo.InvariantCulture,DateTimeStyles.None, out DateTime dt))
            {
                return string.Empty;
            }

            return dt.ToString("dd-MM");
        }
    }
}