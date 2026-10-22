using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.PaymentReference
{
    public class AirlineData
    {
        public Fare Fare { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public Product Product { get; set; }
        public Profile Profile { get; set; }
        public string ShopType { get; set; }
        public string TicketIssueDate { get; set; }
        public string TripType { get; set; }
        public IEnumerable<Trip> Trips { get; set; }
        public IEnumerable<SeatList> SeatList { get; set; }
        public IEnumerable<Bags>? Bags { get; set; }
    }
}