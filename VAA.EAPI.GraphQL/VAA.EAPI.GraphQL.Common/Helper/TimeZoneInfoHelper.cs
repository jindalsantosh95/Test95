using System;

namespace VAA.EAPI.GraphQL.Common.Helper
{
    public static class TimeZoneInfoHelper
    {
        public static TimeZoneInfo FindSystemTimeZoneById(string timeZoneId)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException ex)
            {
                // Attempt to convert an IANA time zone ID to a Windows time zone ID (primarily for Windows hosts).
                if (TimeZoneInfo.TryConvertIanaIdToWindowsId(timeZoneId, out var windowsTimeZoneId))
                {
                    try
                    {
                        return TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneId);
                    }
                    catch (TimeZoneNotFoundException ex1)
                    {
                        throw new TimeZoneNotFoundException($"Time zone not found: {timeZoneId}. Exception: {ex1.Message}", ex1);
                    }
                    catch (InvalidTimeZoneException ex2)
                    {
                        throw new InvalidTimeZoneException($"Invalid time zone: {timeZoneId}. Exception: {ex2.Message}", ex2);
                    }
                }
                else
                {
                    throw new TimeZoneNotFoundException($"Time zone not found: {timeZoneId}.", ex);
                }
            }
            catch (InvalidTimeZoneException ex)
            {
                throw new InvalidTimeZoneException($"Invalid time zone: {timeZoneId}. Exception: {ex.Message}", ex);
            }
        }
    }
}
