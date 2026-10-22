using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusRouteLeg
    {
        public static explicit operator FlightStatusRouteLeg?(DeltaLeg? source) => source is null
            ? null
            : new FlightStatusRouteLeg
            {
                Id = source.Id,
                FlightNumber = source.FlightNumber,
                DepartureAirport = (FlightStatusAirport?)source.DepartureAirport,
                ArrivalAirport = (FlightStatusAirport?)source.ArrivalAirport,
                MarketingAirline = (FlightStatusRouteAirline?)source.MarketingAirline,
                OperatingAirline = (FlightStatusRouteAirline?)source.OperatingAirline,
                Aircraft = (FlightStatusRouteAircraft?)source.Aircraft,
                FlightTypeCode = source.FlightTypeCode,
                ReturnedIndicator = source.ReturnedIndicator,
                ShipNumber = source.ShipNumber,
                TakeOffNumber = source.TakeOffNumber,
                Usable = source.Usable,
                Status = (FlightStatusRouteStatus?)source.Status,
                FlightPosition = (FlightStatusPosition?)source.FlightPosition
            };

        public string? Id { get; set; }
        public string? FlightNumber { get; set; }
        public FlightStatusAirport? DepartureAirport { get; set; }
        public FlightStatusAirport? ArrivalAirport { get; set; }
        public FlightStatusRouteAirline? MarketingAirline { get; set; }
        public FlightStatusRouteAirline? OperatingAirline { get; set; }
        public FlightStatusRouteAircraft? Aircraft { get; set; }
        public string? FlightTypeCode { get; set; }
        public bool? ReturnedIndicator { get; set; }
        public string? ShipNumber { get; set; }
        public string? TakeOffNumber { get; set; }
        public bool? Usable { get; set; }
        public FlightStatusRouteStatus? Status { get; set; }
        public FlightStatusPosition? FlightPosition { get; set; }
    }
}
