namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Helper;

public static class BritishTimeZoneHelper
{
    private static readonly TimeZoneInfo _ukTimeZone = ResolveUkTimeZone();

    public static DateTimeOffset BuildStoreLocalDateTimeOffset(DateTime date, TimeOnly time)
    {
        var localDateTime = DateTime.SpecifyKind(date.Date.Add(time.ToTimeSpan()), DateTimeKind.Unspecified);
        var offset = _ukTimeZone.GetUtcOffset(localDateTime);

        return new DateTimeOffset(localDateTime, offset);
    }

    public static DateTimeOffset ConvertToUkLocal(DateTimeOffset dateTimeOffset)
    {
        return TimeZoneInfo.ConvertTime(dateTimeOffset, _ukTimeZone);
    }

    public static DateTimeOffset InterpretDatabaseClockAsUkLocal(DateTimeOffset databaseDateTimeOffset)
    {
        return BuildStoreLocalDateTimeOffset(
            databaseDateTimeOffset.DateTime,
            TimeOnly.FromDateTime(databaseDateTimeOffset.DateTime));
    }

    private static TimeZoneInfo ResolveUkTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
        }
        catch (TimeZoneNotFoundException)
        {
        }
        catch (InvalidTimeZoneException)
        {
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
        }
        catch (InvalidTimeZoneException)
        {
        }

        return TimeZoneInfo.Utc;
    }
}
