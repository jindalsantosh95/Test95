using System;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Helper
{
    public static class CheckDateFormat
    {
        public static bool IsValidDate(string date)
        {
            string[] acceptedFormats = new[] { "yyyy-MM-dd" };
            if (DateTime.TryParseExact(date, acceptedFormats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime fromDateValue))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
