using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Helper.Enrolment
{
    public static class CheckDateFormat
    {
        public static bool IsValidDate(string date)
        {
            return DateTime.TryParseExact(date, new[] { "yyyy-MM-dd" },
                                         System.Globalization.CultureInfo.InvariantCulture,
                                         System.Globalization.DateTimeStyles.None, out _);
        }
    }
}
