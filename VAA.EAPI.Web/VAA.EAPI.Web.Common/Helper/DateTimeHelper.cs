using System.Globalization;

namespace VAA.EAPI.Web.Common.Helper
{
    public static class DateTimeHelper
    {
        public static string ConvertToUtc(string dateString, string outputDateFormat, string acceptedDateFormat)
        {
            return DateTime.TryParseExact(dateString, acceptedDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result)
                ? result.ToUniversalTime().ToString("yyyy'-'MM'-'ddTHH':'mm':'ss'Z'")
                : null;
        }

        public static DateTime GetLastDayOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
        }

        public static DateTime? ConvertDateTimeFormat(string dateString, string outputDateFormat, string acceptedDateFormat)
        {
            return DateTime.TryParseExact(dateString, outputDateFormat, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out DateTime result) ? result : null;
        }
        public static string ConvertDateToString(string dateString, string acceptedDateFormat, string outputDateFormat)
        {
            return DateTime.TryParseExact(dateString, acceptedDateFormat, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out DateTime result) ? result.ToString(outputDateFormat) : string.Empty;
        }


        /// <summary>
        /// Provide Unix time value for valid date string
        /// </summary>
        /// <param name="dateString"></param>
        /// <returns></returns>
        public static long ConvertDateTimeToUnix(string dateString)
        {
            DateTime dateValue;
            if (DateTime.TryParse(dateString, out dateValue))
            {
                DateTimeOffset dateTime = DateTime.SpecifyKind(dateValue, DateTimeKind.Local);
                return dateTime.ToUnixTimeSeconds();
            }

            return 0;
        }

        /// <summary>
        /// Function to get datetime object from unixtimestamp
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(object unixTimeStamp)
        {
            DateTime unixStart = new(1970, 1, 1, 0, 0, 0, 0);
            long unixTimeStampInTicks = (long)(Convert.ToDouble(unixTimeStamp) * TimeSpan.TicksPerSecond);
            return new DateTime(unixStart.Ticks + unixTimeStampInTicks);
        }

        /// <summary>
        /// Calculate Age of person
        /// </summary>
        /// <param name="birthDate">Person's date of birth</param>
        /// <param name="referenceDate">Age on a specific date (e.g. Flight Depaeture Date). If not specified, DateTime.UtcNow will be used.</param>
        /// <returns>Age, in completed years</returns>
        public static int CalculateAge(DateTime birthDate, DateTime? referenceDate = null)
        {
            DateTime referenceDateUpdated = referenceDate ?? DateTime.UtcNow;
            int age = referenceDateUpdated.Year - birthDate.Year;

            // For leap years 
            if (birthDate > referenceDateUpdated.AddYears(-age))
                age--;

            return age;
        }

        public static DateTime? Air4PNRDateTimeConversion(string dateMonthYear, string time)
        {


            if (!string.IsNullOrEmpty(dateMonthYear) && !string.IsNullOrEmpty(time))
            {
                if (DateTime.TryParse(dateMonthYear[..5] + "20" + dateMonthYear[5..] + " " + time.Insert(2, ":") + "M", out DateTime outDateTime))
                {
                    return outDateTime;
                }
            }
            return null;
        }
    }
}


