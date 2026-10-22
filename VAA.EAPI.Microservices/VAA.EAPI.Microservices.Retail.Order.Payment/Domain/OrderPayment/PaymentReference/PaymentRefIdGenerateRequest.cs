using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentReference
{
    public class PaymentRefIdGenerateRequest
    {
        public Pos Pos { get; set; }
        public Payment paymentAmt { get; set; }
        public LoyaltyMember LoyaltyMemberDetails { get; set; }
        public IEnumerable<Offer> Selectedoffers { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public IEnumerable<Segment> FlightSegments { get; set; }
        public IEnumerable<Journey> Journey { get; set; }
        public AdditionalConfigParams AdditionalConfigParams { get; set; }
        public IEnumerable<SeatList> SeatDetails { get; set; }
        public IEnumerable<Bags>? BagsDetails { get; set; }
        public InsuranceDetails InsuranceDetails { get; set; }
        public string TripType { get; set; }
        public bool IsBagsPayment { get; set; }
        public string? RecordLocator { get; set; }
    }
}