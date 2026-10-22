using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    public class TravelDuration
    {
        public string? DurationType { get; set; }
        public string? Duration { get; set; }

        public static explicit operator TravelDuration(TravelDurationInfo source)
        {
            if (source == null) return default;

            return new()
            {
                Duration = source.Duration,
                DurationType = source.DurationType,
            };
        }
    }
}