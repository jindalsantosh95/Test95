using System;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.PaymentReference
{
    public class FlightLeg
    {
        public DateTime ArrivalTime { get; set; }
        public string BookingClass { get; set; }
        public DateTime DepartureTime { get; set; }
        public string DestinationAirportCode { get; set; }
        public int DestinationTimezone { get; set; }
        public string FlightNumber { get; set; }
        public string MarketingCarrierCode { get; set; }
        public string OperatingCarrierCode { get; set; }
        public string OriginAirportCode { get; set; }
        public string OriginTimezone { get; set; }
        public int DestinationCountryId { get; set; }
        public int OriginCountryId { get; set; }
    }

}