using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusRouteSummary
    {
        public static explicit operator FlightStatusRouteSummary?(DeltaRouteSummary? source) => source is null
            ? null
            : new FlightStatusRouteSummary
            {
                DefaultRouteResultsStopsIndicator = source.DefaultRouteResultsStopsIndicator,
                MultiStop = source.MultiStop,
                NonStop = source.NonStop,
                OneStop = source.OneStop,
                TimeRangeOne = source.TimeRangeOne,
                TimeRangeTwo = source.TimeRangeTwo,
                TimeRangeThree = source.TimeRangeThree
            };

        public string? DefaultRouteResultsStopsIndicator { get; set; }
        public bool? MultiStop { get; set; }
        public bool? NonStop { get; set; }
        public bool? OneStop { get; set; }
        public bool? TimeRangeOne { get; set; }
        public bool? TimeRangeTwo { get; set; }
        public bool? TimeRangeThree { get; set; }
    }
}
