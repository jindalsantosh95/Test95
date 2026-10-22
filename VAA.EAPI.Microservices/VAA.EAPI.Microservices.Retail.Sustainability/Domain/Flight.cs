using System;
namespace VAA.EAPI.Microservices.Retail.Sustainability.Domain
{
    public sealed class Flight
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string FlightNumber { get; set; }
        public string TravelClass { get; set; }
        public string RequestId { get; set; }
        public DateTime Date { get; set; }

        public static explicit operator Flight(FlightDetail source)
        {
            if (source == null) return null;

            return new()
            {
                Date = source.FlightDate,
                Destination = source.DestinationAirport,
                Origin = source.OriginAirport,
                FlightNumber = source.FlightNumber,
                TravelClass = source.Cabin.ToString()
            };
        }
    }
}
