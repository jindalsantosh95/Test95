using System;
using System.Text;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Orchestration.RetailTherapy.Infrastructure.Models.FlightMs
{
    public sealed class Flight
    {
        public static explicit operator Flight(FlightDetail source) => new()
        {
            FlightNo = source.FlightNo,
            Departure = source.Departure,
            Arrival = source.Arrival,
            AirlineCode = source.Carrier,
            ScheduledDateOfDeparture = source.EstimatedDateOfDeparture,
            ScheduledDateTimeOfDeparture = $"{source.EstimatedDateOfDeparture:yyyy-MM-dd}T{source.ScheduledTimeOfDeparture:HH:mm}:00+000".Parse<DateTimeOffset>(),
            ScheduledDateOfArrival = source.EstimatedDateOfArrival,
            ScheduledDateTimeOfArrival = $"{source.EstimatedDateOfArrival:yyyy-MM-dd}T{source.ScheduledTimeOfArrival:HH:mm}:00+000".Parse<DateTime>(),
        };

        public int FlightNo { get; set; }
        public string Departure { get; set; }
        public string Arrival { get; set; }
        public string AirlineCode { get; set; }
        public string AirlineName { get; set; }
        public DateOnly ScheduledDateOfDeparture { get; set; }
        public DateTimeOffset ScheduledDateTimeOfDeparture { get; set; }
        public DateOnly ScheduledDateOfArrival { get; set; }
        public DateTime ScheduledDateTimeOfArrival { get; set; }
        public string ArrivalAirportName { get; set; }
        public string ArrivalCountryName { get; set; }
        public string ArrivalCityName { get; set; }
        public string DepartureAirportName { get; set; }
        public string DepartureCountryName { get; set; }
        public string DepartureCityName { get; set; }
        public string Direction { get; set; } = "D";

        public override string ToString()
        {
            StringBuilder stringBuilder = new();

            stringBuilder.AppendJoin(',', $"{AirlineCode}{FlightNo}".EscapeCsvValue(),
                                          AirlineCode.EscapeCsvValue(),
                                          AirlineName.EscapeCsvValue(),
                                          $"{ScheduledDateOfDeparture:yyyy-MM-dd}".EscapeCsvValue(),
                                          $"{ScheduledDateTimeOfDeparture:yyyy-MM-ddTHH:mm:sszzz}".EscapeCsvValue(),
                                          $"{ScheduledDateOfArrival:yyyy-MM-dd}".EscapeCsvValue(),
                                          $"{ScheduledDateTimeOfArrival:yyyy-MM-ddTHH:mm:ssK}".EscapeCsvValue(),
                                          Arrival.EscapeCsvValue(),
                                          ArrivalAirportName.EscapeCsvValue(),
                                          ArrivalCountryName.EscapeCsvValue(),
                                          ArrivalCityName.EscapeCsvValue(),
                                          Departure.EscapeCsvValue(),
                                          DepartureAirportName.EscapeCsvValue(),
                                          DepartureCountryName.EscapeCsvValue(),
                                          DepartureCityName.EscapeCsvValue(),
                                          Direction.EscapeCsvValue());

            return stringBuilder.ToString();
        }
    }
}
