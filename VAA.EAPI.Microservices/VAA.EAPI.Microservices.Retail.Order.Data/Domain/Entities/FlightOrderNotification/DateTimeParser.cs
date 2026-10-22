using System;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightOrderNotification
{
    public class DateTimeParser
    {
        public static DateTime Parse(string dateStr, string timeStr)
        {
            try
            {
                // Handle date formats like "24AUG"
                DateTime date;
                if (dateStr.Length == 5 && char.IsDigit(dateStr[0]) && char.IsDigit(dateStr[1]))
                {
                    var day = dateStr.Substring(0, 2);
                    var month = dateStr.Substring(2, 3);
                    var year = DateTime.Now.Year;
                    date = DateTime.TryParse($"{day} {month} {year}", out var parsedDate) ? parsedDate : DateTime.MinValue;
                    if (date < DateTime.Now.Date) date = date.AddYears(1);
                }
                else
                {
                    date = DateTime.TryParse(dateStr, out var d) ? d : DateTime.MinValue;
                }

                // Handle time formats like "0920A" or "0310P"
                TimeSpan time;
                if (timeStr.Length == 5 && (timeStr.EndsWith("A") || timeStr.EndsWith("P")))
                {
                    var timeOnly = timeStr.Substring(0, 4);
                    var amPm = timeStr.Substring(4, 1);
                    if (int.TryParse(timeOnly.Substring(0, 2), out var hours) && int.TryParse(timeOnly.Substring(2, 2), out var minutes))
                    {
                        if (amPm == "P" && hours != 12) hours += 12;
                        if (amPm == "A" && hours == 12) hours = 0;
                        time = new TimeSpan(hours, minutes, 0);
                    }
                    else time = TimeSpan.Zero;
                }
                else
                {
                    time = TimeSpan.TryParse(timeStr, out var t) ? t : TimeSpan.Zero;
                }

                return date.Add(time);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }
}