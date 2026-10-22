using System;
namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.PassengerRetrieval
{
    public class PassengerRetrievalQuery
    {
        public string FlightCarrier { get; set; }
        public string FlightNumber { get; set; }
        public string DepartureAirport { get; set; }
        public string ArrivalAirport { get; set; }
        public DateTime DepartureDate { get; set; }
        public string OperatingCarrier { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string MarketingCarrier { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string BookingClass { get; set; }
        public string CabinClass { get; set; }
    }
}
