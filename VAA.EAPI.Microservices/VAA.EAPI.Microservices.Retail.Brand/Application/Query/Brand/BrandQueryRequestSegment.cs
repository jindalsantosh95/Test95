using System;
using System.Linq;

namespace VAA.EAPI.Microservices.Retail.Brand.Application.Query.Brand
{
    public class BrandQueryRequestSegment
    {
        public string Carrier { get; set; }
        public int FlightNumber { get; set; }
        public DateTime DepartureDate { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public int Cabin { get; set; }
        public string FareBasis { get; set; }
        public string TicketDesignator { get; set; }
        public string OperatingCarrier { get; set; }
        public string FareFamily { get; set; }

        public static explicit operator BrandQueryRequestSegment(BrandQueryRequest source)
        {
            if (source == null)
                return null;

            return new()
            {
                 Carrier = source.Itinerary.First().Segments.First().Carrier,
                 FlightNumber = source.Itinerary.First().Segments.First().FlightNumber,
                 DepartureDate = source.Itinerary.First().Segments.First().DepartureDate,
                 Origin= source.Itinerary.First().Segments.First().Origin,
                 Destination = source.Itinerary.First().Segments.First().Destination,
                 Cabin = source.Itinerary.First().Segments.First().Cabin,
                 FareBasis = source.Itinerary.First().Segments.First().FareBasis,
                 TicketDesignator = source.Itinerary.First().Segments.First().TicketDesignator,
                 OperatingCarrier = source.Itinerary.First().Segments.First().OperatingCarrier,
                 FareFamily = source.Itinerary.First().Segments.First().FareFamily
            };
        }

    }
}
