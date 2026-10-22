using System;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Common
{
    public class TimeLimitMapper
    {
        public static DateTime GetTTL(TimeSpan offSet, DateTime departureDate)
        {
            var bookingDateUTC = DateTime.UtcNow;
            var departureDateUTC = departureDate.Add(-offSet);

            var daysDifference = (departureDateUTC - bookingDateUTC).Days;
            var configDaysDifference = ConfigReader.Get<TimeSpan>(Constants.Environment.DaysDifferenceDepartureAndBooking).Days;

            if (daysDifference > configDaysDifference)
            {
                var timeLimitBeforeDeparture = departureDateUTC.Add(-ConfigReader.Get<TimeSpan>(Constants.Environment.ThresholdDaysBeforeDeparture));
                var timeLimitAfterBooking = bookingDateUTC.Add(ConfigReader.Get<TimeSpan>(Constants.Environment.ThresholdDaysAfterBooking));

                return GetLatestTimeLimit(timeLimitBeforeDeparture, timeLimitAfterBooking);
            }

            if (daysDifference <= configDaysDifference && (departureDateUTC.Date - bookingDateUTC.Date).Days > 1)
            {
                return new DateTime(bookingDateUTC.Year, bookingDateUTC.Month, bookingDateUTC.Day, 23, 59, 00, DateTimeKind.Utc);
            }

            var thresholdHours = ConfigReader.Get<TimeSpan>(Constants.Environment.HoursDifferenceDepartureAndBooking).TotalHours;
            if ((departureDateUTC - bookingDateUTC).TotalHours > thresholdHours)
            {
                var timeLimit = new DateTime(bookingDateUTC.Year, bookingDateUTC.Month, bookingDateUTC.Day, 23, 59, 00, DateTimeKind.Utc);
                return (departureDateUTC - timeLimit).TotalHours > thresholdHours
                    ? timeLimit
                    : departureDateUTC.AddHours(-thresholdHours);
            }

            return DateTime.MinValue;
        }

        private static DateTime GetLatestTimeLimit(DateTime timeLimitBeforeDeparture, DateTime timeLimitAfterBooking)
        {
            var latestTimeLimit = timeLimitAfterBooking < timeLimitBeforeDeparture ? timeLimitAfterBooking : timeLimitBeforeDeparture;
            return new DateTime(latestTimeLimit.Year, latestTimeLimit.Month, latestTimeLimit.Day, 23, 59, 00, DateTimeKind.Utc);
        }
    }
}