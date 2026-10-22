using System;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.Flights
{
    public class Flight
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateOnly? DepartureDate { get; set; }
        public string DepartureTime { get; set; }
        public DateOnly? ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public string OperatingBookingClass { get; set; }
        public string MarketingBookingClass { get; set; }
        public string OperatingCarrierCode { get; set; }
        public string MarketingCarrierCode { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string MarketingFlightNumber { get; set; }
    }
}
