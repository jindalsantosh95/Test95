using System;
using System.Globalization;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Helper
{
    public static class Utility
    {
        public static string ParseDateZ(string dateString, string outputDateFormat, string acceptedDateFormat)
        {
            if (DateTime.TryParseExact(dateString, acceptedDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result.ToString(outputDateFormat) + "Z";
            }

            return null;
        }

        public static string ParseDateTimeZ(DateTime date, string outputDateFormat)
        {
            if (date.Year == 1)
                return null;
            else
                return date.ToString(outputDateFormat) + "Z";
        }
    }
}