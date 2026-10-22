using NodaTime;
using NodaTime.Text;
using System;
using System.Globalization;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Common.Helpers
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
        ///  Returns a nullable DateTime        
        ///  <param dateMonthYear="string YYYYDDMMM"></param>
        /// <param time="string 12 hour format hhmmA hhmmP"></param>
        /// </summary>
        /// <returns></returns>
        public static DateTime? Air4ArrivalDateFormatConversion(string dateMonthYear, string time)
        {
            if (!string.IsNullOrEmpty(dateMonthYear) && !string.IsNullOrEmpty(time))
            {
                time = time.Contains(Constants.Application.DeltaNoonChar) ? time.Replace(Constants.Application.DeltaNoonChar, Constants.Application.DeltaPMChar) : time; // This is a Edge case scenario when we get Arrival Time as "1200N" from Delta Response

                if (DateTime.TryParse(dateMonthYear[4..] + dateMonthYear.Substring(0, 4) + " " + time.Insert(2, ":") + "M", out DateTime outDateTime))
                {
                    return outDateTime;
                }
            }
            return null;
        }


        /// <summary>
        ///  Returns a nullable DateTime        
        ///  <param dateMonth="string DDMMM"></param>
        /// <param year="string YY"></param>
        /// <param time="string 12 hour format hhmmA hhmmP"></param>
        /// </summary>
        /// <returns></returns>
        public static DateTime? Air4DepartDateFormatConversion(string dateMonth, string year, string time)
        {
            if (!string.IsNullOrEmpty(dateMonth) && !string.IsNullOrEmpty(year) && !string.IsNullOrEmpty(time))
            {
                time = time.Contains(Constants.Application.DeltaNoonChar) ? time.Replace(Constants.Application.DeltaNoonChar, Constants.Application.DeltaPMChar) : time; // This is a Edge case scenario when we get Departure Time as "1200N" from Delta Response

                if (DateTime.TryParse(dateMonth + year + " " + (string.IsNullOrEmpty(time) ? null : time.Insert(2, ":") + "M"), out DateTime outDateTime))
                {
                    return outDateTime;
                }
            }
            return null;
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
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0);
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
                if (DateTime.TryParse(dateMonthYear[..5] + "20" + dateMonthYear[5..] + " " + (time.Length == 5 ? time.Insert(2, ":") : time.Insert(1, ":")) + "M", out DateTime outDateTime))
                {
                    return outDateTime;
                }
            }

            return null;
        }

        public static DateTime? ParseDateFormat(string inDate)
        {
            if (!DateTime.TryParse(inDate, out DateTime outDate))
            {
                DateTime.TryParseExact(inDate, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out outDate);
            }
            return outDate;
        }

        public static LocalDate? ParseLocalIsoDate(string? source)
        {
            if (source.IsNullOrEmpty()) return null;
            var pattern = LocalDateTimePattern.Create("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return pattern.Parse(source).Value.Date;
        }

        public static LocalDateTime? ParseLocalIsoDateTime(string? source)
        {
            if (source.IsNullOrEmpty()) return null;
            var pattern = LocalDateTimePattern.Create("yyyy-MM-dd\\THH:mm:ss", CultureInfo.InvariantCulture);
            // Substring to remove timezone
            return pattern.Parse(source.Substring(0, 19)).Value;
        }

        public static LocalDateTime? ParseLocalDateTime(string source)
        {
            if (source.IsNullOrEmpty()) return null;
            var pattern = LocalDateTimePattern.Create("dd MMM yyyy hh:mmtt", CultureInfo.InvariantCulture);
            return pattern.Parse(source).Value;
        }

        public static LocalDate? ParseVaaBookingDate(string source)
        {
            if (source.IsNullOrEmpty()) return null;
            var pattern = LocalDatePattern.Create("ddMMMyy", CultureInfo.InvariantCulture);
            return pattern.Parse(source).Value;
        }

        public static LocalTime? ParseVaaBookingTime(string source)
        {
            if (source.IsNullOrEmpty()) return null;
            source = source.Replace("N", "P").Replace("M", "A");
            var pattern = LocalTimePattern.Create("hhmmt", CultureInfo.InvariantCulture);
            return pattern.Parse(source).Value;
        }
        public static LocalTime? Air4PNRLocalTimeConversion(string time)
        {
            if (time.IsNullOrWhiteSpace()) return null;

            if (time.Contains('P') || time.Contains('A'))
            {
                var designator = time[^1];
                var value = int.Parse(time[..^1]);

                int hour = value / 100;
                int minute = value % 100;

                if (designator == 'P' && hour != 12) hour += 12;
                if (designator == 'A' && hour == 12) hour = 0;

                return new LocalTime(hour, minute);
            }

            var timevalue = int.Parse(time);
            return new LocalTime(timevalue / 100, timevalue % 100);

        }
    }
}


