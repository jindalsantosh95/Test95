using NodaTime;
using NodaTime.Text;
using System.Globalization;
using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Common.Helper;

public static class DateTimeHelper
{
    public static LocalDateTime? ParseLocalDateTime(string source)
    {
        if (source.IsNullOrEmpty()) return null;
        var pattern = LocalDateTimePattern.Create("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
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

    /// <summary>
    /// Take the colon Duration for e.g. 08:00 and convert into Iso format 
    /// </summary>
    /// <param name="colonDuration"></param>
    /// <returns>IsoFormat Duration</returns>
    public static string ConvertColonDurationToIso(string colonDuration)
    {
        var duration = colonDuration.Split(':');

        var hours = duration[0] == "00" ? string.Empty : $"{duration[0]}H";
        var minutes = duration[1] == "00" ? string.Empty : $"{duration[1]}M";

        return $"PT{hours}{minutes}";
    }
}
