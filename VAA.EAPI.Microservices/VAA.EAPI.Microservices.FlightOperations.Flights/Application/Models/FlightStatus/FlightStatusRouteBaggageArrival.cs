using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusRouteBaggageArrival
    {
        public static explicit operator FlightStatusRouteBaggageArrival?(DeltaBaggageArrival? source) => source is null
            ? null
            : new FlightStatusRouteBaggageArrival { CarouselNumber = source.CarouselNumber };

        public string? CarouselNumber { get; set; }
    }
}
