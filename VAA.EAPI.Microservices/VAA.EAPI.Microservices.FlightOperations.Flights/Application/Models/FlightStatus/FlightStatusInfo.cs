using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusInfo
    {
        public static explicit operator FlightStatusInfo?(DeltaStatus? source)
        {
            if (source is null)
                return null;

            return new FlightStatusInfo
            {
                Description = source.StatusDescription,
                Color = source.StatusColor,
                Message = source.CancelReasonMessage ?? source.DelayReasonMessage
            };
        }

        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Message { get; set; }
    }
}
