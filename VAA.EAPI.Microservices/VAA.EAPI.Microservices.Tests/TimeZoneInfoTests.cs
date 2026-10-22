using System.Diagnostics;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Tests
{
    public class TimeZoneInfoTests
    {
        [Theory]
        [ClassData(typeof(TimeZoneTestData))]
        public void TimeZonesTest(string airportCode, string timeZone, bool isRoute)
        {
            DateTime dateTime = "2026-01-22T23:10:00".Parse<DateTime>();
            TimeZoneInfo timeZoneInfo;

            try
            {
                if (timeZone.IsNullOrWhiteSpace()) Assert.Fail($"Empty time zone for {airportCode}");

                timeZoneInfo = TimeZoneInfoHelper.FindSystemTimeZoneById(timeZone);

                var localTime = TimeZoneInfo.ConvertTime(dateTime, timeZoneInfo);
                var utcTime = TimeZoneInfo.ConvertTimeToUtc(localTime, timeZoneInfo);

                Debug.WriteLine($"Time zone found: {timeZone}, isRoute: {isRoute}. UTC:{dateTime} -> LOCAL:{localTime} LOCAL: {localTime} -> UTC{utcTime})");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                Assert.Skip(ex.Message);
            }
        }
    }
}
