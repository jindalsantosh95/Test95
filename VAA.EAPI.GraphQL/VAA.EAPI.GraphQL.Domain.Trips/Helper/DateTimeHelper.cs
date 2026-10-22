using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.Trips.Helper
{
    public static class DateTimeHelper
    {
        public static LocalDateTime? ToLocal(DateTime? d) => d.HasValue ? LocalDateTime.FromDateTime(d.Value) : null;

        public static TimeZoneInfo? TryFindSystemTimeZoneById(string? timeZoneId)
        {
            if (string.IsNullOrWhiteSpace(timeZoneId))
            {
                return null;
            }
            try
            {
                return VAA.EAPI.GraphQL.Common.Helper.TimeZoneInfoHelper.FindSystemTimeZoneById(timeZoneId);
            }
            catch
            {
                return null;
            }
        }

    }
}
